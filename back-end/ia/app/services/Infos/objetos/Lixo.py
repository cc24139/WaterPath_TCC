from .Impact import Impact
from services.Infos.metais.chumbo import Chumbo
from services.Infos.metais.Zinco import Zinco
from services.Infos.metais.cadmio import Cadmio
class Lixo(Impact):
    def __init__(self,obj,confidence=0.1,name="lixo"):
        super().__init__(obj,confidence,name)
        
        
    #Retorna em formato json para api
    def getImpact(self, heavyMetais=None):
        return {
            "name": self.name,
            "confidence": self.confidence,
            "impact": self.impact(heavyMetais)
        }
    
    
    def mensagem(self):
        return "Foi detectado a presença de lixo no lago, isso pode impactar na presença de zinco,chumbo e cadmio"
    
    def heavyMetais(self,metal,value):
        
        if not metal in self._AllMetals:
            return None
        match (metal):
            case "Zn, µg/L":
                return Zinco(value)
            case "Pb, µg/L":
                return Chumbo(value)
            case "Cd, µg/L":
                return Cadmio(value)