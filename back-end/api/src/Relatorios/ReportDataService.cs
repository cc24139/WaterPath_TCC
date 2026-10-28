using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Authorization;
using back_end.src.IA.Application;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Relatorios;

public static class ReportParameters
{
    public static readonly (int Code, string Field, string Label)[] All =
    [ (0, "temperatura", "Temperatura"), (1, "ph", "pH"), (4, "condutividade_eletrica", "Condutividade elétrica"),
      (2, "oxigenio_dissolvido", "Oxigênio dissolvido"), (5, "solidos_suspensos_totais", "Sólidos suspensos totais"),
      (7, "carbono_organico_total", "Carbono orgânico total"), (12, "fosforo_total", "Fósforo total") ];
}

public sealed class ReportDataService(WaterPathDbContext db, ResourceAuthorizationService authorization)
{
    public async Task<ReportData> BuildAsync(ReportRequest input, int userId, string issuer, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        if (input.CorpoHidricoId <= 0 || input.Config is null) throw new IaException(400, "Informe corpo hídrico e configuração válidos.");
        input.Config.Validate(ReportTime.Today(now));
        if (!await authorization.PodeAnalisarCorpoHidricoAsync(input.CorpoHidricoId, userId, ct))
            throw new IaException(403, "Você não tem permissão para gerar relatórios deste corpo hídrico.");
        var body = await db.CorposHidricos.AsNoTracking().Where(b => b.Id == input.CorpoHidricoId)
            .Select(b => new { b.Nome, b.Localizacao, Member = b.users.Any(u => u.Id == userId) }).SingleAsync(ct);
        var start = ReportTime.Start(input.Config.StartDate);
        var end = ReportTime.Start(input.Config.EndDate.AddDays(1));
        var collections = await db.Coletas.AsNoTracking().Where(c => c.CorpoHidricoId == input.CorpoHidricoId
                && (body.Member || c.ResponsavelId == userId) && c.DataHora >= start && c.DataHora < end && c.DataHora <= now)
            .OrderByDescending(c => c.DataHora).ThenByDescending(c => c.Id).Take(201)
            .Include(c => c.Medicoes).Include(c => c.MetaisPesados).AsSplitQuery().ToListAsync(ct);
        if (collections.Count > 200) throw new IaException(422, "O período contém mais de 200 coletas. Reduza o intervalo para gerar o relatório.");
        if (collections.Count == 0) throw new IaException(422, "Nenhuma coleta acessível foi encontrada no período selecionado.");
        var collectionIds = collections.Select(c => c.Id).ToArray();
        var selectedIds = await db.PredicoesIA.AsNoTracking().Where(p => collectionIds.Contains(p.ColetaId)
                && p.Tipo == "integrada" && p.CriadaEm <= now.UtcDateTime)
            .GroupBy(p => p.ColetaId).Select(g => g.OrderByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id).First().Id).ToArrayAsync(ct);
        var predictions = await db.PredicoesIA.AsNoTracking().Where(p => selectedIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ColetaId, p.CorpoHidricoId, p.DataColeta, p.CriadaEm, p.EntradaJson, p.ResultadoJson })
            .ToDictionaryAsync(p => p.ColetaId, ct);
        var notices = new List<string>();
        if (!body.Member) notices.Add("Escopo restrito às coletas cadastradas pelo usuário emissor, conforme suas permissões.");
        var rows = collections.Select(c =>
        {
            predictions.TryGetValue(c.Id, out var prediction);
            var measurements = ReportParameters.All.Select(parameter =>
            {
                var m = c.Medicoes.FirstOrDefault(m => (int)m.codigoMedicao == parameter.Code);
                return m is null ? null : new ReportMeasurement(parameter.Code, parameter.Label, m.valor, m.unidade, m.censurado, m.limite);
            }).OfType<ReportMeasurement>().ToArray();
            var measured = c.MetaisPesados.Select(m => new ReportMetal(m.Nome, m.Concentracao, m.Unidade, "Medido")).ToList();
            var valid = prediction is not null && prediction.CorpoHidricoId == c.CorpoHidricoId
                && prediction.DataColeta == c.DataHora.UtcDateTime && InputsMatch(prediction.EntradaJson, c);
            var risk = valid ? AnalysisRisk.Read(prediction!.ResultadoJson) : null;
            var detections = Array.Empty<ReportDetection>();
            var variations = Array.Empty<ReportVariation>();
            if (prediction is not null && risk is null) notices.Add($"Coleta #{c.Id}: resultado de risco indisponível ou sem correspondência com os dados atuais.");
            if (valid && risk is not null)
            {
                using var result = Parse(prediction!.ResultadoJson);
                if (result is not null)
                {
                    var root = result.RootElement;
                    foreach (var field in new[] { "estimatedMetals", "metalPredictions" })
                        foreach (var metal in Elements(root, field))
                            if (Text(metal, "name") is { } name && !measured.Any(m => m.Name == name)
                                && Number(metal, "value") is { } value && value >= 0 && Text(metal, "unit") is { } unit)
                                measured.Add(new(name, value, unit, "Estimado"));
                    detections = Elements(root, "detections").Where(d => Text(d, "className") is not null)
                        .Select(d => new ReportDetection(Text(d, "className")!, (int)Math.Clamp(Number(d, "count") ?? 0, 0, 100000))).ToArray();
                    if (root.TryGetProperty("metalVariation", out var variation) && variation.ValueKind == JsonValueKind.Object)
                        variations = Elements(variation, "metals").Where(m => Text(m, "name") is not null)
                            .Select(m => new ReportVariation(Text(m, "name")!, Number(m, "delta"), Text(m, "unit"),
                                Text(m, "variation") ?? "indeterminada", Text(variation, "referenceDate"),
                                Text(variation, "source") == "medicoes_e_modelo" ? "Medições e modelo" : "Medições")).ToArray();
                }
            }
            return new ReportCollection(c.Id, c.DataHora, c.ResponsavelNome, c.Latitude, c.Longitude, c.ProfundidadeMetros,
                measurements, measured.ToArray(), prediction?.Id, prediction?.CriadaEm, risk, detections, variations);
        }).ToArray();
        var images = new List<ReportImage>();
        if (input.Config.IncludedSections.VisualEvidence)
        {
            var count = input.Config.DocumentLength == "curto" ? 1 : input.Config.DocumentLength == "medio" ? 2 : 3;
            var imageIds = rows.Where(c => c.PredictionId.HasValue && c.Risk is not null).Take(count).Select(c => c.PredictionId!.Value).ToArray();
            var payloads = await db.PredicoesIA.AsNoTracking().Where(p => imageIds.Contains(p.Id))
                .Select(p => new { p.ColetaId, p.Id, p.ImagemOriginal, p.ImagemResultado }).ToListAsync(ct);
            foreach (var id in imageIds)
            {
                var p = payloads.FirstOrDefault(p => p.Id == id);
                if (p is not null && p.ImagemOriginal.Length > 0 && p.ImagemResultado.Length > 0)
                    images.Add(new(p.ColetaId, p.Id, p.ImagemOriginal, p.ImagemResultado));
            }
            if (imageIds.Length > images.Count) notices.Add("Uma ou mais imagens selecionadas não estavam disponíveis no momento da geração.");
            if (rows.Count(c => c.PredictionId.HasValue && c.Risk is not null) > imageIds.Length)
                notices.Add($"Evidências visuais limitadas às {images.Count} análises mais recentes com imagens, conforme a extensão escolhida.");
        }
        return new(Guid.NewGuid(), input.CorpoHidricoId, body.Nome, body.Localizacao, issuer, now,
            input.Config, !body.Member, rows, images.ToArray(), notices.ToArray());
    }

    private static bool InputsMatch(string? json, ColetaEntity collection)
    {
        using var input = Parse(json);
        if (input is null) return false;
        JsonDocument? current;
        // Reutiliza a normalização da análise, inclusive conversão de fósforo mg/L para µg/L.
        try { current = Parse(AmostraDaColeta.Montar(collection)); }
        catch (IaException) { return false; }
        using var currentInput = current;
        if (currentInput is null) return false;
        foreach (var field in ReportParameters.All.Select(p => p.Field).Append("profundidade"))
        {
            if (Number(input.RootElement, field) != Number(currentInput.RootElement, field)) return false;
        }
        var observed = Elements(input.RootElement, "metais_pesados").ToArray();
        var metals = collection.MetaisPesados;
        if (observed.Length != metals.Count) return false;
        foreach (var metal in metals)
            if (!observed.Any(m => Text(m, "name") == metal.Nome && Text(m, "unit") == metal.Unidade && Number(m, "value") == metal.Concentracao)) return false;
        return true;
    }
    internal static JsonDocument? Parse(string? json)
    {
        try { var doc = JsonDocument.Parse(json ?? "{}"); if (doc.RootElement.ValueKind == JsonValueKind.Object) return doc; doc.Dispose(); }
        catch (JsonException) { }
        return null;
    }
    private static IEnumerable<JsonElement> Elements(JsonElement root, string field) => root.TryGetProperty(field, out var value)
        && value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.Object) : [];
    private static string? Text(JsonElement root, string field) => root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static double? Number(JsonElement root, string field) => root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) && double.IsFinite(number) ? number : null;
}
