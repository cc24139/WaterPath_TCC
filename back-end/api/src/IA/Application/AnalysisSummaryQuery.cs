using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA.Application;

public record AnalysisSummaryItem(int ColetaId, int CorpoHidricoId, string CorpoHidricoNome,
    string Localizacao, DateTimeOffset DataHora,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] string? ResponsavelNome,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] int? PredicaoId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] DateTime? AnalisadaEm,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] AnalysisRisk? Risco);

public record AnalysisRisk(int Level, string[] Reasons)
{
    public static AnalysisRisk? Read(string? json)
    {
        try
        {
            using var document = JsonDocument.Parse(json ?? "{}");
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("riskLevel", out var level) || !level.TryGetInt32(out var number)
                || number is < 1 or > 3) return null;
            var reasons = root.TryGetProperty("riskReasons", out var values) && values.ValueKind == JsonValueKind.Array
                ? values.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!).ToArray()
                : [];
            return new(number, reasons);
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException) { return null; }
    }
}

public static class AnalysisSummaryQuery
{
    public static async Task<object> ReadAsync(WaterPathDbContext context, int userId, int? waterBodyId,
        bool mine, int[] collectionIds, string? search, bool oldest, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Coletas.AsNoTracking().Where(c => c.ResponsavelId == userId
            || c.CorpoHidrico.users.Any(u => u.Id == userId));
        if (mine) query = query.Where(c => c.ResponsavelId == userId);
        if (waterBodyId.HasValue) query = query.Where(c => c.CorpoHidricoId == waterBodyId.Value);
        if (collectionIds.Length > 0) query = query.Where(c => collectionIds.Contains(c.Id));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.CorpoHidrico.Nome.ToLower().Contains(term)
                || c.CorpoHidrico.Localizacao.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var ordered = oldest ? query.OrderBy(c => c.DataHora).ThenBy(c => c.Id)
            : query.OrderByDescending(c => c.DataHora).ThenByDescending(c => c.Id);
        var rows = await ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(c => new
        {
            ColetaId = c.Id, c.CorpoHidricoId, CorpoHidricoNome = c.CorpoHidrico.Nome,
            Localizacao = c.CorpoHidrico.Localizacao, c.DataHora, c.ResponsavelNome,
            PredicaoId = context.PredicoesIA.Where(p => p.ColetaId == c.Id && p.Tipo == "integrada")
                .OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id).Select(p => (int?)p.Id).FirstOrDefault(),
        }).ToListAsync(cancellationToken);
        var ids = rows.Where(r => r.PredicaoId.HasValue).Select(r => r.PredicaoId!.Value).ToArray();
        // Uma consulta em lote; imagens e entrada interna nunca são carregadas para a listagem.
        var predictions = await context.PredicoesIA.AsNoTracking().Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.CriadaEm, p.CorpoHidricoId, p.DataColeta, p.ResultadoJson })
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var items = rows.Select(row =>
        {
            var prediction = row.PredicaoId.HasValue ? predictions.GetValueOrDefault(row.PredicaoId.Value) : null;
            var risk = prediction is not null && prediction.CorpoHidricoId == row.CorpoHidricoId
                && prediction.DataColeta == row.DataHora.UtcDateTime ? AnalysisRisk.Read(prediction.ResultadoJson) : null;
            return new AnalysisSummaryItem(row.ColetaId, row.CorpoHidricoId, row.CorpoHidricoNome, row.Localizacao,
                row.DataHora, row.ResponsavelNome, row.PredicaoId, prediction?.CriadaEm, risk);
        }).ToArray();
        return new { total, pagina = page, tamanhoPagina = pageSize, itens = items };
    }
}
