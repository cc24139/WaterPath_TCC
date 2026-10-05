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
        string name, CancellationToken cancellationToken, string history = "[]")
    {
        using var form = new MultipartFormDataContent();
        if (data is not null)
        {
            form.Add(new StringContent(data, Encoding.UTF8), "data");
            form.Add(new StringContent(history, Encoding.UTF8), "history");
        }
        form.Add(Arquivo(image, contentType), "image", name);
        using var response = await EnviarAsync("analyze", form, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
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
            if (data is not null
                && (!root.TryGetProperty("riskLevel", out var risk) || !risk.TryGetInt32(out var level) || level is < 1 or > 3
                    || !root.TryGetProperty("baseRiskLevel", out var basis) || !basis.TryGetInt32(out var baseLevel) || baseLevel is < 1 or > 3
                    || !root.TryGetProperty("riskRuleVersion", out var rule) || rule.GetString() != PredicaoIAService.VersaoRegraRisco
                    || !root.TryGetProperty("riskReasons", out var reasons) || reasons.ValueKind != JsonValueKind.Array
                    || !root.TryGetProperty("history", out var historyResult) || historyResult.ValueKind != JsonValueKind.Object))
                throw new JsonException();
            var bytes = Convert.FromBase64String(annotated.GetString()!);
            if (DetectarContentType(bytes) != "image/jpeg") throw new JsonException();
            // A imagem fica no campo binário, sem duplicação no JSON salvo.
            var result = root.EnumerateObject()
                .Where(p => p.Name is not "annotatedImage" and not "annotatedImageContentType")
                .ToDictionary(p => p.Name, p => p.Value.Clone());
            return (JsonSerializer.Serialize(result), bytes);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
        {
            throw new IaException(502, "O serviço de IA retornou uma predição inválida.");
        }
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
