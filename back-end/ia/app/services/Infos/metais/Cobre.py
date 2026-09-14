from .Metal import Metal
class Cobre(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L",limite = 9):
        super().__init__("Cobre", valor, unidadeMedida,limite)

    
    def mensagem(self):
        return (
            "A concentração de cobre ultrapassa o limite de qualidade da água. "
            "Concentrações elevadas podem causar toxicidade para organismos "
            "aquáticos, afetando processos fisiológicos e a sobrevivência de "
            "espécies sensíveis."
        )