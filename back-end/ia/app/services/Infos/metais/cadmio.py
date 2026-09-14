from .Metal import Metal
class Cadmio(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L",limite = 1):
        super().__init__("Cádmio", valor, unidadeMedida,limite)
    
    def mensagem(self):
        return (
            "A concentração de cádmio ultrapassa o limite de qualidade da água. "
            "O cádmio pode causar toxicidade em organismos aquáticos, favorecer "
            "a bioacumulação e afetar crescimento, reprodução e sobrevivência "
            "da biota exposta."
        )