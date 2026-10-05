"""Rotas da IA; inferência e apresentação ficam nos serviços."""
import base64
import logging
import os

import pandas as pd
from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.responses import JSONResponse, Response
from pydantic import ValidationError

from DTOs.Amostra import Amostra
from services import metals, vision
from services.integration import build_visual_report, structured_metals
from services.Predict.schema import TARGETS
from services.risk.classification import classify_risk, parse_history

app = FastAPI(title="API de IA", description="API para predição com YOLO e regressão de metais", version="2.0.0")
logger = logging.getLogger(__name__)


@app.exception_handler(Exception)
async def inference_error(request, error):
    logger.exception("Falha na análise da IA", exc_info=error)
    return JSONResponse(status_code=503, content={"detail": "O serviço de IA não conseguiu realizar a análise"})


def parse_sample(data):
    try:
        return Amostra.model_validate_json(data)
    except ValidationError as exc:
        raise HTTPException(status_code=422, detail="Dados da amostra inválidos") from exc


def analyze_image(image, data=None, include_annotation=False, history="[]"):
    sample = parse_sample(data) if data is not None else None
    previous_samples = parse_history(history)
    img = vision.read_image(image)
    model = vision.load_model()
    predictions = vision.predict_image(img, model)
    names = vision.class_names(model)
    predicted_metals = metals.predict_metals(sample) if sample is not None else {}
    report = build_visual_report(predictions, predicted_metals, names, img.shape)
    result = {
        "detections": report["detections"],
        "totalObjects": sum(item["count"] for item in report["detections"]),
        "metalPredictions": structured_metals(predicted_metals),
    }
    if include_annotation:
        if sample is not None:
            result.update(classify_risk(report["detections"], sample, previous_samples))
        result["visionModelVersion"] = vision.model_version()
        if sample is not None:
            result["metalModelVersion"] = metals.model_metadata()["version"]
        # A anotação e o relatório usam exatamente as mesmas detecções.
        result["annotatedImage"] = base64.b64encode(vision.annotate_image(img, predictions, names)).decode("ascii")
        result["annotatedImageContentType"] = "image/jpeg"
    return result


@app.post("/analyze")
def analyze(image: UploadFile = File(...), data: str | None = Form(None), history: str = Form("[]")):
    return analyze_image(image, data, include_annotation=True, history=history)


@app.post("/predict")
def predict_quality(data: str = Form(...), image: UploadFile = File(...)):
    return analyze_image(image, data)


@app.get("/predict/infos")
def quality_infos():
    return metals.model_infos()


@app.get("/vision/infos")
def vision_infos():
    return {
        "msg:": "Informações do modelo de visão computacional",
        "names": vision.class_names(vision.load_model()),
        "trainVersion": "train",
    }


@app.post("/vision/predict")
def predict_annotated_image(file: UploadFile = File(...)):
    img = vision.read_image(file)
    model = vision.load_model()
    predictions = vision.predict_image(img, model)
    jpeg = vision.annotate_image(img, predictions, vision.class_names(model))
    return Response(content=jpeg, media_type="image/jpeg",
                    headers={"Content-Disposition": 'inline; filename="predicao.jpg"'})


@app.get("/predict/test")
def predict_test():
    if os.getenv("ENABLE_TEST_ENDPOINT", "false").lower() != "true":
        raise HTTPException(status_code=404, detail="Rota de teste desabilitada")
    test_path = metals.model_path().with_name("test_samples.csv")
    if not test_path.is_file():
        raise HTTPException(status_code=404, detail="Dataset de teste não disponível neste ambiente")
    samples = pd.read_csv(test_path)
    model = metals.load_model()
    inputs = samples.loc[:, list(model.feature_names_in_)]
    expected = samples.loc[:, TARGETS].round(4)
    predicted = pd.DataFrame(model.predict(inputs), columns=TARGETS, index=expected.index).round(4)
    return [
        {
            "entrada": {key: None if pd.isna(value) else value for key, value in inputs.loc[index].to_dict().items()},
            "real": expected.loc[index].to_dict(),
            "predito": predicted.loc[index].to_dict(),
        }
        for index in inputs.index
    ]


@app.get("/")
def root():
    return {"msg": "API funcionando"}
