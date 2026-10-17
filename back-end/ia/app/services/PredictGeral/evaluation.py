"""Backtests cronológicos sem uso de observações futuras nas previsões."""

from datetime import timedelta
from statistics import mean

from .baseline import persistencia, tendencia_robusta
from .features import extrair_historicos
from .preprocessing import preparar_coletas
from .schema import PARAMETER_UNITS


def avaliar(coletas, horizontes=(7, 15), tolerancia_dias: int = 2) -> dict:
    if tolerancia_dias < 0:
        raise ValueError("Tolerância não pode ser negativa")
    preparadas = preparar_coletas(coletas)
    resultados = {}
    for contexto in {(c.ponto, c.profundidade) for c in preparadas}:
        serie = [c for c in preparadas if (c.ponto, c.profundidade) == contexto]
        for horizonte in horizontes:
            if horizonte not in (7, 15):
                raise ValueError("O horizonte deve ser 7 ou 15 dias")
            for nome in PARAMETER_UNITS:
                pares = {"persistencia": [], "tendencia_robusta": []}
                for i, origem in enumerate(serie[:-1]):
                    alvo = origem.data + timedelta(days=horizonte)
                    candidatas = [c for c in serie[i + 1:] if getattr(c, nome) is not None and
                                  abs((c.data - alvo).total_seconds()) <= tolerancia_dias * 86400]
                    if not candidatas:
                        continue
                    observada = min(candidatas, key=lambda c: abs((c.data - alvo).total_seconds()))
                    historico = extrair_historicos(serie[:i + 1], origem.data).get(nome)
                    if historico is None:
                        continue
                    distancia = (observada.data - origem.data).total_seconds() / 86400
                    pares["persistencia"].append((abs(persistencia(historico) - getattr(observada, nome)), distancia))
                    tendencia = tendencia_robusta(historico, horizonte)
                    if tendencia is not None:
                        pares["tendencia_robusta"].append((abs(tendencia - getattr(observada, nome)), distancia))
                for metodo, amostras in pares.items():
                    resultados[(contexto, nome, horizonte, metodo)] = {
                        "pares": len(amostras),
                        "mae": mean(erro for erro, _ in amostras) if amostras else None,
                        "distancia_media_dias": mean(distancia for _, distancia in amostras) if amostras else None,
                        "tolerancia_dias": tolerancia_dias,
                    }
    return resultados
