using System.Globalization;
using System.Text.Json;

namespace back_end.src.IA.Application;

// Validação da integração; a análise temporal permanece no serviço de IA.
public static class ContratoMetais
{
    private static readonly string[] Nomes = ["Fe", "Mn", "Cr", "Ni", "Cu", "Zn", "Cd", "Pb"];
    private static readonly string[] Motivos = ["dados_ausentes", "referencia_temporal_ausente_ou_invalida",
        "referencia_nao_anterior", "unidades_ausentes", "unidades_incompativeis", "variacao_nao_representavel"];

    private static void Campos(JsonElement value, params string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object) throw new JsonException();
        var seen = new HashSet<string>();
        foreach (var field in value.EnumerateObject())
            if (!allowed.Contains(field.Name) || !seen.Add(field.Name)) throw new JsonException();
    }

    private static string? Texto(JsonElement value, string name) =>
        value.TryGetProperty(name, out var field) && field.ValueKind != JsonValueKind.Null ? field.GetString() : null;

    private static double? Numero(JsonElement value, string name) =>
        value.TryGetProperty(name, out var field) && field.ValueKind != JsonValueKind.Null ? field.GetDouble() : null;

    public static void ValidarLista(JsonElement value, bool aceitaNull = true)
    {
        if (aceitaNull && value.ValueKind == JsonValueKind.Null) return;
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > 8) throw new JsonException();
        var names = new HashSet<string>();
        foreach (var item in value.EnumerateArray())
        {
            Campos(item, "name", "value", "unit");
            if (!item.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String
                || !Nomes.Contains(name.GetString()) || !names.Add(name.GetString()!)) throw new JsonException();
            if (item.TryGetProperty("value", out var number) && number.ValueKind != JsonValueKind.Null
                && (number.ValueKind != JsonValueKind.Number || !number.TryGetDouble(out var v)
                    || !double.IsFinite(v) || v < 0)) throw new JsonException();
            if (item.TryGetProperty("unit", out var unit) && unit.ValueKind != JsonValueKind.Null
                && (unit.ValueKind != JsonValueKind.String || unit.GetString()!.Length == 0)) throw new JsonException();
        }
    }

    public static void ValidarReferencia(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return;
        Campos(value, "data", "metais_pesados");
        if (value.TryGetProperty("data", out var date)
            && date.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw new JsonException();
        if (value.TryGetProperty("metais_pesados", out var metals)) ValidarLista(metals, aceitaNull: false);
    }

    private static Dictionary<string, JsonElement> Medicoes(JsonElement value) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty("metais_pesados", out var metals)
            && metals.ValueKind == JsonValueKind.Array
            ? metals.EnumerateArray().ToDictionary(m => m.GetProperty("name").GetString()!, m => m)
            : [];

    private static bool UnidadeCompativel(string? unit) => unit?.Trim().ToLowerInvariant()
        .Replace('µ', 'u').Replace('μ', 'u') is "mg/l" or "ug/l";

    private static DateTimeOffset? Instante(string? value) => value is not null && value.Contains('T')
        && (value.EndsWith('Z') || value.LastIndexOf('+') > value.IndexOf('T')
            || value.LastIndexOf('-') > value.IndexOf('T'))
        && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    public static bool RespostaValida(JsonElement sample, JsonElement result, string? collectionContext = null)
    {
        var reference = sample.TryGetProperty("referencia_metais_pesados", out var supplied) ? supplied : default;
        var hasExplicitReference = reference.ValueKind == JsonValueKind.Object;
        var current = hasExplicitReference ? Medicoes(sample) : Predicoes(result);
        var previous = hasExplicitReference ? Medicoes(reference) : new Dictionary<string, JsonElement>();
        // Respostas persistidas antigas continuam válidas quando a entrada não usa a extensão.
        if (!result.TryGetProperty("metalVariation", out var variation))
            return hasExplicitReference ? false : Medicoes(sample).Count == 0;
        var currentDate = Texto(sample, "data");
        var previousDate = hasExplicitReference ? Texto(reference, "data") : Texto(variation, "referenceDate");
        if (Texto(variation, "currentDate") != currentDate
            || (hasExplicitReference && Texto(variation, "referenceDate") != previousDate)
            || string.IsNullOrWhiteSpace(Texto(variation, "message"))) return false;

        if (!hasExplicitReference && previousDate is not null
            && !HistoricalDateExists(collectionContext, previousDate, currentDate)) return false;
        if (!hasExplicitReference && previousDate is null && collectionContext is not null
            && HasPredictableHistory(collectionContext, currentDate)) return false;

        var items = variation.GetProperty("metals").EnumerateArray().ToArray();
        var actualPrevious = new Dictionary<string, (double? Value, string? Unit)>();
        foreach (var item in items)
        {
            var name = Texto(item, "name");
            if (name is null || !Nomes.Contains(name) || actualPrevious.ContainsKey(name)) return false;
            actualPrevious[name] = (Numero(item, "previousValue"), Texto(item, "previousUnit"));
        }
        var names = current.Keys.Union(previous.Keys).Union(actualPrevious
            .Where(p => p.Value.Value is not null).Select(p => p.Key)).Order(StringComparer.Ordinal).ToArray();
        if (items.Length != names.Length) return false;
        var comparable = 0;
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (Texto(item, "name") != names[i]) return false;
            var now = current.GetValueOrDefault(names[i]);
            var before = previous.GetValueOrDefault(names[i]);
            double? nowValue = current.ContainsKey(names[i]) ? Numero(now, "value") : null;
            double? previousValue = hasExplicitReference && before.ValueKind == JsonValueKind.Object
                ? Numero(before, "value") : actualPrevious[names[i]].Value;
            var nowUnit = current.ContainsKey(names[i]) ? Texto(now, "unit") : null;
            var previousUnit = hasExplicitReference && before.ValueKind == JsonValueKind.Object
                ? Texto(before, "unit") : actualPrevious[names[i]].Unit;
            if (Numero(item, "currentValue") != nowValue || Numero(item, "previousValue") != previousValue
                || Texto(item, "currentUnit") != nowUnit || Texto(item, "previousUnit") != previousUnit
                || Texto(item, "unit") != nowUnit) return false;
            var state = item.GetProperty("variation").GetString();
            var delta = Numero(item, "delta");
            var reason = Texto(item, "reason");
            if (state == "indeterminada")
            {
                if (delta is not null || !Motivos.Contains(reason)) return false;
            }
            else
            {
                var date = Instante(currentDate);
                var earlier = Instante(previousDate);
                if (nowValue is null || previousValue is null || !UnidadeCompativel(nowUnit)
                    || !UnidadeCompativel(previousUnit) || date is null || earlier is null || earlier >= date
                    || reason is not null || delta is null || !double.IsFinite(delta.Value)) return false;
                var expectedDelta = (nowValue.Value * Fator(nowUnit!) - previousValue.Value * Fator(previousUnit!))
                    / Fator(nowUnit!);
                if (!double.IsFinite(expectedDelta) || Math.Abs(delta.Value - expectedDelta) >
                    Math.Max(1e-12, Math.Abs(expectedDelta) * 1e-9)) return false;
                if (!(state == "aumento" && delta > 0 || state == "reducao" && delta < 0
                    || state == "estabilidade" && delta == 0)) return false;
                comparable++;
            }
        }
        return Texto(variation, "status") == (comparable == 0 ? "indeterminada"
            : comparable == items.Length ? "completa" : "parcial");
    }

    private static Dictionary<string, JsonElement> Predicoes(JsonElement result)
    {
        if (!result.TryGetProperty("metalPredictions", out var metals) || metals.ValueKind != JsonValueKind.Array)
            return [];
        return metals.EnumerateArray().ToDictionary(m => m.GetProperty("name").GetString()!, m => m);
    }

    private static double Fator(string unit) => unit.Trim().ToLowerInvariant()
        .Replace('µ', 'u').Replace('μ', 'u') == "mg/l" ? 1 : 0.001;

    private static bool HistoricalDateExists(string? contextJson, string referenceDate, string? currentDate)
    {
        if (contextJson is null) return false;
        try
        {
            using var document = JsonDocument.Parse(contextJson);
            if (!document.RootElement.TryGetProperty("history", out var history)
                || history.ValueKind != JsonValueKind.Array) return false;
            return history.EnumerateArray().Any(record => Texto(record, "collectedAt") == referenceDate
                && Instante(referenceDate) is { } prior && Instante(currentDate) is { } current && prior < current);
        }
        catch (JsonException) { return false; }
    }

    private static bool HasPredictableHistory(string contextJson, string? currentDate)
    {
        if (currentDate is null) return false;
        try
        {
            using var document = JsonDocument.Parse(contextJson);
            if (!document.RootElement.TryGetProperty("history", out var history)
                || history.ValueKind != JsonValueKind.Array) return false;
            foreach (var record in history.EnumerateArray())
            {
                if (Instante(Texto(record, "collectedAt")) is not { } date
                    || Instante(currentDate) is not { } current || date >= current
                    || !record.TryGetProperty("measurements", out var measurements)
                    || measurements.ValueKind != JsonValueKind.Array) continue;
                var required = new HashSet<string>(StringComparer.Ordinal);
                var invalidFeature = false;
                foreach (var measurement in measurements.EnumerateArray())
                {
                    var code = Texto(measurement, "code");
                    var expected = code switch
                    {
                        "Temperatura" => "°c", "Ph" => "ph", "CondutividadeEletrica" => "us/cm",
                        "OxigenioDissolvido" or "SolidosSuspensosTotais" or "CarbonoOrganicoTotal" => "mg/l",
                        "FosforoTotal" => "ug/l", _ => null,
                    };
                    if (code is null || expected is null || measurement.GetProperty("censored").GetBoolean()) continue;
                    var value = Numero(measurement, "value");
                    if (value is null) continue;
                    var unit = Texto(measurement, "unit")?.Trim().ToLowerInvariant()
                        .Replace('µ', 'u').Replace('μ', 'u');
                    var acceptedUnit = unit == expected || code == "FosforoTotal" && unit == "mg/l";
                    if (!acceptedUnit) continue;
                    var nonNegativeRequired = code is "Ph" or "CondutividadeEletrica" or "OxigenioDissolvido";
                    var optionalFeature = code is "SolidosSuspensosTotais" or "CarbonoOrganicoTotal" or "FosforoTotal";
                    if (!double.IsFinite(value.Value)
                        || (nonNegativeRequired && (value.Value < 0 || code == "Ph" && value.Value > 14))
                        || (optionalFeature && value.Value < 0))
                    {
                        invalidFeature = true;
                        break;
                    }
                    if (code is "Temperatura" or "Ph" or "CondutividadeEletrica" or "OxigenioDissolvido")
                        required.Add(code);
                }
                if (!invalidFeature && required.Count == 4) return true;
            }
            return false;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
        { return false; }
    }
}
