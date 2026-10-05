"""Carrega o modelo atual e estima as concentrações na unidade do treinamento."""
import hashlib
import json
import os
from functools import lru_cache
from pathlib import Path

import joblib
import pandas as pd

from services.Predict.schema import FEATURES, FIELDS, REQUIRED_FIELDS, TARGETS

DEFAULT_MODEL_PATH = Path(__file__).parent / "Predict/model/metals_v2/quality_model.joblib"


def model_path():
    return Path(os.getenv("QUALITY_MODEL_PATH", str(DEFAULT_MODEL_PATH)))


@lru_cache(maxsize=1)
def model_metadata():
    return json.loads(model_path().with_name("manifest.json").read_text(encoding="utf-8"))


@lru_cache(maxsize=1)
def load_model():
    path = model_path()
    metadata = model_metadata()
    if hashlib.sha256(path.read_bytes()).hexdigest() != metadata["model_sha256"]:
        raise ValueError("Joblib diverge do hash registrado no manifesto")
    model = joblib.load(path)
    columns = list(model.feature_names_in_)
    required_columns = {FIELDS[field][0] for field in REQUIRED_FIELDS}
    if not set(columns).issubset(FEATURES) or not required_columns.issubset(columns):
        raise ValueError("Modelo incompatível com o contrato v2 das entradas")
    if columns != metadata["features"] or metadata["targets"] != TARGETS:
        raise ValueError("Modelo e manifesto possuem entradas ou alvos diferentes")
    return model


def predict_metals(sample):
    model = load_model()
    columns = list(model.feature_names_in_)
    inputs = pd.DataFrame(sample.decode(columns), columns=columns)
    values = model.predict(inputs)[0]
    return {metal: float(value.round(4)) for metal, value in zip(TARGETS, values)}


def model_infos():
    metadata = model_metadata()
    columns = list(load_model().feature_names_in_)
    return {
        "modelVersion": metadata["version"],
        "features": [
            {"field": field, "column": column, "unit": unit, "required": field in REQUIRED_FIELDS}
            for field, (column, unit) in FIELDS.items() if column in columns
        ],
        "targets": TARGETS,
        "targetFraction": metadata["target_fraction"],
        "removedFeatures": metadata["removed_low_contribution_features"],
        "testMetrics": metadata["test"],
        "limitations": metadata["limitations"],
    }
