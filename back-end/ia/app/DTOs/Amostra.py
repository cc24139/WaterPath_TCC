from dataclasses import dataclass
from datetime import date
from typing import Optional


@dataclass
class Amostra:
    # Localização e identificação
    estacao: str
    latitude: float
    longitude: float
    data: date
    profundidade: Optional[float]
    estacao_ano: str

    # Parâmetros físico-químicos
    temperatura: Optional[float]
    ph: Optional[float]
    condutividade_eletrica: Optional[float]
    cor: Optional[float]
    solidos_suspensos_totais: Optional[float]

    # Íons
    sodio: Optional[float]
    cloreto: Optional[float]

    # Metais
    ferro: Optional[float]
    manganes: Optional[float]
    cobre: Optional[float]
    niquel: Optional[float]
    cromo: Optional[float]
    zinco: Optional[float]
    cadmio: Optional[float]
    chumbo: Optional[float]

    # Nitrogênio
    nitrogenio_amoniacal: Optional[float]
    nitrito: Optional[float]
    nitrato: Optional[float]
    nitrogenio_total: Optional[float]

    # Fósforo
    fosfato: Optional[float]
    fosforo_total: Optional[float]

    # Matéria orgânica
    carbono_organico_total: Optional[float]
    demanda_quimica_oxigenio_permanganato: Optional[float]
    demanda_quimica_oxigenio_dicromato: Optional[float]
    demanda_bioquimica_oxigenio_5_dias: Optional[float]