"""Cria os metais a partir das colunas retornadas pelo modelo existente."""
from .Cobre import Cobre
from .Cromo import Cromo
from .Ferro import Ferro
from .Maganes import Maganes
from .Niquel import Niquel
from .Zinco import Zinco
from .cadmio import Cadmio
from .chumbo import Chumbo

METAL_CLASSES = {
    "Fe, mg/L": Ferro,
    "Mn, mg/L": Maganes,
    "Cr, µg/L": Cromo,
    "Ni, µg/L": Niquel,
    "Cu, µg/L": Cobre,
    "Zn, µg/L": Zinco,
    "Cd, µg/L": Cadmio,
    "Pb, µg/L": Chumbo,
}


def create_metals(predicted):
    return [METAL_CLASSES[column](value) for column, value in predicted.items()]
