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

    public static bool RespostaValida(JsonElement sample, JsonElement result)
    {
        var current = Medicoes(sample);
        var reference = sample.TryGetProperty("referencia_metais_pesados", out var supplied) ? supplied : default;
        var previous = Medicoes(reference);
        // Respostas persistidas antigas continuam válidas quando a entrada não usa a extensão.
        if (!result.TryGetProperty("metalVariation", out var variation))
            return current.Count == 0 && previous.Count == 0
                && (!sample.TryGetProperty("referencia_metais_pesados", out var r) || r.ValueKind == JsonValueKind.Null);
        var currentDate = Texto(sample, "data");
        var previousDate = reference.ValueKind == JsonValueKind.Object ? Texto(reference, "data") : null;
        if (Texto(variation, "currentDate") != currentDate || Texto(variation, "referenceDate") != previousDate
            || string.IsNullOrWhiteSpace(variation.GetProperty("message").GetString())) return false;
        var names = current.Keys.Union(previous.Keys).Order(StringComparer.Ordinal).ToArray();
        var items = variation.GetProperty("metals").EnumerateArray().ToArray();
        if (items.Length != names.Length) return false;
        var comparable = 0;
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item.GetProperty("name").GetString() != names[i]) return false;
            var now = current.GetValueOrDefault(names[i]);
            var before = previous.GetValueOrDefault(names[i]);
            double? nowValue = now.ValueKind == JsonValueKind.Object ? Numero(now, "value") : null;
            double? previousValue = before.ValueKind == JsonValueKind.Object ? Numero(before, "value") : null;
            var nowUnit = now.ValueKind == JsonValueKind.Object ? Texto(now, "unit") : null;
            var previousUnit = before.ValueKind == JsonValueKind.Object ? Texto(before, "unit") : null;
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
                if (!(state == "aumento" && delta > 0 || state == "reducao" && delta < 0
                    || state == "estabilidade" && delta == 0)) return false;
                comparable++;
            }
        }
        return Texto(variation, "status") == (comparable == 0 ? "indeterminada"
            : comparable == items.Length ? "completa" : "parcial");
    }
}
