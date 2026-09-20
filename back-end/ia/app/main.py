import ast
import os
from functools import lru_cache
from pathlib import Path
from threading import Lock

for variable in ("OMP_NUM_THREADS", "MKL_NUM_THREADS", "OPENBLAS_NUM_THREADS", "NUMEXPR_NUM_THREADS"):
    os.environ[variable] = "1"

import cv2
import numpy as np
from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.responses import JSONResponse, Response
from PIL import Image, ImageOps, UnidentifiedImageError
from pydantic import ValidationError

from DTOs.Amostra import Amostra
from services.Infos.objetos.Lixo import Lixo
from services.Infos.objetos.Urbano import Urbano

cv2.setNumThreads(1)
MAX_UPLOAD_BYTES = 4 * 1024 * 1024
MAX_REQUEST_BYTES = MAX_UPLOAD_BYTES + 64 * 1024
MAX_IMAGE_PIXELS = 4_000_000
MAX_IMAGE_SIDE = 1280
Image.MAX_IMAGE_PIXELS = MAX_IMAGE_PIXELS
servicesPath = Path(__file__).resolve().parent / "services"
trainVersion = "train"
dataTestPATH = servicesPath / "Predict/data/Data_Lake Onego_V2.xlsx"


class InferenceLimits:
    def __init__(self, app):
        self.app = app
        self.lock = Lock()

    async def __call__(self, scope, receive, send):
        paths = {"/predict", "/predict/test", "/vision/predict", "/vision/infos"}
        if scope["type"] != "http" or scope["path"].rstrip("/") not in paths:
            return await self.app(scope, receive, send)
        headers = dict(scope["headers"])
        try:
            length = int(headers.get(b"content-length", b"0"))
        except ValueError:
            return await JSONResponse({"detail": "Content-Length inválido"}, status_code=400)(scope, receive, send)
        if length > MAX_REQUEST_BYTES:
            return await JSONResponse({"detail": "Envie uma imagem de até 4 MB"}, status_code=413)(scope, receive, send)
        if not self.lock.acquire(blocking=False):
            return await JSONResponse(
                {"detail": "Servidor ocupado. Tente novamente em instantes."},
                status_code=503,
                headers={"Retry-After": "2"},
            )(scope, receive, send)
        received = 0

        async def limited_receive():
            nonlocal received
            message = await receive()
            received += len(message.get("body", b""))
            if received > MAX_REQUEST_BYTES:
                raise HTTPException(status_code=413, detail="Envie uma imagem de até 4 MB")
            return message

        try:
            await self.app(scope, limited_receive, send)
        finally:
            self.lock.release()


app = FastAPI(title="API de IA", description="API para predição com YOLO", version="1.0.0")
app.add_middleware(InferenceLimits)


@lru_cache(maxsize=1)
def vision_model():
    import onnxruntime as ort

    options = ort.SessionOptions()
    options.intra_op_num_threads = 1
    options.inter_op_num_threads = 1
    options.enable_cpu_mem_arena = False
    options.enable_mem_pattern = False
    path = Path(os.getenv("YOLO_MODEL_PATH", str(servicesPath / "ComputerVision/best.onnx")))
    return ort.InferenceSession(str(path), sess_options=options, providers=["CPUExecutionProvider"])


def vision_names():
    return ast.literal_eval(vision_model().get_modelmeta().custom_metadata_map["names"])


@lru_cache(maxsize=1)
def quality_model():
    import joblib

    model = joblib.load(servicesPath / "Predict/model/best_model.pkl", mmap_mode="r")
    parameters = model.get_params(deep=True)
    limits = {key: 1 for key in parameters if key.split("__")[-1] in {"n_jobs", "thread_count", "nthread"}}
    if limits:
        model.set_params(**limits)
    return model


features = [
    "T, °C",
    "рН",
    "TSS, mg/L",
    "Color, mg Pt-Co/L",
    "TOC, mg/L",
    "CODMn, mg О/L",
    "CODCr, мгО/л",
    "BOD5, mg О2/L",
    "PO4-P, µg/L",
    "TP, µg/L",
    "NH4-N, mgN/L",
    "NO2-N, mgN/L",
    "NO3-N, mgN/L",
    "TN, mg/L",
    "EC, μS/сm at 25 °C",
    "Depth, m"
]


result = [
    "Fe, mg/L",
    "Mn, mg/L",
    "Cr, µg/L",
    "Ni, µg/L",
    "Cu, µg/L",
    "Zn, µg/L",
    "Cd, µg/L",
    "Pb, µg/L"
]


