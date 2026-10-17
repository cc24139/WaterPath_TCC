"""Referências calculadas a partir do histórico recebido."""

from statistics import median

from .features import HistoricoParametro


def persistencia(historico: HistoricoParametro) -> float:
    return historico.ultimo_valor


def tendencia_robusta(historico: HistoricoParametro, horizonte_dias: int) -> float | None:
    """Mediana das inclinações recentes, limitada pela variação observada."""
    pontos = historico.valores[-5:]
    if len(pontos) < 3:
        return None
    inclinacoes = [(b[1] - a[1]) / ((b[0] - a[0]).total_seconds() / 86400)
                   for a, b in zip(pontos, pontos[1:]) if b[0] > a[0]]
    if len(inclinacoes) < 2:
        return None
    amplitude = max(v for _, v in pontos) - min(v for _, v in pontos)
    delta = median(inclinacoes) * (horizonte_dias + historico.idade_dias)
    limite = max(amplitude, abs(historico.ultimo_valor) * 0.1)
    delta = max(-limite, min(limite, delta))
    previsto = historico.ultimo_valor + delta
    if historico.nome == "ph":
        return max(0.0, min(14.0, previsto))
    if historico.nome != "temperatura":
        return max(0.0, previsto)
    return previsto
