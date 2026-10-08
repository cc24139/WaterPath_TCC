"""Observações de metais; independentes das estimativas do modelo."""
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field, model_validator

MetalName = Literal["Fe", "Mn", "Cr", "Ni", "Cu", "Zn", "Cd", "Pb"]


class MedicaoMetal(BaseModel):
    model_config = ConfigDict(allow_inf_nan=False, strict=True, extra="forbid")
    name: MetalName
    value: float | None = Field(default=None, ge=0)
    unit: str | None = Field(default=None, min_length=1)


def unique_metals(items):
    if len({item.name for item in items}) != len(items):
        raise ValueError("Cada metal deve aparecer uma única vez")
    return items


class ReferenciaMetais(BaseModel):
    model_config = ConfigDict(strict=True, extra="forbid")
    data: str | None = None
    metais_pesados: list[MedicaoMetal] = Field(default_factory=list, max_length=8)

    @model_validator(mode="after")
    def validate_metals(self):
        unique_metals(self.metais_pesados)
        return self