def read_image(file):
    if file.size is not None and file.size > MAX_UPLOAD_BYTES:
        raise HTTPException(status_code=413, detail="Envie uma imagem de até 4 MB")
    try:
        file.file.seek(0, 2)
        if file.file.tell() > MAX_UPLOAD_BYTES:
            raise HTTPException(status_code=413, detail="Envie uma imagem de até 4 MB")
        file.file.seek(0)
        with Image.open(file.file) as source:
            if source.width * source.height > MAX_IMAGE_PIXELS:
                raise HTTPException(status_code=413, detail="A imagem deve ter até 4 megapixels")
            source.draft("RGB", (MAX_IMAGE_SIDE, MAX_IMAGE_SIDE))
            source.thumbnail((MAX_IMAGE_SIDE, MAX_IMAGE_SIDE))
            with ImageOps.exif_transpose(source) as oriented:
                with oriented.convert("RGB") as rgb:
                    return cv2.cvtColor(np.asarray(rgb), cv2.COLOR_RGB2BGR)
    except Image.DecompressionBombError as exc:
        raise HTTPException(status_code=413, detail="A imagem deve ter até 4 megapixels") from exc
    except (UnidentifiedImageError, OSError, ValueError) as exc:
        raise HTTPException(status_code=400, detail="O arquivo enviado não é uma imagem válida") from exc


def predict_image(img):
    session = vision_model()
    height, width = img.shape[:2]
    size = session.get_inputs()[0].shape[2]
    scale = min(size / height, size / width)
    resized = cv2.resize(img, (round(width * scale), round(height * scale)))
    left = round((size - resized.shape[1]) / 2 - 0.1)
    top = round((size - resized.shape[0]) / 2 - 0.1)
    padded = np.full((size, size, 3), 114, dtype=np.uint8)
    padded[top:top + resized.shape[0], left:left + resized.shape[1]] = resized
    tensor = np.ascontiguousarray(padded[:, :, ::-1].transpose(2, 0, 1)[None], dtype=np.float32)
    tensor /= 255.0
    output = session.run(None, {session.get_inputs()[0].name: tensor})[0][0].T
    scores = output[:, 4:].max(axis=1)
    selected = scores > 0.25
    output = output[selected]
    scores = scores[selected]
    if not len(output):
        return []
    classes = output[:, 4:].argmax(axis=1)
    boxes = output[:, :4].copy()
    boxes[:, :2] -= boxes[:, 2:] / 2
    indices = cv2.dnn.NMSBoxesBatched(boxes.tolist(), scores.tolist(), classes.tolist(), 0.25, 0.7)
    detections = []
    for index in np.asarray(indices).reshape(-1)[:300]:
        x, y, w, h = boxes[index]
        coordinates = np.array([x - left, y - top, x + w - left, y + h - top]) / scale
        coordinates[[0, 2]] = coordinates[[0, 2]].clip(0, width)
        coordinates[[1, 3]] = coordinates[[1, 3]].clip(0, height)
        detections.append((coordinates, float(scores[index]), int(classes[index])))
    return detections


def image_report(predictions, metals):
    report = []
    for box, confidence, label in predictions:
        if label == 2:
            obj = Urbano(box, confidence, "Urbano")
        else:
            obj = Lixo(box, confidence if label == 0 else -1, "lixo")
        report.append([obj.impact(metals)])
    return report


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
    predictions = predict_image(img)
    names = vision_names()
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


def predict_metals(data):
    predicted = quality_model().predict(data.decode())
    return {metal: f"{value.round(4)}" for metal, value in zip(result, predicted[0])}


@app.post("/predict")
def predictQuality(data: str = Form(...), image: UploadFile = File(...)):
    try:
        sample = Amostra.model_validate_json(data)
    except ValidationError as exc:
        raise HTTPException(status_code=422, detail="Dados da amostra inválidos") from exc
    img = read_image(image)
    predicted = predict_metals(sample)
    predictions = predict_image(img)
    return {
        "msg": "Predição realizada com sucesso",
        "predictions": predicted,
        "predictedHeavyMetais": sample.model_dump(),
        "relatorioImg": image_report(predictions, predicted),
    }


@app.get("/predict/test")
def predict_test():
    if os.getenv("ENABLE_TEST_ENDPOINT", "false").lower() != "true":
        raise HTTPException(status_code=404, detail="Rota de teste desabilitada")
    import pandas as pd
    from sklearn.model_selection import train_test_split

    dados = pd.read_excel(dataTestPATH)
    dados.columns = dados.columns.str.strip()
    dados = dados.dropna()
    dados[features] = dados[features].map(tratar_censurado)
    dados[result] = dados[result].map(tratar_censurado)
    dados = dados.dropna()
    X = dados.loc[:, features]
    Y = dados.loc[:, result]
    _, TestX, _, TestY = train_test_split(
        X,
        Y,
        test_size=0.2,
        random_state=42
    )
    predicoes = quality_model().predict(TestX)
    PredY = pd.DataFrame(
        predicoes,
        columns=result,
        index=TestY.index
    ).round(4)
    TestY = TestY.round(4)
    resultado = []
    for i in TestX.index:
        resultado.append({
            "entrada": TestX.loc[i].to_dict(),
            "real": TestY.loc[i].to_dict(),
            "predito": PredY.loc[i].to_dict()
        })
    return resultado


@app.get("/")
async def root():
    return {"msg": "API funcionando"}


def tratar_censurado(valor):
    if isinstance(valor, str) and valor.startswith("<"):
        return float(valor[1:]) / 2
    return float(valor)
