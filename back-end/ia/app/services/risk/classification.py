
import json
from fastapi import HTTPException
from pydantic import BaseModel, ConfigDict, Field, ValidationError

RULE_VERSION = "waterpath-risk-v1"
RISK_LABELS = {1: "baixo", 2: "moderado", 3: "alto"}


class HistoryItem(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True)
    prediction_id: int = Field(alias="predictionId", gt=0)
    collection_id: int = Field(alias="coletaId", gt=0)
    base_risk_level: int = Field(alias="baseRiskLevel", ge=1, le=3)


def parse_history(data):
    try:
        records = json.loads(data)
        if not isinstance(records, list) or len(records) > 5:
            raise ValueError("O histórico deve conter até cinco coletas")
        history = [HistoryItem.model_validate(record) for record in records]
        if len({item.collection_id for item in history}) != len(history):
            raise ValueError("Cada coleta deve aparecer uma única vez")
        if len({item.prediction_id for item in history}) != len(history):
            raise ValueError("Cada predição deve aparecer uma única vez")
        return history
    except (ValueError, ValidationError) as exc:
        raise HTTPException(status_code=422, detail="Histórico de risco inválido") from exc


def classify_risk(images, sample, history):
    visual_classes = sorted({image.name for image in images if image.indica_risco()})
    reasons = []
    if visual_classes:
        reasons.append(f"Sinal visual: {', '.join(visual_classes)}.")
    measurement_alerts = []
    if not 6 <= sample.ph <= 9:
        measurement_alerts.append(f"pH {sample.ph:g} fora da faixa de referência 6–9.")
    if sample.oxigenio_dissolvido < 5:
        measurement_alerts.append(f"Oxigênio dissolvido {sample.oxigenio_dissolvido:g} mg/L abaixo de 5 mg/L.")
    reasons.extend(measurement_alerts)
    base_level = 1 + int(bool(visual_classes)) + int(bool(measurement_alerts))
    # Usa o nível anterior ao ajuste para não realimentar o peso do histórico.
    previous_alerts = sum(item.base_risk_level >= 2 for item in history)
    level = min(3, base_level + int(previous_alerts >= 3))
    if not visual_classes and not measurement_alerts:
        reasons.append("Sem sinais atuais pelos critérios de triagem avaliados.")
    if previous_alerts >= 3:
        reasons.append(f"Recorrência: {previous_alerts} das {len(history)} coletas anteriores apresentaram alerta.")
    result = getResponse(level, base_level, RISK_LABELS[level], reasons, history)
    # Retrato dos valores efetivamente consultados pela regra, sem metais estimados.
    result["riskInputs"] = {
        "ph": sample.ph,
        "oxigenio_dissolvido": sample.oxigenio_dissolvido,
        "visualClasses": visual_classes,
    }
    return result
    
    
def getResponse(level, base_level, risk_label, risk_reasons, history):
    return {
        "riskLevel": level,
        "baseRiskLevel": base_level,
        "riskLabel": risk_label,
        "riskReasons": risk_reasons,
        "riskRuleVersion": RULE_VERSION,
        "history": {
            "evaluatedCollections": len(history),
            "alertCollections": sum(item.base_risk_level >= 2 for item in history),
            "adjustment": level - base_level,
            "samples": [item.model_dump(by_alias=True) for item in history],
        },
    }


