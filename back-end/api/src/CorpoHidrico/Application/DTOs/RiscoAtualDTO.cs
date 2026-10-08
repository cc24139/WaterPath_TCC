using System.Text.Json;

namespace Application.DTOs;

public record RiscoAtualDTO(
    int CorpoHidricoId, int NivelRisco, string? RotuloRisco,
    IReadOnlyList<string> Motivos, int ColetaId, DateTimeOffset DataColeta,
    int PredicaoId, DateTime DataPredicao, string VersaoRegraRisco, JsonElement? VariacaoMetais = null);
