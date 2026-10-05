from .Impact import Impact
from services.Infos.metais.chumbo import Chumbo
from services.Infos.metais.Zinco import Zinco
from services.Infos.metais.cadmio import Cadmio
from services.Infos.metais.Cromo import Cromo
from services.Infos.metais.Niquel import Niquel
from services.Infos.metais.Cobre import Cobre


class Urbano(Impact):
    sinal_risco = True
    metais_relacionados = (Cromo, Niquel, Cobre, Zinco, Cadmio, Chumbo)

    def __init__(self, obj, confidence=0.1, name="Urbano", class_id=None):
        super().__init__(obj, confidence, name, class_id)

    def mensagem(self):
        return (
            "Foi detectado a presença de ambiente urbano no lago, caso não ocorra o devido tratamento pode impactar na presença de"
            "Cromo,Niquel,Cobre,Zinco,Cadmio e Chumbo!"
        )
