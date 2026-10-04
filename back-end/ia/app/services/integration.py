"""Cruza evidências visuais e predições sem alterar concentrações estimadas."""
from services.Infos.objetos.Drenagem import Drenagem
from services.Infos.objetos.Lixo import Lixo
from services.Infos.objetos.Urbano import Urbano


def build_visual_report(predictions, metals, names, image_shape):
    height, width = image_shape[:2]
    handlers = {"lixo": Lixo, "urbano": Urbano, "drainage connection": Drenagem}
    grouped = {}
    for box, confidence, label in predictions:
        name = str(names.get(label, f"class_{label}"))
        coordinates = [float(value) for value in box]
        x1, y1, x2, y2 = coordinates
        area_ratio = max(0.0, x2 - x1) * max(0.0, y2 - y1) / (width * height)
        if name not in grouped:
            handler = handlers.get(name.lower())
            interpretation = (
                handler(coordinates, float(confidence), name).impact(metals)
                if handler else "Classe visual sem regra de interpretação cadastrada."
            )
            grouped[name] = {
                "classId": int(label), "className": name, "count": 0,
                "maxConfidence": 0.0, "meanBoxAreaRatio": 0.0,
                "interpretation": interpretation,
            }
        stats = grouped[name]
        count = stats["count"]
        stats["meanBoxAreaRatio"] = (stats["meanBoxAreaRatio"] * count + area_ratio) / (count + 1)
        stats["count"] += 1
        stats["maxConfidence"] = max(stats["maxConfidence"], float(confidence))
    return {"detections": list(grouped.values())}


def structured_metals(predicted):
    return [
        {"name": key.split(",", 1)[0], "value": float(value), "unit": key.split(",", 1)[1].strip()}
        for key, value in predicted.items()
    ]
