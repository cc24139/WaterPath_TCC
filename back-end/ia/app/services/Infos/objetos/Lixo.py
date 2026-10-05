from .Impact import Impact
from services.Infos.metais.chumbo import Chumbo
from services.Infos.metais.Zinco import Zinco
from services.Infos.metais.cadmio import Cadmio


class Lixo(Impact):
    sinal_risco = True
    metais_relacionados = (Zinco, Chumbo, Cadmio)

    def __init__(self, obj, confidence=0.1, name="lixo", class_id=None):
        super().__init__(obj, confidence, name, class_id)

    def mensagem(self):
        return "Foi detectado a presença de lixo no lago, isso pode impactar na presença de zinco,chumbo e cadmio"
