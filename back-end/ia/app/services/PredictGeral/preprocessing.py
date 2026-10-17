"""Valida e organiza medições históricas sem preencher lacunas artificiais."""

from collections.abc import Mapping
from dataclasses import fields
from datetime import date, datetime, time, timezone
from math import isfinite

from .schema import Coleta, PARAMETER_UNITS, PARAMETERS


def _date(value):
    if isinstance(value, datetime):
        parsed = value
    elif isinstance(value, date):
        parsed = datetime.combine(value, time.min)
    elif isinstance(value, str):
        try:
            parsed = datetime.fromisoformat(value.strip().replace("Z", "+00:00"))
        except ValueError as exc:
            raise ValueError(f"data inválida: {value!r}") from exc
    else:
        raise ValueError(f"data inválida: {value!r}")
    # Datas com fuso são convertidas ao mesmo referencial antes da ordenação.
    if parsed.tzinfo is not None:
        parsed = parsed.astimezone(timezone.utc).replace(tzinfo=None)
    return parsed


def _number(value, name):
    if isinstance(value, bool):
        raise ValueError(f"{name} deve ser um número finito")
    try:
        result = float(value)
    except (TypeError, ValueError) as exc:
        raise ValueError(f"{name} deve ser um número finito") from exc
    if not isfinite(result):
        raise ValueError(f"{name} deve ser um número finito")
    return result


def _unit(value):
    return str(value).strip().lower().replace("μ", "µ").replace(" ", "")


def _convert(name, value, unit):
    number = _number(value, name)
    if unit is None or _unit(unit) == _unit(PARAMETER_UNITS[name]):
        return number
    unit = _unit(unit)
    converted = None
    if name == "temperatura":
        if unit in ("c", "celsius"):
            converted = number
        if unit in ("°f", "f", "fahrenheit"):
            converted = (number - 32) * 5 / 9
        if unit in ("k", "kelvin"):
            converted = number - 273.15
    elif name == "ph":
        if unit in ("", "1", "adimensional", "semunidade"):
            converted = number
    elif name == "condutividade_eletrica":
        if unit in ("us/cm", "µs/cm"):
            converted = number
        if unit == "ms/cm":
            converted = number * 1000
        if unit == "s/m":
            converted = number * 10000
    elif name == "fosforo_total":
        if unit in ("µgp/l", "µg/l", "ug/l", "ugp/l"):
            converted = number
        if unit in ("mg/l", "mgp/l"):
            converted = number * 1000
        if unit in ("g/l", "gp/l"):
            converted = number * 1_000_000
    else:
        if unit in ("µg/l", "ug/l"):
            converted = number / 1000
        if unit == "g/l":
            converted = number * 1000
    if converted is None:
        raise ValueError(f"unidade incompatível para {name}: {unit!r}")
    if not isfinite(converted):
        raise ValueError(f"{name} convertido deve ser um número finito")
    return converted


def _coleta(item):
    if isinstance(item, Coleta):
        values = {field.name: getattr(item, field.name) for field in fields(Coleta)}
        units = {}
    elif isinstance(item, Mapping):
        values = dict(item)
        units = values.pop("unidades", {})
        if not isinstance(units, Mapping):
            raise ValueError("unidades deve mapear parâmetros para unidades")
    else:
        raise ValueError("cada coleta deve ser Coleta ou mapping")

    if "data" not in values:
        raise ValueError("coleta sem data")
    
    normalized = {"data": _date(values["data"])}
    point = values.get("ponto")
    normalized["ponto"] = None if point is None or str(point).strip() == "" else str(point).strip()
    depth = values.get("profundidade")
    normalized["profundidade"] = None if depth is None else _number(depth, "profundidade")
    if normalized["profundidade"] is not None and normalized["profundidade"] < 0:
        raise ValueError("profundidade deve ser não negativa")

    for name in PARAMETERS:
        raw = values.get(name)
        unit = units.get(name)
        if isinstance(raw, Mapping):
            unit = raw.get("unidade", unit)
            raw = raw.get("valor")
        converted = None if raw is None else _convert(name, raw, unit)
        if converted is not None:
            if name == "ph" and not 0 <= converted <= 14:
                raise ValueError("ph deve estar entre 0 e 14")
            if name != "temperatura" and converted < 0:
                raise ValueError(f"{name} deve ser não negativo")
        normalized[name] = converted
    return Coleta(**normalized)


def preparar_coletas(coletas):
    """Retorna coletas cronológicas; duplicatas do mesmo contexto são mescladas.

    Em conflito entre duas medições presentes no mesmo instante/ponto/profundidade,
    prevalece a última entrada. Valores ausentes não apagam medições existentes.
    Contextos diferentes são preservados como séries distintas no resultado.
    """
    if coletas is None or isinstance(coletas, (str, bytes, Mapping)):
        raise ValueError("coletas deve ser uma sequência de medições")
    try:
        iterator = iter(coletas)
    except TypeError as exc:
        raise ValueError("coletas deve ser uma sequência de medições") from exc
    merged = {}
    for item in iterator:
        coleta = _coleta(item)
        key = (coleta.data, coleta.ponto, coleta.profundidade)
        previous = merged.get(key)
        if previous is None:
            merged[key] = coleta
        else:
            for name in PARAMETERS:
                value = getattr(coleta, name)
                if value is not None:
                    setattr(previous, name, value)
    if not merged or not any(
        getattr(coleta, name) is not None
        for coleta in merged.values() for name in PARAMETERS
    ):
        raise ValueError("histórico sem medições válidas")
    return sorted(merged.values(), key=lambda c: (c.data, c.ponto or "", c.profundidade if c.profundidade is not None else -1))
