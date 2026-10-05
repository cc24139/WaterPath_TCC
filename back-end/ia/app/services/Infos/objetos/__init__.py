"""Representa cada detecção com a classe correspondente ao nome do modelo."""
from .Drenagem import Drenagem
from .Impact import Impact
from .Lixo import Lixo
from .Urbano import Urbano

IMAGE_CLASSES = {"lixo": Lixo, "urbano": Urbano, "drainage connection": Drenagem}


def create_images(predictions, names):
    images = []
    for box, confidence, label in predictions:
        name = str(names.get(label, f"class_{label}"))
        image_class = IMAGE_CLASSES.get(name.lower(), Impact)
        images.append(image_class(box, confidence, name, class_id=int(label)))
    return images
