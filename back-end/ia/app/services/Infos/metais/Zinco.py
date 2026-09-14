from .Metal import Metal
class Zinco(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L",limite = 180):
        super().__init__("Zinco", valor, unidadeMedida,limite)
        
    
    def mensagem(self):
        return (
            "A concentração de zinco ultrapassa o limite de qualidade da água. "
            "Embora seja um elemento essencial em baixas concentrações, níveis "
            "elevados podem causar efeitos tóxicos em organismos aquáticos e "
            "comprometer o equilíbrio do ecossistema."
        )