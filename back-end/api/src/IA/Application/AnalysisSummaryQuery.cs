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
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] AnalysisRisk? Risco,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] AnalysisMetalSummary[]? Metais);

public record AnalysisMetalSummary(string Name, double? Value, string Unit, double? Delta,
    string? Variation, double Limit, string LimitUnit, string? Source);

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
    private static readonly (string Name, double Limit, string Unit)[] MetalLimits =
    [
        ("Fe", 0.3, "mg/L"), ("Mn", 0.1, "mg/L"), ("Cr", 50, "µg/L"), ("Ni", 25, "µg/L"),
        ("Cu", 9, "µg/L"), ("Zn", 180, "µg/L"), ("Cd", 1, "µg/L"), ("Pb", 10, "µg/L"),
    ];

    private sealed record MetalValue(double Value, string Unit, string Source);
    private sealed record MetalChange(double? CurrentValue, string? CurrentUnit, double? Delta, string? Variation);

    private static AnalysisMetalSummary[]? ReadMetals(string? json)
    {
        try
        {
            using var document = JsonDocument.Parse(json ?? "{}");
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            var values = new Dictionary<string, MetalValue>(StringComparer.Ordinal);
            void ReadValues(string fieldName, string source)
            {
                if (!root.TryGetProperty(fieldName, out var list) || list.ValueKind != JsonValueKind.Array) return;
                foreach (var item in list.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object
                        || !item.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String
                        || !item.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Number
                        || !value.TryGetDouble(out var number) || !double.IsFinite(number)
                        || !item.TryGetProperty("unit", out var unit) || unit.ValueKind != JsonValueKind.String
                        || string.IsNullOrWhiteSpace(unit.GetString())) continue;
                    values.TryAdd(name.GetString()!, new(number, unit.GetString()!, source));
                }
            }

            ReadValues("measuredMetals", "medido");
            ReadValues("estimatedMetals", "estimado");
            ReadValues("metalPredictions", "estimado");

            var changes = new Dictionary<string, MetalChange>(StringComparer.Ordinal);
            if (root.TryGetProperty("metalVariation", out var variation)
                && variation.ValueKind == JsonValueKind.Object
                && variation.TryGetProperty("metals", out var rows) && rows.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in rows.EnumerateArray())
                {
                    if (row.ValueKind != JsonValueKind.Object
                        || !row.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String) continue;
                    double? ReadNumber(string field) => row.TryGetProperty(field, out var value)
                        && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)
                        && double.IsFinite(number) ? number : null;
                    var currentUnit = row.TryGetProperty("currentUnit", out var unit) && unit.ValueKind == JsonValueKind.String
                        ? unit.GetString() : null;
                    var state = row.TryGetProperty("variation", out var stateValue) && stateValue.ValueKind == JsonValueKind.String
                        ? stateValue.GetString() : null;
                    changes.TryAdd(name.GetString()!, new(ReadNumber("currentValue"), currentUnit,
                        ReadNumber("delta"), state));
                }
            }

            var hasMetalData = values.Count > 0 || changes.Count > 0;
            if (!hasMetalData) return null;
            return MetalLimits.Select(limit =>
            {
                values.TryGetValue(limit.Name, out var value);
                changes.TryGetValue(limit.Name, out var change);
                var currentValue = value?.Value ?? change?.CurrentValue;
                var currentUnit = value?.Unit ?? change?.CurrentUnit ?? limit.Unit;
                return new AnalysisMetalSummary(limit.Name, currentValue, currentUnit, change?.Delta,
                    change?.Variation, limit.Limit, limit.Unit, value?.Source);
            }).ToArray();
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        { return null; }
    }

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
                row.DataHora, row.ResponsavelNome, row.PredicaoId, prediction?.CriadaEm, risk,
                prediction is not null && prediction.CorpoHidricoId == row.CorpoHidricoId
                    && prediction.DataColeta == row.DataHora.UtcDateTime ? ReadMetals(prediction.ResultadoJson) : null);
        }).ToArray();
        return new { total, pagina = page, tamanhoPagina = pageSize, itens = items };
    }
}
