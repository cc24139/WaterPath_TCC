import ast
import hashlib
import json
import os
from pathlib import Path

import cv2
import numpy as np
from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.responses import Response
from PIL import Image, ImageOps, UnidentifiedImageError
from pydantic import ValidationError

from DTOs.Amostra import Amostra
from services.integration import build_visual_report, structured_metals
from services.Predict.schema import FEATURES, FIELDS, REQUIRED_FIELDS, TARGETS

servicesPath = Path(__file__).resolve().parent / "services"
trainVersion = "train"
QUALITY_VERSION = "metals_v2"
QUALITY_MODEL_DEFAULT = servicesPath / "Predict/model" / QUALITY_VERSION / "quality_model.joblib"


app = FastAPI(title="API de IA", description="API para predição com YOLO e regressão de metais", version="2.0.0")


def vision_model():
    import onnxruntime as ort

    path = Path(os.getenv("YOLO_MODEL_PATH", str(servicesPath / "ComputerVision/best.onnx")))
    return ort.InferenceSession(str(path), providers=["CPUExecutionProvider"])


def vision_names(model=None):
    if model is None:
        model = vision_model()
    return ast.literal_eval(model.get_modelmeta().custom_metadata_map["names"])


def quality_model():
    import joblib

    path = Path(os.getenv("QUALITY_MODEL_PATH", str(QUALITY_MODEL_DEFAULT)))
    metadata = quality_metadata()
    if hashlib.sha256(path.read_bytes()).hexdigest() != metadata["model_sha256"]:
        raise ValueError("Joblib diverge do hash registrado no manifesto")
    model = joblib.load(path)
    names = list(model.feature_names_in_)
    if not set(names).issubset(FEATURES) or not {FIELDS[field][0] for field in REQUIRED_FIELDS}.issubset(names):
        raise ValueError("Modelo incompatível com o contrato v2 das entradas")
    if names != metadata["features"] or metadata["targets"] != TARGETS:
        raise ValueError("Modelo e manifesto possuem entradas ou alvos diferentes")
    return model


features = FEATURES
result = TARGETS


def quality_metadata():
    path = Path(os.getenv("QUALITY_MODEL_PATH", str(QUALITY_MODEL_DEFAULT)))
    return json.loads(path.with_name("manifest.json").read_text(encoding="utf-8"))


@app.get("/predict/infos")
def quality_infos():
    metadata = quality_metadata()
    columns = list(quality_model().feature_names_in_)
    return {
        "modelVersion": metadata["version"],
        "features": [{"field": field, "column": column, "unit": unit,
                      "required": field in REQUIRED_FIELDS}
                     for field, (column, unit) in FIELDS.items() if column in columns],
        "targets": result,
        "targetFraction": metadata["target_fraction"],
        "removedFeatures": metadata["removed_low_contribution_features"],
        "testMetrics": metadata["test"],
        "limitations": metadata["limitations"],
    }


def read_image(file):
    try:
        file.file.seek(0)
        with Image.open(file.file) as source:
            with ImageOps.exif_transpose(source) as oriented:
                with oriented.convert("RGB") as rgb:
                    return cv2.cvtColor(np.asarray(rgb), cv2.COLOR_RGB2BGR)
    except Image.DecompressionBombError as exc:
        raise HTTPException(status_code=413, detail="A imagem excede o limite de pixels do leitor") from exc
    except (UnidentifiedImageError, OSError, ValueError) as exc:
        raise HTTPException(status_code=400, detail="O arquivo enviado não é uma imagem válida") from exc


def prepare_vision_input(img, size):
    """Ajusta a imagem ao tamanho do modelo, preservando sua proporção."""
    height, width = img.shape[:2]
    scale = min(size / height, size / width)
    resized = cv2.resize(img, (round(width * scale), round(height * scale)))
    left = round((size - resized.shape[1]) / 2 - 0.1)
    top = round((size - resized.shape[0]) / 2 - 0.1)
    padded = np.full((size, size, 3), 114, dtype=np.uint8)
    padded[top:top + resized.shape[0], left:left + resized.shape[1]] = resized
    rgb = cv2.cvtColor(padded, cv2.COLOR_BGR2RGB)
    normalized = rgb.astype(np.float32) / 255.0
    channels_first = normalized.transpose(2, 0, 1)
    tensor = np.expand_dims(channels_first, axis=0)
    return tensor, scale, left, top


