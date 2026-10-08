"""Verifica o contrato HTTP com os pesos reais do projeto."""
import base64
import io
import json
from pathlib import Path
import unittest
from unittest.mock import patch

import numpy as np
from fastapi import UploadFile
from fastapi.testclient import TestClient
from PIL import Image

from main import app
from services import metals, vision
from services.Infos.metais.metal import Metal
from services.Infos.objetos.Lixo import Lixo
from services.integration import build_visual_report
from services.risk.classification import classify_risk

APP_DIR = Path(__file__).resolve().parents[1]
PHOTO = APP_DIR / "services/ComputerVision/dataset/train/images/000058_jpg.rf.zumct7JlQ1EznlPftdka.jpg"
SAMPLE_FILE = APP_DIR / "services/Predict/model/metals_v2/example_request.json"


class IntegrationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.client = TestClient(app, raise_server_exceptions=False)
        cls.photo = PHOTO.read_bytes()
        cls.sample = SAMPLE_FILE.read_text(encoding="utf-8")

    def upload(self, route="/analyze", image=None, data=None, field="image"):
        return self.client.post(route, data={} if data is None else {"data": data},
                                files={field: ("rio.jpg", self.photo if image is None else image, "image/jpeg")})

    def test_real_models_single_inference_and_annotation(self):
        with patch.object(vision, "predict_image", wraps=vision.predict_image) as predict:
            response = self.upload(data=self.sample)
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(predict.call_count, 1)
        result = response.json()
        self.assertEqual(result["totalObjects"], 1)
        self.assertEqual(result["riskLevel"], 2)
        self.assertEqual(result["baseRiskLevel"], 2)
        sample = json.loads(self.sample)
        self.assertEqual(result["riskInputs"], {"ph": sample["ph"],
                         "oxigenio_dissolvido": sample["oxigenio_dissolvido"], "visualClasses": ["Lixo"]})
        self.assertEqual(result["history"]["evaluatedCollections"], 0)
        detection = result["detections"][0]
        self.assertEqual(detection["className"], "Lixo")
        self.assertTrue(detection["indicatesRisk"])
        self.assertAlmostEqual(detection["maxConfidence"], 0.4075051844, places=5)
        self.assertEqual(len(result["metalPredictions"]), 8)
        self.assertEqual(result["metalModelVersion"], "metals_v2_20261004")
        self.assertEqual(result["visionModelVersion"], "9b96a16b9575c7d768c0fd44b561b8335666e7efd2f6d86a2a427030c8a1835a")
        annotated = base64.b64decode(result["annotatedImage"], validate=True)
        with Image.open(io.BytesIO(annotated)) as image:
            self.assertEqual(image.format, "JPEG")
            image.verify()

    def test_http_metal_variation_preserves_existing_inference(self):
        baseline = self.upload(data=self.sample).json()
        sample = json.loads(self.sample)
        sample.update({
            "data": "2026-10-07T12:00:00Z",
            "metais_pesados": [{"name": "Pb", "value": 10, "unit": "µg/L"}],
            "referencia_metais_pesados": {"data": "2026-10-06T12:00:00Z",
                                         "metais_pesados": [{"name": "Pb", "value": 5, "unit": "µg/L"}]},
        })
        response = self.upload(data=json.dumps(sample))
        self.assertEqual(response.status_code, 200, response.text)
        result = response.json()
        self.assertEqual(result["metalVariation"]["metals"][0]["variation"], "aumento")
        self.assertEqual(result["metalVariation"]["metals"][0]["delta"], 5)
        for key in ("metalPredictions", "riskInputs", "riskLevel", "baseRiskLevel", "riskReasons"):
            self.assertEqual(result[key], baseline[key])

    def test_http_invalid_metal_payload_rejected_before_inference(self):
        with patch.object(vision, "load_model", side_effect=AssertionError("Entrada inválida")):
            for field in ("metais_pesados", "referencia_metais_pesados"):
                sample = json.loads(self.sample)
                measurements = [{"name": "Pb", "value": -1, "unit": "µg/L"}]
                sample[field] = measurements if field == "metais_pesados" else {"metais_pesados": measurements}
                self.assertEqual(self.upload(data=json.dumps(sample)).status_code, 422)
            duplicate = self.sample.rstrip()[:-1] + ', "metais_pesados": [{"name":"Pb","value":1,"value":2}]}'
            self.assertEqual(self.upload(data=duplicate).status_code, 422)

    def test_existing_metals_predictions_preserved(self):
        response = self.upload("/predict", data=self.sample)
        self.assertEqual(response.status_code, 200, response.text)
        values = [item["value"] for item in response.json()["metalPredictions"]]
        self.assertEqual(values, [0.0928, 0.01, 0.4463, 0.5, 0.5, 3.5088, 0.05, 0.4732])

    def test_real_pipeline_passes_domain_objects_to_report_and_risk(self):
        with patch("main.build_visual_report", wraps=build_visual_report) as report, \
             patch("main.classify_risk", wraps=classify_risk) as risk:
            response = self.upload(data=self.sample)
        self.assertEqual(response.status_code, 200, response.text)
        images, predicted_metals, _ = report.call_args.args
        self.assertEqual(len(images), 1)
        self.assertIsInstance(images[0], Lixo)
        self.assertEqual(len(predicted_metals), 8)
        self.assertTrue(all(isinstance(metal, Metal) for metal in predicted_metals))
        self.assertIs(risk.call_args.args[0], images)

    def test_boxes_and_confidence_preserved(self):
        image = vision.read_image(UploadFile(filename="rio.jpg", file=io.BytesIO(self.photo)))
        predictions = vision.predict_image(image, vision.load_model())
        self.assertEqual(len(predictions), 1)
        box, confidence, label = predictions[0]
        np.testing.assert_allclose(box, [129.61108398, 111.85837555, 174.56512451, 178.41200256], atol=0.0001)
        self.assertAlmostEqual(confidence, 0.4075051844, places=5)
        self.assertEqual(label, 0)

    def test_vision_only_does_not_estimate_metals(self):
        with patch.object(metals, "predict_metals", side_effect=AssertionError("Sem medições")):
            response = self.upload()
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(response.json()["metalPredictions"], [])
        self.assertNotIn("riskLevel", response.json())

    def test_real_detection_combined_with_measurements_and_history(self):
        sample = json.loads(self.sample)
        sample["oxigenio_dissolvido"] = 4
        response = self.upload(data=json.dumps(sample))
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(response.json()["riskLevel"], 3)
        history = [{"predictionId":i + 1,"coletaId":i + 1,"baseRiskLevel":2} for i in range(3)]
        response = self.client.post("/analyze", data={"data":self.sample,"history":json.dumps(history)},
                                    files={"image":("rio.jpg",self.photo,"image/jpeg")})
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(response.json()["riskLevel"], 3)
        self.assertEqual(response.json()["baseRiskLevel"], 2)
        self.assertEqual(response.json()["history"]["adjustment"], 1)

    def test_existing_image_endpoint(self):
        response = self.upload("/vision/predict", field="file")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.headers["content-type"], "image/jpeg")
        with Image.open(io.BytesIO(response.content)) as image:
            image.verify()

    def test_invalid_and_truncated_image_rejected_before_model(self):
        with patch.object(vision, "load_model", side_effect=AssertionError("Imagem inválida")):
            for content in (b"not an image", b"\xff\xd8\xff\xd9"):
                with self.subTest(content=content):
                    self.assertEqual(self.upload(image=content).status_code, 400)

    def test_size_limit(self):
        response = self.upload(image=b"x" * (vision.MAX_IMAGE_BYTES + 1))
        self.assertEqual(response.status_code, 413)

    def test_missing_invalid_or_non_finite_measurements(self):
        for changes in ({"ph": None}, {"oxigenio_dissolvido": None}, {"ph": 15}, {"temperatura": float("nan")}):
            sample = json.loads(self.sample)
            sample.update(changes)
            with self.subTest(changes=changes):
                self.assertEqual(self.upload(data=json.dumps(sample)).status_code, 422)
        self.assertEqual(self.upload(data="invalid json").status_code, 422)

    def test_strict_contract_rejects_ignored_fields_and_coercion_before_inference(self):
        with patch.object(vision, "load_model", side_effect=AssertionError("Entrada inválida")):
            for changes in ({"ph": "7"}, {"ph": True}, {"oxigenioDissolvido": 6},
                            {"nitrogenio_total": 1}, {"fosforo_total": -1}, {"data": 123}):
                sample = json.loads(self.sample)
                sample.update(changes)
                with self.subTest(changes=changes):
                    self.assertEqual(self.upload(data=json.dumps(sample)).status_code, 422)
            duplicate = self.sample.rstrip()[:-1] + ', "ph": 7}'
            self.assertEqual(self.upload(data=duplicate).status_code, 422)

    def test_optional_missing_values_keep_model_imputation_and_current_risk(self):
        sample = json.loads(self.sample)
        for name in ("solidos_suspensos_totais", "carbono_organico_total", "fosforo_total"):
            sample[name] = None
        response = self.upload(data=json.dumps(sample))
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(response.json()["riskInputs"]["ph"], sample["ph"])
        self.assertEqual(response.json()["riskInputs"]["oxigenio_dissolvido"], sample["oxigenio_dissolvido"])
        self.assertEqual(response.json()["riskLevel"], 2)
        self.assertEqual(len(response.json()["metalPredictions"]), 8)

    def test_model_failure_is_explicit(self):
        with patch.object(vision, "load_model", side_effect=FileNotFoundError("Modelo ausente")):
            with self.assertLogs("main", level="ERROR"):
                response = self.upload()
        self.assertEqual(response.status_code, 503)
        self.assertNotIn("detections", response.json())

    def test_exif_orientation(self):
        photo = Image.new("RGB", (20, 10))
        exif = Image.Exif()
        exif[274] = 6
        file = io.BytesIO()
        photo.save(file, format="JPEG", exif=exif)
        image = vision.read_image(UploadFile(filename="rotated.jpg", file=file))
        self.assertEqual(image.shape[:2], (20, 10))

    def test_infos_and_disabled_dataset_route(self):
        self.assertEqual(self.client.get("/predict/infos").status_code, 200)
        self.assertEqual(self.client.get("/vision/infos").status_code, 200)
        self.assertEqual(self.client.get("/predict/test").status_code, 404)


if __name__ == "__main__":
    unittest.main()
