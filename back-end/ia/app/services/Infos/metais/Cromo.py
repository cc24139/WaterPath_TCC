from .metal import Metal
class Cromo(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L",limite=50):
        super().__init__("Cromo", valor, unidadeMedida,limite)
    
    def mensagem(self):
        return (
            "A concentração de cromo ultrapassa o limite de qualidade da água. "
            "A exposição a formas tóxicas de cromo pode causar efeitos adversos "
            "em organismos aquáticos, comprometendo processos fisiológicos e "
            "o equilíbrio do ecossistema."
        )
