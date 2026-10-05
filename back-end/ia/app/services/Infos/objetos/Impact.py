"""Evidência visual e interpretação dos metais relacionados à sua classe."""


class Impact:
    sinal_risco = False
    metais_relacionados = ()

    def __init__(self, obj, confidence, name, class_id=None):
        self.obj = [float(value) for value in obj]
        self.confidence = float(confidence)
        self.name = name
        self.class_id = class_id

    def indica_risco(self):
        return self.sinal_risco

    def impact(self, heavyMetais=None):
        if not heavyMetais or not self.indica_risco():
            return self.mensagem()
        alerts = ""
        for metal in heavyMetais:
            if isinstance(metal, self.metais_relacionados) and metal.acima_do_limite():
                alerts += (
                    f"{metal.nome} previsto acima do limite de referência "
                    f"({metal.valor} {metal.unidadeMedida}) \n"
                )
        return f"{self.mensagem()} \n {alerts}"

    def mensagem(self):
        return "Classe visual sem regra de interpretação cadastrada."

    def getImpact(self, heavyMetais=None):
        return {
            "obj": self.obj,
            "confidence": self.confidence,
            "name": self.name,
            "impact": self.impact(heavyMetais),
        }
