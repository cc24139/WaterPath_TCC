namespace back_end.src.Domain.CorpoHidrico;

// Projeção de leitura: evita carregar as imagens binárias das predições.
public record ResultadoRiscoPersistido(
    int PredicaoId, int ColetaId, DateTimeOffset DataColeta,
    DateTime DataPredicao, string ResultadoJson);
