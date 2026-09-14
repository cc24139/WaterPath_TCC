
class Metal:
    def __init__(self, nome, valor, unidadeMedida,limite):
        self.nome = nome
        self.valor = valor
        self.unidadeMedida = unidadeMedida
        self.limite = limite

    #unidade padrão é a unidade que o modelo retorna, mas os valores do limite estão de acordo com o conama
    def definirLimite(self,unidadePadrao = True):
        if unidadePadrao:
            return self.limite
        return self.converterParaMgL(self.limite)
            
        
    
    def converterParaMgL(self,valor):
        if self.unidadeMedida == "mg/L":
            return valor
        return valor/1000
    
    def mensagem(self):
        pass
    
    