
class Impact():
    def __init__(self,obj,confidence,name):
        self.obj = obj
        self.confidence = confidence
        self.name = name
        
    def impact(self,heavyMetais=None):
        pass
    
    #Retorna em formato json para api
    def getImpact(self):
        return {
            "obj": self.obj,
            "confidence": self.confidence,
            "name": self.name
        }
    
    def heavyMetais(self, metal):
        pass