"""Contrato interno para séries históricas e previsões de qualidade da água."""

from dataclasses import dataclass
from datetime import date, datetime
from typing import Optional


# Os nomes e as unidades seguem o contrato de entrada do serviço de metais.
PARAMETER_UNITS = {
    "temperatura": "°C",
    "ph": "pH",
    "condutividade_eletrica": "µS/cm",
    "oxigenio_dissolvido": "mg/L",
    "solidos_suspensos_totais": "mg/L",
    "carbono_organico_total": "mg/L",
    "fosforo_total": "µg/L",
}
PARAMETERS = tuple(PARAMETER_UNITS)


@dataclass
class Coleta:
    """Uma coleta; None representa parâmetro não medido, inclusive os quatro básicos."""

    data: date | datetime | str
    temperatura: Optional[float] = None
    ph: Optional[float] = None
    condutividade_eletrica: Optional[float] = None
    oxigenio_dissolvido: Optional[float] = None
    solidos_suspensos_totais: Optional[float] = None
    carbono_organico_total: Optional[float] = None
    fosforo_total: Optional[float] = None
    ponto: Optional[str] = None
    profundidade: Optional[float] = None  # metros

    def to_dict(self):
        """Representação com os rótulos legados usados pelo modelo de metais."""
        return {
            "T, °C": self.temperatura,
            "рН": self.ph,
            "EC, µS/cm": self.condutividade_eletrica,
            "DO, mg/L": self.oxigenio_dissolvido,
            "TSS, mg/L": self.solidos_suspensos_totais,
            "TOC, mg/L": self.carbono_organico_total,
            "TP, µg/L": self.fosforo_total,
        }
