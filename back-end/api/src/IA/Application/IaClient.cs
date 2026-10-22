using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;

namespace back_end.src.IA.Application;

public static class IaErrorCodes
{
    public const string InvalidInput = "IA_INVALID_INPUT";
    public const string Unauthorized = "IA_UNAUTHORIZED";
    public const string Forbidden = "IA_FORBIDDEN";
    public const string RateLimited = "IA_RATE_LIMITED";
    public const string Unavailable = "IA_UNAVAILABLE";
    public const string Timeout = "IA_TIMEOUT";
    public const string InvalidResponse = "IA_INVALID_RESPONSE";
    public const string Persistence = "IA_PERSISTENCE_ERROR";
}

public class IaException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;

    public IaException(int statusCode, string message)
        : this(statusCode, IaErrorCodes.InvalidInput, message) { }
}

public class IaClient
{
    private readonly HttpClient client;
    private readonly ILogger<IaClient> logger;

    public IaClient(HttpClient client, ILogger<IaClient>? logger = null)
    {
        this.client = client;
        this.logger = logger ?? NullLogger<IaClient>.Instance;
    }

    private async Task<HttpResponseMessage> EnviarAsync(string route, MultipartFormDataContent form,
        CancellationToken cancellationToken)
    {
        var elapsed = Stopwatch.StartNew();
        try
        {
            var response = await client.PostAsync(route, form, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Serviço de IA respondeu. Operacao={Operation} Status={StatusCode} DuracaoMs={ElapsedMs}",
                    route, (int)response.StatusCode, elapsed.ElapsedMilliseconds);
                return response;
            }
            using (response)
            {
                var status = (int)response.StatusCode;
                logger.LogWarning("Serviço de IA rejeitou a operação. Operacao={Operation} Status={StatusCode} DuracaoMs={ElapsedMs}",
                    route, status, elapsed.ElapsedMilliseconds);
                if (status is 400 or 413 or 422)
                    throw new IaException(status, IaErrorCodes.InvalidInput,
                        "A imagem ou os dados da amostra foram rejeitados.");
                if (status == 429)
                    throw new IaException(503, IaErrorCodes.RateLimited,
                        "O serviço de IA atingiu o limite de solicitações. Tente novamente mais tarde.");
                if (status >= 500)
                    throw new IaException(503, IaErrorCodes.Unavailable,
                        "O serviço de IA está temporariamente indisponível.");
                throw new IaException(502, IaErrorCodes.Unavailable,
                    "O serviço de IA não conseguiu realizar a análise.");
            }
        }
        catch (HttpRequestException error)
        {
            logger.LogWarning(error,
                "Falha de conexão com o serviço de IA. Operacao={Operation} DuracaoMs={ElapsedMs}",
                route, elapsed.ElapsedMilliseconds);
            throw new IaException(503, IaErrorCodes.Unavailable,
                "O serviço de IA está temporariamente indisponível.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timeout no serviço de IA. Operacao={Operation} DuracaoMs={ElapsedMs}",
                route, elapsed.ElapsedMilliseconds);
            throw new IaException(504, IaErrorCodes.Timeout,
                "O serviço de IA excedeu o tempo de resposta.");
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
            logger.LogWarning(ex, "Resposta inválida do serviço de IA. Operacao=analyze");
            throw new IaException(502, IaErrorCodes.InvalidResponse,
                "O serviço de IA retornou uma predição inválida.");
        }
        catch (HttpRequestException error)
        {
            logger.LogWarning(error, "Falha ao ler a resposta do serviço de IA. Operacao=analyze");
            throw new IaException(503, IaErrorCodes.Unavailable,
                "O serviço de IA está temporariamente indisponível.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timeout ao ler a resposta do serviço de IA. Operacao=analyze");
            throw new IaException(504, IaErrorCodes.Timeout,
                "O serviço de IA excedeu o tempo de resposta.");
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
