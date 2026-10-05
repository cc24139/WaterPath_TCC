"""Contrato único das entradas e unidades da regressão de metais v2."""

FIELDS = {
    "temperatura": ("T, °C", "°C"),
    "ph": ("рН", "pH"),
    "condutividade_eletrica": ("EC, µS/cm", "µS/cm"),
    "oxigenio_dissolvido": ("DO, mg/L", "mg/L"),
    "solidos_suspensos_totais": ("TSS, mg/L", "mg/L"),
    "carbono_organico_total": ("TOC, mg/L", "mg/L"),
    "nitrogenio_total": ("TN, mg/L", "mg/L"),
    "fosforo_total": ("TP, µg/L", "µg P/L"),
}
FEATURES = [column for column, _ in FIELDS.values()]
TARGETS = ["Fe, mg/L", "Mn, mg/L", "Cr, µg/L", "Ni, µg/L",
           "Cu, µg/L", "Zn, µg/L", "Cd, µg/L", "Pb, µg/L"]
REQUIRED_FIELDS = ("temperatura", "ph", "condutividade_eletrica", "oxigenio_dissolvido")
