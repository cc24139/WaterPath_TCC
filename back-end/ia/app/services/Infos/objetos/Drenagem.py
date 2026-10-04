from .Impact import Impact


class Drenagem(Impact):
    def mensagem(self):
        return (
            "Foi detectada uma conexão de drenagem. A imagem não permite determinar "
            "a composição do lançamento; investigue sua origem e relacione-o à coleta."
        )

    def heavyMetais(self, metal, value):
        # Não há associação validada entre esta classe visual e metais específicos.
        return None
