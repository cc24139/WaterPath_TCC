"""Verifica os objetos usados na interpretação e na classificação de risco."""
import unittest
from unittest.mock import patch

from DTOs.Amostra import Amostra
from services.Infos.metais import create_metals
from services.Infos.metais.Cobre import Cobre
from services.Infos.metais.Cromo import Cromo
from services.Infos.metais.Ferro import Ferro
from services.Infos.metais.Maganes import Maganes
from services.Infos.metais.Niquel import Niquel
from services.Infos.metais.Zinco import Zinco
from services.Infos.metais.cadmio import Cadmio
from services.Infos.metais.chumbo import Chumbo
from services.Infos.metais.metal import Metal
from services.Infos.objetos import create_images
from services.Infos.objetos.Drenagem import Drenagem
from services.Infos.objetos.Impact import Impact
from services.Infos.objetos.Lixo import Lixo
from services.Infos.objetos.Urbano import Urbano
from services.Infos.objetos.turbidez import Turbidez
from services.integration import build_visual_report, interpretation, structured_metals
from services.risk.classification import classify_risk
from services import metals as metal_service


class DomainTests(unittest.TestCase):
    def test_metal_estimates_fill_only_missing_values_and_keep_observations(self):
        sample = Amostra(temperatura=22, ph=7, condutividade_eletrica=100, oxigenio_dissolvido=6,
                         metais_pesados=[{"name": "Pb", "value": 0.02, "unit": "mg/L"}])
        predicted = create_metals({
            "Fe, mg/L": 0.1, "Mn, mg/L": 0.2, "Cr, µg/L": 3, "Ni, µg/L": 4,
            "Cu, µg/L": 5, "Zn, µg/L": 6, "Cd, µg/L": 7, "Pb, µg/L": 8,
        })
        with patch.object(metal_service, "predict_metals", return_value=predicted) as inference:
            combined, measured, estimated, predictions = metal_service.predict_and_merge_metals(sample)
        inference.assert_called_once_with(sample)
        self.assertEqual(len(combined), 8)
        self.assertEqual(measured, [{"name": "Pb", "value": 0.02, "unit": "mg/L"}])
        self.assertEqual(len(estimated), 7)
        self.assertEqual(len(predictions), 7)
        self.assertEqual(next(item for item in combined if item.codigo == "Pb").valor, 20)

    def test_complete_measured_metals_skip_model_and_preserve_all_values(self):
        values = [
            ("Fe", 1, "mg/L"), ("Mn", 2, "mg/L"), ("Cr", 3, "µg/L"), ("Ni", 4, "µg/L"),
            ("Cu", 5, "µg/L"), ("Zn", 6, "µg/L"), ("Cd", 7, "µg/L"), ("Pb", 8, "µg/L"),
        ]
        sample = Amostra(temperatura=22, ph=7, condutividade_eletrica=100, oxigenio_dissolvido=6,
                         metais_pesados=[{"name": name, "value": value, "unit": unit} for name, value, unit in values])
        with patch.object(metal_service, "predict_metals", side_effect=AssertionError("All metals measured")):
            combined, measured, estimated, predictions = metal_service.predict_and_merge_metals(sample)
        self.assertEqual(len(combined), 8)
        self.assertEqual(len(measured), 8)
        self.assertEqual(estimated, [])
        self.assertEqual(predictions, [])

    def test_all_metals_keep_historical_references_and_boundaries(self):
        references = [
            ("Fe, mg/L", Ferro, "Ferro", 0.3),
            ("Mn, mg/L", Maganes, "Maganês", 0.1),
            ("Cr, µg/L", Cromo, "Cromo", 50),
            ("Ni, µg/L", Niquel, "Níquel", 25),
            ("Cu, µg/L", Cobre, "Cobre", 9),
            ("Zn, µg/L", Zinco, "Zinco", 180),
            ("Cd, µg/L", Cadmio, "Cádmio", 1),
            ("Pb, µg/L", Chumbo, "Chumbo", 10),
        ]
        for column, metal_class, name, limit in references:
            with self.subTest(column=column):
                metal = create_metals({column: limit})[0]
                self.assertIsInstance(metal, Metal)
                self.assertIsInstance(metal, metal_class)
                self.assertEqual(metal.nome, name)
                self.assertEqual(metal.valor, limit)
                self.assertEqual(metal.definirLimite(), limit)
                self.assertFalse(metal.acima_do_limite())
                metal.valor = limit + 0.01
                self.assertTrue(metal.acima_do_limite())
                self.assertTrue(metal.mensagem())
                code, unit = column.split(", ")
                self.assertEqual(structured_metals([metal]), [
                    {"name": code, "unit": unit, "value": limit + 0.01}
                ])
                mg_limit = limit if unit == "mg/L" else limit / 1000
                self.assertEqual(metal.definirLimite(False), mg_limit)

    def test_images_use_model_names_and_preserve_detection_attributes(self):
        names = {7: "Lixo", 4: "URBANO", 9: "drainage connection", 3: "desconhecida"}
        predictions = [([1, 2, 11, 22], 0.73, label) for label in names]
        images = create_images(predictions, names)
        self.assertEqual([type(image) for image in images], [Lixo, Urbano, Drenagem, Impact])
        for image, label in zip(images, names):
            self.assertEqual(image.obj, [1, 2, 11, 22])
            self.assertEqual(image.confidence, 0.73)
            self.assertEqual(image.name, names[label])
            self.assertEqual(image.class_id, label)

    def test_unrelated_metals_do_not_interrupt_visual_alerts(self):
        metals = create_metals({"Fe, mg/L": 2, "Mn, mg/L": 1, "Cr, µg/L": 51,
                                "Zn, µg/L": 181, "Cd, µg/L": 2, "Pb, µg/L": 11})
        message = interpretation(Lixo([0, 0, 1, 1]), metals)
        for name in ("Zinco", "Cádmio", "Chumbo"):
            self.assertIn(f"{name} previsto acima", message)
        for name in ("Ferro", "Maganês", "Cromo"):
            self.assertNotIn(f"{name} previsto acima", message)
        self.assertIn("Cromo previsto acima", interpretation(Urbano([0, 0, 1, 1]), metals))

    def test_interpretation_reads_object_limits_and_methods(self):
        zinc = Zinco(181)
        image = Lixo([0, 0, 1, 1])
        self.assertIn("Zinco previsto acima", interpretation(image, [zinc]))
        zinc.limite = 200
        self.assertNotIn("Zinco previsto acima", interpretation(image, [zinc]))
        with patch.object(image, "impact", return_value="interpretação do objeto") as impact:
            self.assertEqual(interpretation(image, [zinc]), "interpretação do objeto")
            impact.assert_called_once_with([zinc])

    def test_drainage_and_unimplemented_classes_do_not_invent_metal_associations(self):
        metals = create_metals({"Zn, µg/L": 999, "Cd, µg/L": 999})
        drainage = Drenagem([0, 0, 1, 1], 0.7, "drainage connection")
        self.assertTrue(drainage.indica_risco())
        self.assertNotIn("previsto acima", interpretation(drainage, metals))
        for image_class in (Impact, Turbidez):
            image = image_class([0, 0, 1, 1], 0.7, "desconhecida")
            self.assertFalse(image.indica_risco())
            self.assertEqual(interpretation(image, metals), "Classe visual sem regra de interpretação cadastrada.")

    def test_report_aggregates_objects_without_changing_predictions(self):
        names = {5: "Lixo"}
        images = create_images([([0, 0, 10, 10], 0.4, 5), ([0, 0, 20, 20], 0.8, 5)], names)
        metals = create_metals({"Zn, µg/L": 180})
        report = build_visual_report(images, metals, (100, 100, 3))
        detection = report["detections"][0]
        self.assertEqual(detection["classId"], 5)
        self.assertEqual(detection["count"], 2)
        self.assertEqual(detection["maxConfidence"], 0.8)
        self.assertAlmostEqual(detection["meanBoxAreaRatio"], 0.025)
        self.assertNotIn("previsto acima", detection["interpretation"])
        self.assertEqual(metals[0].valor, 180)
        self.assertEqual(build_visual_report([], [], (100, 100, 3)), {"detections": []})

    def test_risk_uses_the_visual_object_method(self):
        image = Lixo([0, 0, 1, 1])
        sample = Amostra(temperatura=22, ph=7, condutividade_eletrica=100, oxigenio_dissolvido=6)
        self.assertEqual(classify_risk([image], sample, [])["riskLevel"], 2)
        with patch.object(image, "indica_risco", return_value=False):
            self.assertEqual(classify_risk([image], sample, [])["riskLevel"], 1)
