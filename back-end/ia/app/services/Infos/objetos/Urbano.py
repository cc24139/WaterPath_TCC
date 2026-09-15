from .Impact import Impact
from services.Infos.metais.chumbo import Chumbo
from services.Infos.metais.Zinco import Zinco
from services.Infos.metais.cadmio import Cadmio
from services.Infos.metais.Cromo import Cromo
from services.Infos.metais.Niquel import Niquel
from services.Infos.metais.Cobre import Cobre
class Urbano(Impact):
    def __init__(self,obj,confidence=0.1,name="Urbano"):
        super().__init__(obj,confidence,name)
        
        
    #Retorna em formato json para api
    def getImpact(self, heavyMetais=None):
        return {
            "name": self.name,
            "confidence": self.confidence,
            "impact": self.impact(heavyMetais)
        }
        
    def mensagem(self):
        return ("Foi detectado a presença de ambiente urbano no lago, caso não ocorra o devido tratamento pode impactar na presença de"
    "Cromo,Niquel,Cobre,Zinco,Cadmio e Chumbo!")
    
    def heavyMetais(self,metal,value):
        if metal not in self._AllMetals:
            return None
        match metal:
            case "Cr, µg/L":
                return Cromo(value)

            case "Ni, µg/L":
                return Niquel(value)

            case "Cu, µg/L":
                return Cobre(value)

            case "Zn, µg/L":
                return Zinco(value)

            case "Cd, µg/L":
                return Cadmio(value)

            case "Pb, µg/L":
                return Chumbo(value)