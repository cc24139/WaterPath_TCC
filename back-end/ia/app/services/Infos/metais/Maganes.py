from .Metal import Metal
class Maganes(Metal):
    def __init__(self, valor, unidadeMedida = "mg/L",limite = 0.1):
        super().__init__("Maganês", valor, unidadeMedida,limite)

    def limite(self):
        return 0.1 #resolução do conama
    
    def mensagem(self):
        return (
            "A concentração de manganês ultrapassa o limite de qualidade da água. "
            "Níveis elevados podem afetar organismos aquáticos e indicar alteração "
            "nas condições geoquímicas ou contaminação do corpo hídrico."
        )