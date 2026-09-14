from .Metal import Metal
class Chumbo(Metal):
    def __init__(self, valor, unidadeMedida = "µg/L", limite = 10):
        super().__init__("Chumbo", valor, unidadeMedida,limite)

    
    def mensagem(self):
        return "Alerta de contaminação por chumbo (Pb): a concentração detectada ultrapassa o limite estabelecido para águas doces Classe 1 e Classe 2 pela Resolução CONAMA nº 357/2005. A presença elevada de Pb representa risco de toxicidade para organismos aquáticos e pode favorecer sua acumulação em organismos expostos, especialmente em componentes bentônicos e peixes. A exposição pode provocar alterações fisiológicas, neurológicas, imunológicas, reprodutivas e de crescimento, dependendo da concentração, tempo de exposição e características físico-químicas da água. A ocorrência desse excedente indica necessidade de investigação da fonte de contaminação, avaliação da qualidade do sedimento e acompanhamento da biota e da evolução temporal da concentração."