"""Variação temporal de observações ou estimativas, sem alterar regras de risco."""
from datetime import datetime
from decimal import Decimal
from functools import lru_cache
from pathlib import Path
import math
import joblib

from DTOs.Amostra import Amostra
from DTOs.MetaisPesados import MedicaoMetal
from services import metals as metal_service
# Unidades de concentração já usadas pelo projeto. Não presume equivalência de ppm.
UNIT_FACTORS = {"mg/l": Decimal("1"), "ug/l": Decimal("0.001")}


@lru_cache(maxsize=1)
def load_variation_model():
    return joblib.load(Path(__file__).with_name("metal_variation_model.joblib"))


def unit_factor(unit):
    return UNIT_FACTORS.get(unit.strip().lower().replace("µ", "u").replace("μ", "u")) if unit else None

def predizerMetal(metal,sample):
    if not sample or not sample.metais_pesados:
        return None
    for item in sample.metais_pesados:
        if item.name == metal:
            if item.value is None or item.unit is None:
                return None
            factor = unit_factor(item.unit)
            if factor is None:
                return None
            return load_variation_model().predict([[item.value * float(factor)]])[0]
    return None

def instant(value):
    if not value:
        return None
    try:
        date = datetime.fromisoformat(value.replace("Z", "+00:00"))
        return date if date.utcoffset() is not None else None   
    except ValueError:
        return None


_HISTORY_FEATURES = {
    "Temperatura": ("temperatura", "°c"),
    "Ph": ("ph", "ph"),
    "CondutividadeEletrica": ("condutividade_eletrica", "us/cm"),
    "OxigenioDissolvido": ("oxigenio_dissolvido", "mg/l"),
    "SolidosSuspensosTotais": ("solidos_suspensos_totais", "mg/l"),
    "CarbonoOrganicoTotal": ("carbono_organico_total", "mg/l"),
    "FosforoTotal": ("fosforo_total", "ug/l"),
}
_REQUIRED_HISTORY_FEATURES = {"temperatura", "ph", "condutividade_eletrica", "oxigenio_dissolvido"}


def _historical_sample(record):
    """Build the model input from the measurements retained in collection_context."""
    fields = {"data": record["collectedAt"]}
    present = set()
    for measurement in record["measurements"]:
        mapping = _HISTORY_FEATURES.get(measurement["code"])
        if mapping is None or measurement["censored"] or measurement["value"] is None:
            continue
        field, expected_unit = mapping
        unit = measurement["unit"].strip().lower().replace("µ", "u").replace("μ", "u")
        value = measurement["value"]
        if field == "fosforo_total" and unit == "mg/l":
            value *= 1000
        elif unit != expected_unit:
            continue
        fields[field] = value
        present.add(field)
    if not _REQUIRED_HISTORY_FEATURES.issubset(present):
        return None
    try:
        return Amostra.model_validate(fields)
    except (TypeError, ValueError):
        return None


def _observations(predicted):
    return [MedicaoMetal(name=item.codigo, value=item.valor, unit=item.unidadeMedida)
            for item in predicted]


def _predicted_history_reference(sample, collection_context):
    if not collection_context:
        return None, None
    current_time = instant(sample.data)
    if current_time is None:
        return None, None
    predictions = []
    # Predict each historical collection with usable required measurements; the
    # latest usable collection before the current sample becomes the reference.
    for record in collection_context.get("history", []):
        date = record.get("collectedAt")
        collected_at = instant(date)
        if collected_at is None or collected_at >= current_time:
            continue
        historical_sample = _historical_sample(record)
        if historical_sample is None:
            continue
        try:
            predicted = metal_service.predict_metals(historical_sample)
        except (ValueError, TypeError, KeyError):
            continue
        predictions.append((collected_at, record["collectionId"], date, _observations(predicted)))
    if not predictions:
        return None, None
    _, _, reference_date, reference_metals = max(predictions, key=lambda item: (item[0], item[1]))
    return reference_date, reference_metals


def _compare_metal_sets(sample, current_source, previous_source, reference_date):
    current = {item.name: item for item in current_source or []}
    previous = {item.name: item for item in previous_source or []}
    current_time, previous_time = instant(sample.data), instant(reference_date)
    items = []
    for name in sorted(current.keys() | previous.keys()):
        now, before = current.get(name), previous.get(name)
        item = {
            "name": name,
            "currentValue": now.value if now else None,
            "currentUnit": now.unit if now else None,
            "previousValue": before.value if before else None,
            "previousUnit": before.unit if before else None,
            "variation": "indeterminada", "delta": None,
            "unit": now.unit if now else None,
            "reason": None,
        }
        if now is None or before is None or now.value is None or before.value is None:
            item["reason"] = "dados_ausentes"
        elif current_time is None or previous_time is None:
            item["reason"] = "referencia_temporal_ausente_ou_invalida"
        elif previous_time >= current_time:
            item["reason"] = "referencia_nao_anterior"
        elif not now.unit or not now.unit.strip() or not before.unit or not before.unit.strip():
            item["reason"] = "unidades_ausentes"
        else:
            current_factor, previous_factor = unit_factor(now.unit), unit_factor(before.unit)
            if current_factor is None or previous_factor is None:
                item["reason"] = "unidades_incompativeis"
            else:
                # Decimal evita que uma conversão exata gere um falso aumento.
                difference = (Decimal(str(now.value)) * current_factor
                              - Decimal(str(before.value)) * previous_factor) / current_factor
                delta = float(difference)
                if not math.isfinite(delta) or (difference != 0 and delta == 0):
                    item["reason"] = "variacao_nao_representavel"
                else:
                    item["variation"] = "aumento" if difference > 0 else "reducao" if difference < 0 else "estabilidade"
                    item["delta"] = delta
        items.append(item)
    comparable = sum(item["variation"] != "indeterminada" for item in items)
    status = "indeterminada" if not comparable else "completa" if comparable == len(items) else "parcial"
    return {
        "status": status, "currentDate": sample.data, "referenceDate": reference_date,
        "metals": items,
        "message": "Variação determinada para todos os metais informados." if status == "completa" else
                   "Não é possível determinar a variação de todos os metais: faltam dados comparáveis.",
    }


def analyze_metal_variation(sample, history=None, collection_context=None, predicted_current=None):
    reference = sample.referencia_metais_pesados
    observed_current = sample.metais_pesados
    observed_previous = reference.metais_pesados if reference else None
    observed_reference_date = reference.data if reference else None
    observed = _compare_metal_sets(sample, observed_current, observed_previous, observed_reference_date)
    if observed["status"] != "indeterminada":
        return observed

    # Only attempt model-based comparison when the API actually sent history.
    # Never mix measured values with model estimates in the fallback.
    records = collection_context.get("history", []) if collection_context else []
    if not records or predicted_current is None:
        return observed

    reference_date, historical_prediction = _predicted_history_reference(sample, collection_context)
    estimated = _compare_metal_sets(
        sample,
        _observations(predicted_current),
        historical_prediction,
        reference_date,
    )
    estimated["source"] = "predicoes_modelo_historico"
    if reference_date is None:
        estimated["message"] = (
            "A variação permaneceu indeterminada: o histórico foi recebido, mas nenhuma coleta anterior "
            "tinha medições válidas para estimar os metais."
        )
    else:
        estimated["message"] = (
            "Comparação baseada em previsões do modelo para a coleta atual e a coleta histórica de referência."
            if estimated["status"] == "completa" else
            "A comparação baseada em previsões do modelo ficou incompleta: faltam dados comparáveis."
        )
    return estimated
