"""Contrato do histórico observado, separado da janela de níveis de risco."""
import json
from datetime import datetime

from fastapi import HTTPException
from pydantic import BaseModel, ConfigDict, Field, ValidationError


class StrictRecord(BaseModel):
    model_config = ConfigDict(extra="forbid", strict=True, allow_inf_nan=False)


class Measurement(StrictRecord):
    code: str = Field(min_length=1)
    value: float | None
    unit: str = Field(min_length=1)
    censored: bool
    limit: float | None


class MetalObservation(StrictRecord):
    name: str = Field(min_length=1)
    value: float
    unit: str = Field(min_length=1)


class Collection(StrictRecord):
    waterBodyId: int = Field(gt=0)
    collectionId: int = Field(gt=0)
    collectedAt: str
    measurements: list[Measurement]
    metals: list[MetalObservation]


class CollectionContext(StrictRecord):
    waterBodyId: int = Field(gt=0)
    currentCollectionId: int = Field(gt=0)
    currentCollectedAt: str
    history: list[Collection]


def instant(text):
    value = datetime.fromisoformat(text.replace("Z", "+00:00"))
    if value.utcoffset() is None:
        raise ValueError("Instante sem fuso horário")
    return value


def parse_collection_context(data, sample, risk_history):
    if data is None:
        return None
    try:
        def unique_fields(pairs):
            result = {}
            for key, value in pairs:
                if key in result:
                    raise ValueError("Campo duplicado no contexto")
                result[key] = value
            return result

        raw = json.loads(data, object_pairs_hook=unique_fields)
        context = CollectionContext.model_validate(raw)
        current_date = instant(context.currentCollectedAt)
        if sample is None or instant(sample.data) != current_date:
            raise ValueError("Amostra diferente da coleta atual")
        ids = {context.currentCollectionId}
        previous_key = None
        for record in context.history:
            key = (instant(record.collectedAt), record.collectionId)
            if (record.waterBodyId != context.waterBodyId or record.collectionId in ids
                    or key[0] >= current_date or (previous_key is not None and key <= previous_key)):
                raise ValueError("Histórico fora de ordem, duplicado ou de outro lago")
            if len({m.code for m in record.measurements}) != len(record.measurements):
                raise ValueError("Medição duplicada")
            ids.add(record.collectionId)
            previous_key = key
        window = {record.collectionId for record in context.history[-5:]}
        if any(item.collection_id not in window for item in risk_history):
            raise ValueError("Níveis de risco fora da janela histórica")
        # Conserva os mesmos instantes/unidades para auditoria e validação na API principal.
        return raw
    except (ValueError, TypeError, AttributeError, ValidationError) as exc:
        raise HTTPException(status_code=422, detail="Contexto de coletas inválido") from exc
