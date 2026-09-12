from Impact import Impact
class Turbidez(Impact):
    def __init__(self,obj,confidence,name):
        super().__init__(obj,confidence,name)
        
    def impact():
        return "exemplo"
    
    def heavyMetais():
        return "exemplo"
    #Retorna em formato json para api
    def getImpact(self):
        return {
            "name": self.name,
            "confidence": self.confidence,
            "mensagem": self.impact()
        }
    
    
    