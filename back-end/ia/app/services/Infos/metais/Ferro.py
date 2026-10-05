from .metal import Metal


class Ferro(Metal):
    codigo = "Fe"

    def __init__(self, valor, unidadeMedida="mg/L", limite=0.3):
        super().__init__("Ferro", valor, unidadeMedida, limite)

    def mensagem(self):
        return (
            "A concentração de ferro ultrapassa o limite de qualidade da água. "
            "Concentrações elevadas podem alterar características do ambiente "
            "aquático e provocar deposição de compostos de ferro, afetando "
            "organismos e habitats."
        )
