using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace back_end.src.IA.Application;

public class IaException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class IaClient(HttpClient client)
{
    private async Task<HttpResponseMessage> EnviarAsync(string route, MultipartFormDataContent form,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.PostAsync(route, form, cancellationToken);
            if (response.IsSuccessStatusCode) return response;
            using (response)
            {
                // Erros de entrada da IA são preservados; falhas do serviço viram 502.
                var status = (int)response.StatusCode;
                throw new IaException(status is 400 or 413 or 422 ? status : 502,
                    status is 400 or 413 or 422
                        ? "A IA rejeitou a imagem ou os dados da amostra."
                        : "O serviço de IA não conseguiu realizar a análise.");
            }
        }
        catch (HttpRequestException)
        {
            throw new IaException(502, "Não foi possível acessar o serviço de IA.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IaException(504, "O serviço de IA excedeu o tempo de resposta.");
        }
    }

    private static ByteArrayContent Arquivo(byte[] image, string contentType)
    {
        var content = new ByteArrayContent(image);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return content;
    }

    public async Task<(string ResultadoJson, byte[] Imagem)> AnalisarAsync(string? data, byte[] image, string contentType,
        string name, CancellationToken cancellationToken, string history = "[]", string? collectionContext = null)
    {
        using var form = new MultipartFormDataContent();
        if (data is not null)
        {
            form.Add(new StringContent(data, Encoding.UTF8), "data");
            form.Add(new StringContent(history, Encoding.UTF8), "history");
        }
        form.Add(Arquivo(image, contentType), "image", name);
        if (collectionContext is not null)
            form.Add(new StringContent(collectionContext, Encoding.UTF8), "collectionContext");
        using var response = await EnviarAsync("analyze", form, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            ValidarCamposUnicos(root);
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("detections", out var detections) || detections.ValueKind != JsonValueKind.Array
                || !root.TryGetProperty("metalPredictions", out var metals) || metals.ValueKind != JsonValueKind.Array
                || !root.TryGetProperty("totalObjects", out var total) || total.ValueKind != JsonValueKind.Number
                || !total.TryGetInt32(out var count) || count < 0
                || !root.TryGetProperty("visionModelVersion", out var version) || version.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(version.GetString())
                || !root.TryGetProperty("annotatedImageContentType", out var type) || type.GetString() != "image/jpeg"
                || !root.TryGetProperty("annotatedImage", out var annotated) || annotated.ValueKind != JsonValueKind.String)
                throw new JsonException();
            if (data is not null && !ContratoRisco.Valido(data, json, history)) throw new JsonException();
            if (collectionContext is not null)
            {
                if (!root.TryGetProperty("metalModelVersion", out var metalVersion)
                    || metalVersion.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(metalVersion.GetString())
                    || metals.GetArrayLength() != 8) throw new JsonException();
                var names = new HashSet<string>();
                foreach (var metal in metals.EnumerateArray())
                {
                    var symbol = metal.GetProperty("name").GetString();
                    var value = metal.GetProperty("value").GetDouble();
                    if (symbol is not ("Fe" or "Mn" or "Cr" or "Ni" or "Cu" or "Zn" or "Cd" or "Pb")
                        || !names.Add(symbol) || !double.IsFinite(value) || value < 0
                        || metal.GetProperty("unit").GetString() != (symbol is "Fe" or "Mn" ? "mg/L" : "µg/L"))
                        throw new JsonException();
                }
                using var sent = JsonDocument.Parse(collectionContext);
                if (!root.TryGetProperty("collectionContext", out var received)
                    || !System.Text.Json.Nodes.JsonNode.DeepEquals(
                        System.Text.Json.Nodes.JsonNode.Parse(sent.RootElement.GetRawText()),
                        System.Text.Json.Nodes.JsonNode.Parse(received.GetRawText()))) throw new JsonException();
            }
            var bytes = Convert.FromBase64String(annotated.GetString()!);
            if (DetectarContentType(bytes) != "image/jpeg") throw new JsonException();
            // A imagem fica no campo binário, sem duplicação no JSON salvo.
            var result = root.EnumerateObject()
                .Where(p => p.Name is not "annotatedImage" and not "annotatedImageContentType")
                .ToDictionary(p => p.Name, p => p.Value.Clone());
            return (JsonSerializer.Serialize(result), bytes);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException
            or KeyNotFoundException or ArgumentException)
        {
            throw new IaException(502, "O serviço de IA retornou uma predição inválida.");
        }
    }

    private static void ValidarCamposUnicos(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>();
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new JsonException();
                ValidarCamposUnicos(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) ValidarCamposUnicos(item);
    }

    public static string? DetectarContentType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff)
            return "image/jpeg";
        if (bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return "image/png";
        return null;
    }
}
