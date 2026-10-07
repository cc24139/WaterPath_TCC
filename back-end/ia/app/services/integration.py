"""Cruza evidências visuais e predições sem alterar concentrações estimadas."""


def interpretation(image, metals):
    return image.impact(metals)


def build_visual_report(images, metals, image_shape):
    height, width = image_shape[:2]
    grouped = {} # Serve para gerar resposta para cada classe ao invés de pegar objetos da imagem individualmente
    for image in images:
        name = image.name
        x1, y1, x2, y2 = image.obj
        area_ratio = max(0.0, x2 - x1) * max(0.0, y2 - y1) / (width * height)
        if name not in grouped:
            grouped[name] = {
                "indicatesRisk": image.indica_risco(),
                "classId": image.class_id, "className": name, "count": 0,
                "maxConfidence": 0.0, "meanBoxAreaRatio": 0.0,
                "interpretation": interpretation(image, metals),
            }
        stats = grouped[name]
        count = stats["count"]
        stats["meanBoxAreaRatio"] = (stats["meanBoxAreaRatio"] * count + area_ratio) / (count + 1)
        stats["count"] += 1
        stats["maxConfidence"] = max(stats["maxConfidence"], image.confidence)
    return {"detections": list(grouped.values())}


def structured_metals(predicted):
    return [
        {"name": metal.codigo, "value": metal.valor, "unit": metal.unidadeMedida}
        for metal in predicted
    ]
