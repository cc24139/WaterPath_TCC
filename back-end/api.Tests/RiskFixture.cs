using System.Text.Json;
using back_end.src.IA.Application;

namespace WaterPath.Api.Tests;

internal static class RiskFixture
{
    internal const string Sample = "{\"temperatura\":22,\"ph\":7,\"condutividade_eletrica\":100,\"oxigenio_dissolvido\":6}";

    internal static string Result(string sample = Sample, string history = "[]", string[]? classes = null,
        string[]? reasons = null)
    {
        classes ??= ["Lixo"];
        using var input = JsonDocument.Parse(sample);
        using var previous = JsonDocument.Parse(history);
        var ph = input.RootElement.GetProperty("ph").GetDouble();
        var oxygen = input.RootElement.GetProperty("oxigenio_dissolvido").GetDouble();
        var records = previous.RootElement.EnumerateArray().Select(x => x.Clone()).ToArray();
        var alerts = records.Count(x => x.GetProperty("baseRiskLevel").GetInt32() >= 2);
        var baseLevel = 1 + (classes.Length > 0 ? 1 : 0) + (ph < 6 || ph > 9 || oxygen < 5 ? 1 : 0);
        var level = Math.Min(3, baseLevel + (alerts >= 3 ? 1 : 0));
        return JsonSerializer.Serialize(new
        {
            detections = classes.Select((name, index) => new { classId = index, className = name, count = 1, indicatesRisk = true }),
            totalObjects = classes.Length, metalPredictions = Array.Empty<object>(), visionModelVersion = "modelo-de-teste",
            riskLevel = level, baseRiskLevel = baseLevel, riskLabel = level == 1 ? "baixo" : level == 2 ? "moderado" : "alto",
            riskRuleVersion = PredicaoIAService.VersaoRegraRisco, riskReasons = reasons ?? ["Sinal visual: Lixo."],
            riskInputs = new { ph, oxigenio_dissolvido = oxygen, visualClasses = classes.Order(StringComparer.Ordinal) },
            history = new { evaluatedCollections = records.Length, alertCollections = alerts, adjustment = level - baseLevel, samples = records },
            annotatedImageContentType = "image/jpeg", annotatedImage = Convert.ToBase64String([0xff, 0xd8, 0xff, 0xd9]),
        });
    }
}
