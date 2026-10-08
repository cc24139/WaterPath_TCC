"""Variação temporal de observações, sem acrescentar limites ou regras de risco."""
from datetime import datetime
from decimal import Decimal
import math

# Unidades de concentração já usadas pelo projeto. Não presume equivalência de ppm.
UNIT_FACTORS = {"mg/l": Decimal("1"), "ug/l": Decimal("0.001")}


def unit_factor(unit):
    return UNIT_FACTORS.get(unit.strip().lower().replace("µ", "u").replace("μ", "u")) if unit else None


def instant(value):
    if not value:
        return None
    try:
        date = datetime.fromisoformat(value.replace("Z", "+00:00"))
        return date if date.utcoffset() is not None else None
    except ValueError:
        return None


def analyze_metal_variation(sample):
    current = {item.name: item for item in sample.metais_pesados or []}
    reference = sample.referencia_metais_pesados
    previous = {item.name: item for item in reference.metais_pesados} if reference else {}
    reference_date = reference.data if reference else None
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
