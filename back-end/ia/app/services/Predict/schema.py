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
LEGACY_FEATURES = [
    "T, °C", "рН", "TSS, mg/L", "Color, mg Pt-Co/L", "TOC, mg/L",
    "CODMn, mg О/L", "CODCr, мгО/л", "BOD5, mg О2/L", "PO4-P, µg/L",
    "TP, µg/L", "NH4-N, mgN/L", "NO2-N, mgN/L", "NO3-N, mgN/L",
    "TN, mg/L", "EC, μS/сm at 25 °C", "Depth, m",
]
