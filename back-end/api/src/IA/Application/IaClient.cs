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

    public async Task<string> PredizerAsync(string data, byte[] image, string contentType,
        string name, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(data, Encoding.UTF8), "data");
        form.Add(Arquivo(image, contentType), "image", name);
        using var response = await EnviarAsync("predict", form, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("detections", out var detections) || detections.ValueKind != JsonValueKind.Array
                || !root.TryGetProperty("metalPredictions", out var metals) || metals.ValueKind != JsonValueKind.Array
                || !root.TryGetProperty("totalObjects", out var total) || total.ValueKind != JsonValueKind.Number
                || !total.TryGetInt32(out var count) || count < 0)
                throw new JsonException();
            return root.GetRawText();
        }
        catch (JsonException)
        {
            throw new IaException(502, "O serviço de IA retornou uma predição inválida.");
        }
    }

    public async Task<(byte[] Bytes, string ContentType)> PredizerImagemAsync(byte[] image,
        string contentType, string name, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        form.Add(Arquivo(image, contentType), "file", name);
        using var response = await EnviarAsync("vision/predict", form, cancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var type = response.Content.Headers.ContentType?.MediaType;
        if (bytes.Length == 0 || type != "image/jpeg" || DetectarContentType(bytes) != "image/jpeg")
            throw new IaException(502, "O serviço de IA retornou uma imagem inválida.");
        return (bytes, type);
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
