"""Resumo de observações reais disponíveis em uma data de referência."""

from dataclasses import dataclass
from datetime import datetime
from statistics import median

from .schema import PARAMETER_UNITS, Coleta


@dataclass(frozen=True)
class HistoricoParametro:
    nome: str
    ultimo_valor: float
    ultima_data: datetime
    idade_dias: float
    valores: tuple[tuple[datetime, float], ...]
    intervalos_dias: tuple[float, ...]
    variacao_recente_por_dia: float | None


def extrair_historicos(coletas: list[Coleta], data_referencia: datetime) -> dict[str, HistoricoParametro]:
    """Não consulta observações posteriores à referência nem preenche lacunas."""
    resultado = {}
    for nome in PARAMETER_UNITS:
        valores = tuple((c.data, getattr(c, nome)) for c in coletas
                       if c.data <= data_referencia and getattr(c, nome) is not None)
        if not valores:
            continue
        intervalos = tuple((b[0] - a[0]).total_seconds() / 86400
                           for a, b in zip(valores, valores[1:]))
        recentes = valores[-5:]
        inclinacoes = [(b[1] - a[1]) / ((b[0] - a[0]).total_seconds() / 86400)
                       for a, b in zip(recentes, recentes[1:]) if b[0] > a[0]]
        resultado[nome] = HistoricoParametro(
            nome, valores[-1][1], valores[-1][0],
            (data_referencia - valores[-1][0]).total_seconds() / 86400,
            valores, intervalos, median(inclinacoes) if inclinacoes else None,
        )
    return resultado
