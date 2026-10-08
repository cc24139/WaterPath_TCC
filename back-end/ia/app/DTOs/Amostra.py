from pydantic import BaseModel, ConfigDict, Field, field_validator
from typing import Optional
from DTOs.MetaisPesados import MedicaoMetal, ReferenciaMetais, unique_metals
from services.Predict.schema import FIELDS


class Amostra(BaseModel):
    model_config = ConfigDict(allow_inf_nan=False, strict=True, extra="forbid")

    # Localização e identificação — opcionais
    estacao: Optional[str] = None
    latitude: Optional[str] = None
    longitude: Optional[str] = None
    data: Optional[str] = None
    # Profundidade é metadado; não participa da regressão v2.
    profundidade: Optional[float] = Field(default=None, ge=0)
    estacao_ano: Optional[str] = None

    # Quatro medições essenciais, na unidade publicada por /predict/infos.
    temperatura: float
    ph: float = Field(ge=0, le=14)
    condutividade_eletrica: float = Field(ge=0)
    oxigenio_dissolvido: float = Field(ge=0, description="Oxigênio dissolvido medido, em mg/L; não saturação em %")
    solidos_suspensos_totais: Optional[float] = Field(default=None, ge=0)
    carbono_organico_total: Optional[float] = Field(default=None, ge=0)
    fosforo_total: Optional[float] = Field(default=None, ge=0, description="µg P/L")

    # Concentrações observadas e referência anterior; não entram na regressão.
    metais_pesados: list[MedicaoMetal] | None = Field(default=None, max_length=8)
    referencia_metais_pesados: ReferenciaMetais | None = None

    @field_validator("metais_pesados")
    @classmethod
    def validate_metals(cls, items):
        return unique_metals(items) if items is not None else None

    def decode(self, columns=None):
        import math

        values = {column: getattr(self, field, None) for field, (column, _) in FIELDS.items()}
        return [[values[column] if values[column] is not None else math.nan for column in (columns or list(values))]]
