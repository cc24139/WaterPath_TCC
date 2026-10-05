from .Impact import Impact


class Drenagem(Impact):
    sinal_risco = True
    # O histórico não contém uma associação validada a metais específicos.

    def mensagem(self):
        return (
            "Foi detectada uma conexão de drenagem. A imagem não permite determinar "
            "a composição do lançamento; investigue sua origem e relacione-o à coleta."
        )