def extract_detections(output, image_shape, scale, left, top):
    """Filtra as detecções e converte as caixas para a imagem original."""
    height, width = image_shape[:2]
    scores = output[:, 4:].max(axis=1)
    selected = scores > 0.25
    output = output[selected]
    scores = scores[selected]
    if len(output) == 0:
        return []
    classes = output[:, 4:].argmax(axis=1)
    boxes = output[:, :4].copy()
    boxes[:, :2] -= boxes[:, 2:] / 2
    indices = cv2.dnn.NMSBoxesBatched(boxes.tolist(), scores.tolist(), classes.tolist(), 0.25, 0.7)
    detections = []
    for index in np.asarray(indices).reshape(-1)[:300]:
        x, y, box_width, box_height = boxes[index]
        coordinates = np.array([
            x - left,
            y - top,
            x + box_width - left,
            y + box_height - top,
        ]) / scale
        coordinates[[0, 2]] = coordinates[[0, 2]].clip(0, width)
        coordinates[[1, 3]] = coordinates[[1, 3]].clip(0, height)
        detections.append((coordinates, float(scores[index]), int(classes[index])))
    return detections


def predict_image(img, session=None):
    if session is None:
        session = vision_model()
    model_input = session.get_inputs()[0]
    tensor, scale, left, top = prepare_vision_input(img, model_input.shape[2])
    outputs = session.run(None, {model_input.name: tensor})
    predictions = outputs[0][0].T
    return extract_detections(predictions, img.shape, scale, left, top)


@app.get("/vision/infos")
def getInfos():
    return {
        "msg:": "Informações do modelo de visão computacional",
        "names": vision_names(),
        "trainVersion": trainVersion,
    }


@app.post("/vision/predict")
def predict(file: UploadFile = File(...)):
    img = read_image(file)
    model = vision_model()
    predictions = predict_image(img, model)
    names = vision_names(model)
    for box, confidence, label in predictions:
        x1, y1, x2, y2 = box.round().astype(int)
        cv2.rectangle(img, (x1, y1), (x2, y2), (0, 180, 255), 2)
        cv2.putText(img, f"{names[label]} {confidence:.2f}", (x1, max(y1 - 8, 16)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 180, 255), 1)
    success, jpeg = cv2.imencode(".jpg", img)
    if not success:
        raise HTTPException(status_code=500, detail="Não foi possível gerar a imagem predita")
    return Response(
        content=jpeg.tobytes(),
        media_type="image/jpeg",
        headers={"Content-Disposition": 'inline; filename="predicao.jpg"'},
    )


def predict_metals(data, model=None):
    import pandas as pd

    if model is None:
        model = quality_model()
    columns = list(model.feature_names_in_)
    inputs = pd.DataFrame(data.decode(columns), columns=columns)
    predicted = model.predict(inputs)
    return {metal: f"{value.round(4)}" for metal, value in zip(result, predicted[0])}


@app.post("/predict")
def predictQuality(data: str = Form(...), image: UploadFile = File(...)):
    try:
        sample = Amostra.model_validate_json(data)
    except ValidationError as exc:
        raise HTTPException(status_code=422, detail="Dados da amostra inválidos") from exc
    img = read_image(image)
    metal_model = quality_model()
    visual_model = vision_model()
    predicted = predict_metals(sample, metal_model)
    predictions = predict_image(img, visual_model)
    visual_report = build_visual_report(predictions, predicted, vision_names(visual_model), img.shape)
    return {
        "detections": visual_report["detections"],
        "totalObjects": sum(item["count"] for item in visual_report["detections"]),
        "metalPredictions": structured_metals(predicted),
    }


@app.get("/predict/test")
def predict_test():
    if os.getenv("ENABLE_TEST_ENDPOINT", "false").lower() != "true":
        raise HTTPException(status_code=404, detail="Rota de teste desabilitada")
    import pandas as pd
    path = Path(os.getenv("QUALITY_MODEL_PATH", str(QUALITY_MODEL_DEFAULT)))
    test_path = path.with_name("test_samples.csv")
    if not test_path.is_file():
        raise HTTPException(status_code=404, detail="Dataset de teste não disponível neste ambiente")
    dados = pd.read_csv(test_path)
    model = quality_model()
    TestX = dados.loc[:, list(model.feature_names_in_)]
    TestY = dados.loc[:, result]
    predicoes = model.predict(TestX)
    PredY = pd.DataFrame(
        predicoes,
        columns=result,
        index=TestY.index
    ).round(4)
    TestY = TestY.round(4)
    resultado = []
    for i in TestX.index:
        resultado.append({
            "entrada": {key: (None if pd.isna(value) else value) for key, value in TestX.loc[i].to_dict().items()},
            "real": TestY.loc[i].to_dict(),
            "predito": PredY.loc[i].to_dict()
        })
    return resultado


@app.get("/")
async def root():
    return {"msg": "API funcionando"}

#Para rodar no windows: uvicorn main:app --reload --host

def tratar_censurado(valor):
    if isinstance(valor, str) and valor.startswith("<"):
        return float(valor[1:]) / 2
    return float(valor)
