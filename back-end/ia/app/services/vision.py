"""Leitura, detecção e anotação com os pesos ONNX existentes."""
import ast
import hashlib
import os
import warnings
from functools import lru_cache
from pathlib import Path

import cv2
import numpy as np
import onnxruntime as ort
from fastapi import HTTPException, UploadFile
from PIL import Image, ImageOps, UnidentifiedImageError

MAX_IMAGE_BYTES = 10 * 1024 * 1024
DEFAULT_MODEL_PATH = Path(__file__).parent / "ComputerVision/best.onnx"


def model_path():
    return Path(os.getenv("YOLO_MODEL_PATH", str(DEFAULT_MODEL_PATH)))


@lru_cache(maxsize=1)
def load_model():
    return ort.InferenceSession(str(model_path()), providers=["CPUExecutionProvider"])


def class_names(model):
    return ast.literal_eval(model.get_modelmeta().custom_metadata_map["names"])


@lru_cache(maxsize=1)
def model_version():
    return hashlib.sha256(model_path().read_bytes()).hexdigest()


def read_image(file: UploadFile):
    try:
        file.file.seek(0, 2)
        if file.file.tell() > MAX_IMAGE_BYTES:
            raise HTTPException(status_code=413, detail="A imagem deve ter até 10 MB")
        file.file.seek(0)
        # Valida o conteúdo; o tipo informado pelo cliente não é suficiente.
        with warnings.catch_warnings():
            warnings.simplefilter("error", Image.DecompressionBombWarning)
            with Image.open(file.file) as source:
                if source.format not in {"JPEG", "PNG"}:
                    raise HTTPException(status_code=400, detail="Envie uma imagem JPEG ou PNG válida")
                with ImageOps.exif_transpose(source) as oriented:
                    with oriented.convert("RGB") as rgb:
                        return cv2.cvtColor(np.asarray(rgb), cv2.COLOR_RGB2BGR)
    except (Image.DecompressionBombError, Image.DecompressionBombWarning) as exc:
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


def predict_image(img, session):
    model_input = session.get_inputs()[0]
    tensor, scale, left, top = prepare_vision_input(img, model_input.shape[2])
    outputs = session.run(None, {model_input.name: tensor})
    predictions = outputs[0][0].T
    return extract_detections(predictions, img.shape, scale, left, top)


def annotate_image(img, predictions, names):
    annotated = img.copy()
    for box, confidence, label in predictions:
        x1, y1, x2, y2 = box.round().astype(int)
        cv2.rectangle(annotated, (x1, y1), (x2, y2), (0, 180, 255), 2)
        cv2.putText(annotated, f"{names[label]} {confidence:.2f}", (x1, max(y1 - 8, 16)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 180, 255), 1)
    success, jpeg = cv2.imencode(".jpg", annotated)
    if not success:
        raise HTTPException(status_code=500, detail="Não foi possível gerar a imagem predita")
    return jpeg.tobytes()
