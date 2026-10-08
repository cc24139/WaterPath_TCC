using System.Text.Json;

namespace back_end.src.IA.Application;

public static class ContratoRisco
{
    // Confere a resposta pela regra existente; a classificação continua na IA.
    public static bool Valido(string? entrada, string resultado, string? historicoEnviado = null)
    {
        if (entrada is null) return false;
        try
        {
            var sample = ContratoAmostra.Ler(entrada);
            using var document = JsonDocument.Parse(resultado);
            var root = document.RootElement;
            if (root.GetProperty("riskRuleVersion").GetString() != PredicaoIAService.VersaoRegraRisco) return false;
            if (!ContratoMetais.RespostaValida(sample, root)) return false;
            var inputs = root.GetProperty("riskInputs");
            var ph = sample.GetProperty("ph").GetDouble();
            var oxygen = sample.GetProperty("oxigenio_dissolvido").GetDouble();
            if (inputs.GetProperty("ph").GetDouble() != ph
                || inputs.GetProperty("oxigenio_dissolvido").GetDouble() != oxygen) return false;
            var classes = inputs.GetProperty("visualClasses").EnumerateArray().Select(p => p.GetString()!).ToArray();
            var detections = root.GetProperty("detections").EnumerateArray().ToArray();
            var signals = new List<string>();
            var count = 0;
            foreach (var detection in detections)
            {
                var name = detection.GetProperty("className").GetString();
                var number = detection.GetProperty("count").GetInt32();
                if (string.IsNullOrWhiteSpace(name) || number <= 0
                    || detection.GetProperty("classId").GetInt32() < 0) return false;
                count = checked(count + number);
                if (detection.GetProperty("indicatesRisk").GetBoolean()) signals.Add(name);
            }
            if (root.GetProperty("totalObjects").GetInt32() != count
                || classes.Any(string.IsNullOrWhiteSpace)
                || !classes.SequenceEqual(signals.Distinct().Order(StringComparer.Ordinal))) return false;
            var history = root.GetProperty("history");
            var records = history.GetProperty("samples").EnumerateArray().ToArray();
            if (records.Length > 5) return false;
            var predictions = new HashSet<int>();
            var collections = new HashSet<int>();
            var alerts = 0;
            foreach (var record in records)
            {
                var prediction = record.GetProperty("predictionId").GetInt32();
                var collection = record.GetProperty("coletaId").GetInt32();
                var basis = record.GetProperty("baseRiskLevel").GetInt32();
                if (prediction <= 0 || collection <= 0 || basis is < 1 or > 3
                    || !predictions.Add(prediction) || !collections.Add(collection)) return false;
                if (basis >= 2) alerts++;
            }
            if (historicoEnviado is not null)
            {
                using var sent = JsonDocument.Parse(historicoEnviado);
                var expected = sent.RootElement.EnumerateArray().ToArray();
                if (expected.Length != records.Length) return false;
                for (var i = 0; i < records.Length; i++)
                    foreach (var key in new[] { "predictionId", "coletaId", "baseRiskLevel" })
                        if (expected[i].GetProperty(key).GetInt32() != records[i].GetProperty(key).GetInt32()) return false;
            }
            var baseLevel = 1 + (classes.Length > 0 ? 1 : 0) + (ph < 6 || ph > 9 || oxygen < 5 ? 1 : 0);
            var level = Math.Min(3, baseLevel + (alerts >= 3 ? 1 : 0));
            return root.GetProperty("baseRiskLevel").GetInt32() == baseLevel
                && root.GetProperty("riskLevel").GetInt32() == level
                && root.GetProperty("riskLabel").GetString() == (level == 1 ? "baixo" : level == 2 ? "moderado" : "alto")
                && root.GetProperty("riskReasons").EnumerateArray().All(p => p.ValueKind == JsonValueKind.String)
                && history.GetProperty("evaluatedCollections").GetInt32() == records.Length
                && history.GetProperty("alertCollections").GetInt32() == alerts
                && history.GetProperty("adjustment").GetInt32() == level - baseLevel;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException
            or FormatException or OverflowException) { return false; }
    }
}
