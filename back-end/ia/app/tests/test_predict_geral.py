from datetime import datetime

import pytest

from app.services.PredictGeral.evaluation import avaliar
from app.services.PredictGeral.predictor import prever
from app.services.PredictGeral.preprocessing import preparar_coletas


def test_previsao_independente_e_datas_fora_de_ordem():
    coletas = [
        {"data": "2026-01-08", "ph": 7.2, "ponto": "A"},
        {"data": "2026-01-01", "temperatura": 20, "ponto": "A"},
        {"data": "2026-01-08", "temperatura": 22, "ponto": "A"},
    ]
    resultado = prever(coletas, 7)
    assert resultado["data_alvo"] == "2026-01-15T00:00:00"
    assert resultado["parametros"]["ph"]["valor"] == 7.2
    assert resultado["parametros"]["temperatura"]["valor"] == 22
    assert resultado["parametros"]["fosforo_total"]["valor"] is None
    assert len(preparar_coletas(coletas)) == 2


def test_contextos_separados_e_unidade_incompativel():
    coletas = [{"data": "2026-01-01", "ph": 7, "ponto": "A"},
               {"data": "2026-01-02", "ph": 9, "ponto": "B"}]
    with pytest.raises(ValueError):
        prever(coletas, 7)
    assert prever(coletas, 7, ponto="A")["parametros"]["ph"]["valor"] == 7
    with pytest.raises(ValueError):
        preparar_coletas([{"data": "2026-01-01", "ph": {"valor": 7, "unidade": "mg/L"}}])
    with pytest.raises(ValueError):
        preparar_coletas([{"data": "2026-01-01", "fosforo_total": {"valor": 1e308, "unidade": "mg/L"}}])


def test_tendencia_nao_e_exposta_sem_validacao():
    coletas = [{"data": "2026-01-01", "ph": 7},
               {"data": "2026-01-02", "ph": 8},
               {"data": "2026-01-03", "ph": 9}]
    assert prever(coletas, 7)["parametros"]["ph"]["metodo"] == "persistencia"


def test_backtest_respeita_horizonte_e_lacunas():
    coletas = [{"data": "2026-01-01", "ph": 7},
               {"data": "2026-01-08", "ph": 8},
               {"data": "2026-02-01", "ph": 9}]
    resultado = avaliar(coletas, horizontes=(7,), tolerancia_dias=1)
    metrica = resultado[((None, None), "ph", 7, "persistencia")]
    assert metrica["pares"] == 1
    assert metrica["mae"] == 1
    assert metrica["distancia_media_dias"] == 7
