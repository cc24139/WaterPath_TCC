from .metal import Metal
class Niquel(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L",limite=25):
        super().__init__("Níquel", valor, unidadeMedida,limite)

    def mensagem(self):
        return (
            "A concentração de níquel ultrapassa o limite de qualidade da água. "
            "A exposição elevada pode causar efeitos tóxicos em organismos "
            "aquáticos e favorecer a acumulação do elemento em organismos "
            "expostos."
        )
