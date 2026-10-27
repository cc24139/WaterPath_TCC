"""Carrega o modelo atual e estima as concentrações na unidade do treinamento."""
import hashlib
import json
import os
from functools import lru_cache
from pathlib import Path

import joblib
import pandas as pd

from services.Predict.schema import FEATURES, FIELDS, REQUIRED_FIELDS, TARGETS
from services.Infos.metais import create_metals

DEFAULT_MODEL_PATH = Path(__file__).parents[1] / "models/metals_v2/quality_model.joblib"


class InvalidMetalMeasurement(ValueError):
    pass


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
    return create_metals({metal: float(value.round(4)) for metal, value in zip(TARGETS, values)})


def predict_and_merge_metals(sample):
    """Fill missing measurements with the model and retain valid measured values."""
    target_by_name = {target.split(",", 1)[0]: target for target in TARGETS}
    observed = {item.name: item for item in (sample.metais_pesados or [])}
    values = {}
    estimated = []
    measured = []
    units = {"Fe, mg/L": "mg/L", "Mn, mg/L": "mg/L", "Cr, µg/L": "µg/L", "Ni, µg/L": "µg/L",
             "Cu, µg/L": "µg/L", "Zn, µg/L": "µg/L", "Cd, µg/L": "µg/L", "Pb, µg/L": "µg/L"}
    conversion = {"mg/l": 1.0, "ug/l": 0.001}
    model_predictions = []
    for item in observed.values():
        if item.value is None:
            continue
        target = target_by_name.get(item.name)
        normalized_unit = item.unit.strip().lower().replace("µ", "u").replace("μ", "u") if item.unit else None
        expected = units.get(target)
        expected_normalized = expected.lower().replace("µ", "u") if expected else None
        if target is None or normalized_unit not in conversion or expected_normalized is None:
            raise InvalidMetalMeasurement("Unidade de metal medida inválida")
        value = item.value
        if normalized_unit != expected_normalized:
            value *= conversion[normalized_unit] / conversion[expected_normalized]
        values[target] = float(value)
        measured.append({"name": item.name, "value": item.value, "unit": item.unit})

    missing = set(TARGETS) - set(values)
    if missing:
        model_predictions = predict_metals(sample)
        predicted_values = {f"{item.codigo}, {item.unidadeMedida}": item.valor for item in model_predictions}
        for target in TARGETS:
            if target in missing:
                values[target] = predicted_values[target]
                name = target.split(",", 1)[0]
                estimated.append({"name": name, "value": predicted_values[target], "unit": units[target]})
    return create_metals(values), measured, estimated, [
        item for item in model_predictions if f"{item.codigo}, {item.unidadeMedida}" in missing
    ]


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
