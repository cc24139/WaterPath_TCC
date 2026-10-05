"""Concentração prevista e referência do metal, na unidade do modelo."""


class Metal:
    def __init__(self, nome, valor, unidadeMedida, limite):
        self.nome = nome
        self.valor = float(valor)
        self.unidadeMedida = unidadeMedida
        self.limite = limite

    def definirLimite(self, unidadePadrao=True):
        if unidadePadrao:
            return self.limite
        return self.converterParaMgL(self.limite)

    def converterParaMgL(self, valor):
        if self.unidadeMedida == "mg/L":
            return valor
        return valor / 1000

    def acima_do_limite(self):
        return self.valor > self.definirLimite()

    def mensagem(self):
        return ""
