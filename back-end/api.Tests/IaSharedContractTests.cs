using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using back_end.src.IA.Application;
using Xunit;

namespace WaterPath.Api.Tests;

public class IaSharedContractTests
{
    private static JsonDocument OpenApi() => JsonDocument.Parse(File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory, "Contracts", "ia", "v1", "openapi.json")));

    [Fact]
    public void ValidadorDaAmostraSegueSchemaCompartilhado()
    {
        using var contract = OpenApi();
        var schema = contract.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("Sample");
        var example = schema.GetProperty("example").GetRawText();
        Assert.Equal(7, ContratoAmostra.Ler(example).GetProperty("ph").GetDouble());

        var values = new Dictionary<string, JsonNode?>
        {
            ["estacao"] = "A", ["latitude"] = "-23.0", ["longitude"] = "-46.0",
            ["profundidade"] = 1, ["estacao_ano"] = "seca", ["solidos_suspensos_totais"] = 1,
            ["carbono_organico_total"] = 1, ["fosforo_total"] = 1,
            ["metais_pesados"] = JsonNode.Parse("[{\"name\":\"Pb\",\"value\":1,\"unit\":\"µg/L\"}]"),
            ["referencia_metais_pesados"] = JsonNode.Parse("{\"data\":\"2026-10-06T12:00:00Z\",\"metais_pesados\":[]}"),
        };
        var baseSample = JsonNode.Parse(example)!.AsObject();
        var properties = schema.GetProperty("properties").EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.True(properties.SetEquals(properties.Where(baseSample.ContainsKey).Concat(values.Keys)));
        foreach (var (name, value) in values)
        {
            var candidate = (JsonObject)baseSample.DeepClone();
            candidate[name] = value?.DeepClone();
            ContratoAmostra.Ler(candidate.ToJsonString());
        }

        var unexpected = (JsonObject)baseSample.DeepClone();
        unexpected["campo_fora_do_contrato"] = 1;
        Assert.Throws<JsonException>(() => ContratoAmostra.Ler(unexpected.ToJsonString()));
        foreach (var name in schema.GetProperty("required").EnumerateArray().Select(x => x.GetString()!))
        {
            var candidate = (JsonObject)baseSample.DeepClone();
            candidate.Remove(name);
            Assert.Throws<JsonException>(() => ContratoAmostra.Ler(candidate.ToJsonString()));
        }
        foreach (var property in schema.GetProperty("properties").EnumerateObject())
        {
            if (property.Value.TryGetProperty("minimum", out var minimum))
            {
                var candidate = (JsonObject)baseSample.DeepClone();
                candidate[property.Name] = minimum.GetDouble() - 1;
                Assert.Throws<JsonException>(() => ContratoAmostra.Ler(candidate.ToJsonString()));
            }
            if (property.Value.TryGetProperty("maximum", out var maximum))
            {
                var candidate = (JsonObject)baseSample.DeepClone();
                candidate[property.Name] = maximum.GetDouble() + 1;
                Assert.Throws<JsonException>(() => ContratoAmostra.Ler(candidate.ToJsonString()));
            }
        }
    }

    [Fact]
    public async Task IaClientEnviaEConsomeExemplosDoContratoCompartilhado()
    {
        using var contract = OpenApi();
        var schemas = contract.RootElement.GetProperty("components").GetProperty("schemas");
        var sample = schemas.GetProperty("Sample").GetProperty("example").GetRawText();
        var context = schemas.GetProperty("CollectionContext").GetProperty("example").GetRawText();
        var response = schemas.GetProperty("AnalyzeResponse").GetProperty("example").GetRawText();
        var sentFields = new HashSet<string>();
        var handler = new ContractHandler(async request =>
        {
            var form = Assert.IsType<MultipartFormDataContent>(request.Content);
            foreach (var part in form)
            {
                sentFields.Add(part.Headers.ContentDisposition!.Name!.Trim('"'));
                await part.LoadIntoBufferAsync();
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        });
        var client = new IaClient(new HttpClient(handler) { BaseAddress = new Uri("http://ia.test/") });

        var result = await client.AnalisarAsync(sample, [0xff, 0xd8, 0xff, 0xd9], "image/jpeg", "rio.jpg",
            CancellationToken.None, "[]", context);

        var canonicalFields = contract.RootElement.GetProperty("paths").GetProperty("/analyze").GetProperty("post")
            .GetProperty("requestBody").GetProperty("content").GetProperty("multipart/form-data")
            .GetProperty("schema").GetProperty("properties").EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.True(canonicalFields.SetEquals(sentFields));
        Assert.Equal([0xff, 0xd8, 0xff, 0xd9], result.Imagem);
        using var saved = JsonDocument.Parse(result.ResultadoJson);
        Assert.Equal("waterpath-risk-v1", saved.RootElement.GetProperty("riskRuleVersion").GetString());
        Assert.False(saved.RootElement.TryGetProperty("annotatedImage", out _));
    }

    private sealed class ContractHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => response(request);
    }
}
