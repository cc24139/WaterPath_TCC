
class Impact():
    def __init__(self,obj,confidence,name):
        self.obj = obj
        self.confidence = confidence
        self.name = name
        self._AllMetals = [
                "Fe, mg/L",
                "Mn, mg/L",
    "Cr, µg/L",
    "Ni, µg/L",
    "Cu, µg/L",
    "Zn, µg/L",
    "Cd, µg/L",
    "Pb, µg/L"
        ]
        
    def impact(self,heavyMetais=None):
        detectado = ""
        if not heavyMetais:
            return "Foi detectada a presença de lixo no lago. O descarte irregular pode contaminar a água e aumentar a concentração de metais pesados."
        for metal,value in heavyMetais.items():
            metal = self.heavyMetais(metal,float(value))
            if metal == None:
                break
            print(metal.valor,metal.definirLimite())
            if metal.valor > metal.definirLimite():
                detectado += f"{metal.nome} acima do limite ({metal.valor} {metal.unidadeMedida}) \n"
        return f"{self.mensagem()} \n {detectado}"
            
    def mensagem(self):
        pass
    #Retorna em formato json para api
    def getImpact(self):
        return {
            "obj": self.obj,
            "confidence": self.confidence,
            "name": self.name
        }
    
    def heavyMetais(self, metal,value):
        pass