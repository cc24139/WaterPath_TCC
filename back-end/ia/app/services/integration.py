"""Cruza evidências visuais e predições sem alterar concentrações estimadas."""

# Mantém as interpretações e referências já usadas pelo projeto.
VISUAL_MESSAGES = {
    "lixo": "Foi detectado a presença de lixo no lago, isso pode impactar na presença de zinco,chumbo e cadmio",
    "urbano": "Foi detectado a presença de ambiente urbano no lago, caso não ocorra o devido tratamento pode impactar na presença deCromo,Niquel,Cobre,Zinco,Cadmio e Chumbo!",
    "drainage connection": "Foi detectada uma conexão de drenagem. A imagem não permite determinar a composição do lançamento; investigue sua origem e relacione-o à coleta.",
}
METAL_REFERENCES = {
    "Cr, µg/L": ("Cromo", 50), "Ni, µg/L": ("Níquel", 25),
    "Cu, µg/L": ("Cobre", 9), "Zn, µg/L": ("Zinco", 180),
    "Cd, µg/L": ("Cádmio", 1), "Pb, µg/L": ("Chumbo", 10),
}
RELATED_METALS = {
    "lixo": {"Zn, µg/L", "Pb, µg/L", "Cd, µg/L"},
    "urbano": set(METAL_REFERENCES),
    "drainage connection": set(),
}


def interpretation(class_name, metals):
    name = class_name.lower()
    if name not in VISUAL_MESSAGES:
        return "Classe visual sem regra de interpretação cadastrada."
    message = VISUAL_MESSAGES[name]
    if not metals:
        return message
    alerts = ""
    for metal, value in metals.items():
        if metal in RELATED_METALS[name]:
            display_name, limit = METAL_REFERENCES[metal]
            if float(value) > limit:
                unit = metal.split(",", 1)[1].strip()
                alerts += f"{display_name} previsto acima do limite de referência ({float(value)} {unit}) \n"
    return f"{message} \n {alerts}"


def build_visual_report(predictions, metals, names, image_shape):
    height, width = image_shape[:2]
    grouped = {}
    for box, confidence, label in predictions:
        name = str(names.get(label, f"class_{label}"))
        coordinates = [float(value) for value in box]
        x1, y1, x2, y2 = coordinates
        area_ratio = max(0.0, x2 - x1) * max(0.0, y2 - y1) / (width * height)
        if name not in grouped:
            grouped[name] = {
                "classId": int(label), "className": name, "count": 0,
                "maxConfidence": 0.0, "meanBoxAreaRatio": 0.0,
                "interpretation": interpretation(name, metals),
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
