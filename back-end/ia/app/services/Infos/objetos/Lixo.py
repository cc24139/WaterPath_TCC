from .Impact import Impact
from services.Infos.metais.chumbo import Chumbo
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
    
    def heavyMetais(self,metal,value):
        print(metal)
        return Chumbo(float(value),"µg/L")
        match(metal):
            case "Pb, ":
                return Chumbo(self.obj, "Pb, µg/L")
                