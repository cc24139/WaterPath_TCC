using System.Text.Json;
using back_end.src.IA.Domain;

namespace back_end.src.IA.Application;

public static class PredicaoIAPublicContract
{
    // Somente dados de domínio aprovados são devolvidos. Versões de modelo,
    // entradas internas, contexto e evidências usadas pela regra ficam no servidor.
    private static readonly HashSet<string> ResultadoPermitido =
    [
        "detections", "totalObjects", "metalPredictions", "measuredMetals", "estimatedMetals", "baseRiskLevel",
        "riskLevel", "riskLabel", "riskReasons", "metalVariation",
    ];

    public static object Criar(PredicaoIAEntity prediction)
        => Criar(prediction.Id, prediction.ColetaId, prediction.CorpoHidricoId, prediction.DataColeta,
            prediction.Tipo, prediction.CriadaEm, prediction.ResultadoJson);

    public static object Criar(int id, int coletaId, int? corpoHidricoId, DateTime? dataColeta,
        string tipo, DateTime criadaEm, string? resultadoJson)
    {
        if (string.IsNullOrWhiteSpace(resultadoJson))
            throw new IaException(502, IaErrorCodes.InvalidResponse,
                "O serviço de IA retornou uma predição inválida.");

        Dictionary<string, JsonElement> result;
        try
        {
            using var document = JsonDocument.Parse(resultadoJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException();
            result = document.RootElement.EnumerateObject()
                .Where(property => ResultadoPermitido.Contains(property.Name))
                .ToDictionary(property => property.Name, property => property.Value.Clone());
            if (!result.TryGetValue("detections", out var detections) || detections.ValueKind != JsonValueKind.Array
                || !result.TryGetValue("metalPredictions", out var metals) || metals.ValueKind != JsonValueKind.Array
                || !result.TryGetValue("totalObjects", out var total) || !total.TryGetInt32(out var count) || count < 0
                || (tipo == "integrada" && (!result.TryGetValue("riskLevel", out var risk)
                    || !risk.TryGetInt32(out var level) || level is < 1 or > 3
                    || !result.TryGetValue("riskLabel", out var label) || label.ValueKind != JsonValueKind.String
                    || !result.TryGetValue("riskReasons", out var reasons) || reasons.ValueKind != JsonValueKind.Array)))
                throw new JsonException();
        }
        catch (Exception error) when (error is JsonException or ArgumentException)
        {
            throw new IaException(502, IaErrorCodes.InvalidResponse,
                "O serviço de IA retornou uma predição inválida.");
        }

        return new
        {
            Id = id,
            ColetaId = coletaId,
            CorpoHidricoId = corpoHidricoId,
            DataColeta = dataColeta,
            Tipo = tipo,
            CriadaEm = criadaEm,
            Resultado = result,
            ImagemOriginalUrl = $"/api/ia/predicoes/{id}/imagem/original",
            ImagemResultadoUrl = $"/api/ia/predicoes/{id}/imagem",
        };
    }
}
