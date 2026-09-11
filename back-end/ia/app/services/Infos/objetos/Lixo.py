
from services.Infos.metais import chumbo
class Lixo(Impact):
    def __init__(self,obj,confidence=0.1,name="lixo"):
        super().__init__(obj,confidence,name)
        
    def impact(self,heavyMetais=None):
        detectado = ""
        for metal in self.heavyMetais():
            if heavyMetais and metal in heavyMetais:
                metal = self.heavyMetais(metal)
                if metal.valor > metal.limite():
                    detectado += f"{metal.nome} acima do limite ({metal.valor} {metal.unidadeMedida})\n"
        return f"Foi detectada a presença de lixo no lago. O descarte irregular pode contaminar a água e aumentar a concentração de metais pesados.\n{detectado}"
    
    #Retorna em formato json para api
    def getImpact(self):
        return {
            "name": self.name,
            "confidence": self.confidence,
            "impact": self.impact()
        }
    
    def heavyMetais(self,metal):
        match(metal):
            case "Chumbo":
                return Chumbo(self.obj, "mg/l")
                