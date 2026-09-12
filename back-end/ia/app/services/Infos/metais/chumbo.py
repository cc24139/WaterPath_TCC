from .Metal import Metal
class Chumbo(Metal):
    def __init__(self, valor, unidadeMedida = "mg/l"):
        super().__init__("Chumbo", valor, unidadeMedida)

    def limite(self):
        return 0.1

    def mensagem(self):
        return "A concentração de chumbo no ambiente pode causar problemas de saúde significativos."