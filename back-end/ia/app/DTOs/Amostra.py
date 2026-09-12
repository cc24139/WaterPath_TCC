from pydantic import BaseModel
from typing import Optional


class Amostra(BaseModel):

    # Localização e identificação — opcionais
    estacao: Optional[str] = None
    latitude: Optional[str] = None
    longitude: Optional[str] = None
    data: Optional[str] = None
    profundidade: Optional[float] = None
    estacao_ano: Optional[str] = None

    # Features utilizadas pelo modelo — obrigatórias
    temperatura: float
    ph: float
    condutividade_eletrica: float
    cor: float
    solidos_suspensos_totais: float

    nitrogenio_amoniacal: float
    nitrito: float
    nitrato: float
    nitrogenio_total: float

    fosfato: float
    fosforo_total: float

    carbono_organico_total: float
    demanda_quimica_oxigenio_permanganato: float
    demanda_quimica_oxigenio_dicromato: float
    demanda_bioquimica_oxigenio_5_dias: float

    def decode(self):
        return [[
            self.temperatura,
            self.ph,
            self.solidos_suspensos_totais,
            self.cor,
            self.carbono_organico_total,
            self.demanda_quimica_oxigenio_permanganato,
            self.demanda_quimica_oxigenio_dicromato,
            self.demanda_bioquimica_oxigenio_5_dias,
            self.fosfato,
            self.fosforo_total,
            self.nitrogenio_amoniacal,
            self.nitrito,
            self.nitrato,
            self.nitrogenio_total,
            self.condutividade_eletrica,
            self.profundidade
        ]]