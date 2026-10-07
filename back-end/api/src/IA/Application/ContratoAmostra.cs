using System.Text.Json;
using System.Text.Json.Nodes;

namespace back_end.src.IA.Application;

// Nomes e unidades iguais a DTOs/Amostra.py e services/Predict/schema.py.
public static class ContratoAmostra
{
    private static readonly string[] Obrigatorios =
        ["temperatura", "ph", "condutividade_eletrica", "oxigenio_dissolvido"];
    private static readonly string[] Numericos =
        [.. Obrigatorios, "solidos_suspensos_totais", "carbono_organico_total",
            "fosforo_total", "profundidade"];
    private static readonly string[] Metadados = ["estacao", "latitude", "longitude", "data", "estacao_ano"];

    public static JsonElement Ler(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new JsonException();
        var nomes = new HashSet<string>();
        foreach (var field in root.EnumerateObject())
        {
            if (!nomes.Add(field.Name)) throw new JsonException();
            if (Numericos.Contains(field.Name))
            {
                if (field.Value.ValueKind == JsonValueKind.Null && !Obrigatorios.Contains(field.Name)) continue;
                if (field.Value.ValueKind != JsonValueKind.Number || !field.Value.TryGetDouble(out var value)
                    || !double.IsFinite(value) || (field.Name != "temperatura" && value < 0)
                    || (field.Name == "ph" && value > 14)) throw new JsonException();
            }
            else if (!Metadados.Contains(field.Name)
                || field.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw new JsonException();
        }
        if (Obrigatorios.Any(name => !nomes.Contains(name))) throw new JsonException();
        return root.Clone();
    }

    public static string ReferenciarColeta(string json, DateTimeOffset dataColeta)
    {
        var sample = Ler(json);
        if (sample.TryGetProperty("data", out var date) && date.ValueKind != JsonValueKind.Null
            && (!DateTimeOffset.TryParse(date.GetString(), System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var supplied) || supplied != dataColeta))
            throw new IaException(422, "A data da amostra deve corresponder ao instante da coleta.");
        var normalized = JsonNode.Parse(sample.GetRawText())!.AsObject();
        normalized["data"] = dataColeta.ToUniversalTime().ToString("O");
        return normalized.ToJsonString();
    }
}
