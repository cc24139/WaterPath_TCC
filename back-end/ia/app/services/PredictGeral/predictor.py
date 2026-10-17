"""Previsão independente dos parâmetros em um contexto de coleta."""

from datetime import timedelta

from .baseline import persistencia
from .features import extrair_historicos
from .preprocessing import preparar_coletas
from .schema import PARAMETER_UNITS


def prever(coletas, horizonte_dias: int, *, ponto=None, profundidade=None) -> dict:
    if horizonte_dias not in (7, 15):
        raise ValueError("O horizonte deve ser 7 ou 15 dias")
    preparadas = preparar_coletas(coletas)
    contextos = {(c.ponto, c.profundidade) for c in preparadas}
    if ponto is None and profundidade is None and len(contextos) != 1:
        raise ValueError("Informe ponto e profundidade para selecionar um contexto")
    selecionadas = [c for c in preparadas if
                    (ponto is None or c.ponto == ponto) and
                    (profundidade is None or c.profundidade == profundidade)]
    if not selecionadas or len({(c.ponto, c.profundidade) for c in selecionadas}) != 1:
        raise ValueError("Contexto de coleta ausente ou ambíguo")
    referencia = selecionadas[-1].data
    historicos = extrair_historicos(selecionadas, referencia)
    resultado = {}
    for nome, unidade in PARAMETER_UNITS.items():
        historico = historicos.get(nome)
        if historico is None:
            resultado[nome] = {"valor": None, "unidade": unidade,
                               "metodo": None, "evidencia": "sem_historico"}
            continue
        resultado[nome] = {
            "valor": persistencia(historico),
            "unidade": unidade, "metodo": "persistencia",
            "ultima_medicao": historico.ultima_data.isoformat(),
            "idade_medicao_dias": historico.idade_dias,
            "evidencia": "limitada" if len(historico.valores) < 3 or historico.idade_dias > horizonte_dias else "historico_disponivel",
        }
    return {"data_referencia": referencia.isoformat(),
            "data_alvo": (referencia + timedelta(days=horizonte_dias)).isoformat(),
            "horizonte_dias": horizonte_dias, "ponto": selecionadas[0].ponto,
            "profundidade": selecionadas[0].profundidade,
            "parametros": resultado}
