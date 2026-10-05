using System.Net;
using System.Text;
using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA;
using back_end.src.IA.Application;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WaterPath.Api.Tests;

public class PredicaoIATests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;
    private int coletaId;
    private static readonly byte[] Jpeg = [0xff, 0xd8, 0xff, 0xd9];
    private const string Sample = "{\"temperatura\":22,\"ph\":7,\"condutividade_eletrica\":100,\"oxigenio_dissolvido\":6}";

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new TestDbContext(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var river = new CorpoHidricoEntity("Rio de teste", "Local de teste", 10, false);
        db.CorposHidricos.Add(river);
        await db.SaveChangesAsync();
        var collection = new ColetaEntity { CorpoHidricoId = river.Id, DataHora = DateTimeOffset.UtcNow };
        db.Coletas.Add(collection);
        await db.SaveChangesAsync();
        coletaId = collection.Id;
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    private static IFormFile Upload(byte[]? bytes = null)
    {
        bytes ??= Jpeg;
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", "rio.jpg");
    }

    private static string ValidResponse() => JsonSerializer.Serialize(new
    {
        detections = new[] { new { classId = 0, className = "Lixo", count = 1 } }, totalObjects = 1,
        metalPredictions = Array.Empty<object>(), visionModelVersion = "modelo-de-teste",
        riskLevel = 2, baseRiskLevel = 2, riskRuleVersion = PredicaoIAService.VersaoRegraRisco,
        riskReasons = new[] { "Sinal visual: Lixo." }, history = new { adjustment = 0 },
        annotatedImageContentType = "image/jpeg", annotatedImage = Convert.ToBase64String(Jpeg),
    });

    private PredicaoIAService Service(Handler handler) => new(db,
        new IaClient(new HttpClient(handler) { BaseAddress = new Uri("http://ia.test/") }));

    [Fact]
    public async Task UmaChamadaSalvaResultadoEImagensDaMesmaColeta()
    {
        var handler = new Handler(async request =>
        {
            Assert.Equal("/analyze", request.RequestUri!.AbsolutePath);
            var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
            var parts = multipart.ToArray();
            Assert.Equal(3, parts.Length);
            Assert.Equal(Sample, await parts.Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync());
            Assert.Equal(Jpeg, await parts.Single(p => p.Headers.ContentDisposition!.Name == "image").ReadAsByteArrayAsync());
            Assert.Equal("[]", await parts.Single(p => p.Headers.ContentDisposition!.Name == "history").ReadAsStringAsync());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
        });
        var controller = new ControllerIA(db, Service(handler));
        var response = Assert.IsType<CreatedAtActionResult>(await controller.Predizer(
            new PredicaoIAInput { ColetaId = coletaId, Image = Upload(), Data = Sample }, default));
        Assert.Equal(201, response.StatusCode);
        db.ChangeTracker.Clear();
        var saved = await db.PredicoesIA.SingleAsync();
        Assert.Equal(coletaId, saved.ColetaId);
        Assert.Equal(Jpeg, saved.ImagemOriginal);
        Assert.Equal(Jpeg, saved.ImagemResultado);
        Assert.Equal(Sample, saved.EntradaJson);
        using var result = JsonDocument.Parse(saved.ResultadoJson!);
        Assert.Equal(1, result.RootElement.GetProperty("totalObjects").GetInt32());
        Assert.Equal(2, result.RootElement.GetProperty("riskLevel").GetInt32());
        Assert.False(result.RootElement.TryGetProperty("annotatedImage", out _));
        Assert.Equal(1, handler.Calls);
        Assert.IsType<OkObjectResult>(await controller.Obter(saved.Id, default));
        Assert.Equal(Jpeg, Assert.IsType<FileContentResult>(await controller.ImagemOriginal(saved.Id, default)).FileContents);
        Assert.Equal(Jpeg, Assert.IsType<FileContentResult>(await controller.Imagem(saved.Id, default)).FileContents);
    }

    [Fact]
    public async Task AnaliseVisualTambemSalvaDeteccoesSemFabricarMetais()
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        { Content = new StringContent(ValidResponse()) }));
        var result = await Service(handler).CriarAsync(coletaId, Upload(), null, default);
        Assert.Equal("vision", result.Tipo);
        Assert.Null(result.EntradaJson);
        Assert.NotNull(result.ResultadoJson);
    }

    [Theory]
    [InlineData(400, 400)]
    [InlineData(413, 413)]
    [InlineData(422, 422)]
    [InlineData(503, 502)]
    public async Task FalhaDaIaNaoDeixaRegistroParcial(int upstream, int expected)
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)upstream)));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), Sample, default));
        Assert.Equal(expected, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData(false, 502)]
    [InlineData(true, 504)]
    public async Task ConexaoOuTimeoutNaoGravam(bool timeout, int expected)
    {
        var handler = new Handler(_ => throw (timeout ? new TaskCanceledException() : new HttpRequestException()));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), Sample, default));
        Assert.Equal(expected, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("<html>erro</html>")]
    [InlineData("{\"detections\":[],\"totalObjects\":0,\"metalPredictions\":[],\"visionModelVersion\":\"v\",\"annotatedImageContentType\":3,\"annotatedImage\":\"invalid\"}")]
    public async Task RespostaInvalidaNaoGrava(string body)
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), Sample, default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task ArquivoInvalidoNaoChamaIa()
    {
        var handler = new Handler(_ => throw new Exception("A IA não deve ser chamada"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(Encoding.UTF8.GetBytes("texto")), Sample, default));
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(0, handler.Calls);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [RealIaFact]
    public async Task ModeloRealViaHttpPersisteImagemEResultado()
    {
        using var http = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("WATERPATH_IA_TEST_URL")!) };
        var bytes = await File.ReadAllBytesAsync(Environment.GetEnvironmentVariable("WATERPATH_TEST_IMAGE")!);
        var ia = new IaClient(http);
        var service = new PredicaoIAService(db, ia);
        var collection = await db.Coletas.SingleAsync(c => c.Id == coletaId);
        for (var days = 3; days >= 1; days--)
        {
            var previous = new ColetaEntity { CorpoHidricoId = collection.CorpoHidricoId, DataHora = collection.DataHora.AddDays(-days) };
            db.Coletas.Add(previous);
            await db.SaveChangesAsync();
            var previousPrediction = await service.CriarAsync(previous.Id, Upload(bytes), Sample.Replace("\"oxigenio_dissolvido\":6", "\"oxigenio_dissolvido\":4"), default);
            using var analysis = JsonDocument.Parse(previousPrediction.ResultadoJson!);
            Assert.InRange(analysis.RootElement.GetProperty("baseRiskLevel").GetInt32(), 2, 3);
        }
        await using var app = await HttpHost(ia);
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(coletaId.ToString()), "coletaId");
        form.Add(new StringContent(Sample), "data");
        form.Add(new ByteArrayContent(bytes), "image", "rio.jpg");
        var response = await api.PostAsync("/api/ia/predicoes", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        db.ChangeTracker.Clear();
        var saved = await db.PredicoesIA.SingleAsync(p => p.ColetaId == coletaId);
        Assert.Equal(coletaId, saved.ColetaId);
        Assert.Equal(bytes, saved.ImagemOriginal);
        using var result = JsonDocument.Parse(saved.ResultadoJson!);
        Assert.NotEmpty(result.RootElement.GetProperty("metalPredictions").EnumerateArray());
        Assert.NotEmpty(result.RootElement.GetProperty("visionModelVersion").GetString()!);
        Assert.InRange(result.RootElement.GetProperty("riskLevel").GetInt32(), 1, 3);
        Assert.Equal(3, result.RootElement.GetProperty("history").GetProperty("alertCollections").GetInt32());
        Assert.Equal(Math.Min(3, result.RootElement.GetProperty("baseRiskLevel").GetInt32() + 1), result.RootElement.GetProperty("riskLevel").GetInt32());
        Assert.Equal("image/jpeg", saved.ContentTypeResultado);
        Assert.True(saved.ImagemResultado.Length > 100);
        Assert.Equal(bytes, await api.GetByteArrayAsync($"/api/ia/predicoes/{saved.Id}/imagem/original"));
        Assert.Equal(saved.ImagemResultado, await api.GetByteArrayAsync($"/api/ia/predicoes/{saved.Id}/imagem"));
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/ia/predicoes/{saved.Id}")).StatusCode);
        using var invalid = new MultipartFormDataContent();
        invalid.Add(new StringContent(coletaId.ToString()), "coletaId");
        invalid.Add(new StringContent(Sample), "data");
        invalid.Add(new ByteArrayContent(Jpeg), "image", "invalida.jpg");
        Assert.Equal(HttpStatusCode.BadRequest, (await api.PostAsync("/api/ia/predicoes", invalid)).StatusCode);
        Assert.Equal(4, await db.PredicoesIA.CountAsync());
        await app.StopAsync();
    }

    private async Task<WebApplication> HttpHost(IaClient ia)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(db);
        builder.Services.AddSingleton(ia);
        builder.Services.AddScoped<PredicaoIAService>();
        builder.Services.AddControllers().AddApplicationPart(typeof(ControllerIA).Assembly);
        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task HistoricoUsaColetasAnterioresDoMesmoRioSemDuplicarFotos()
    {
        var riverId = (await db.Coletas.SingleAsync(c => c.Id == coletaId)).CorpoHidricoId;
        var now = DateTimeOffset.UtcNow;
        var previousIds = new List<int>();
        for (var days = 1; days <= 6; days++)
        {
            var previous = new ColetaEntity { CorpoHidricoId = riverId, DataHora = now.AddDays(-days) };
            db.Coletas.Add(previous);
            await db.SaveChangesAsync();
            previousIds.Add(previous.Id);
            // Duas fotos na mesma coleta continuam sendo uma ocorrência.
            await SavePrediction(previous.Id, 2, now.AddMinutes(-2));
            await SavePrediction(previous.Id, 1, now.AddMinutes(-1));
        }
        await SavePrediction(coletaId, 3, now);
        var otherRiver = new CorpoHidricoEntity("Outro rio", "Outro local", 10, false);
        db.CorposHidricos.Add(otherRiver);
        await db.SaveChangesAsync();
        var otherCollection = new ColetaEntity { CorpoHidricoId = otherRiver.Id, DataHora = now.AddDays(-1) };
        var futureCollection = new ColetaEntity { CorpoHidricoId = riverId, DataHora = now.AddDays(1) };
        db.Coletas.AddRange(otherCollection, futureCollection);
        await db.SaveChangesAsync();
        await SavePrediction(otherCollection.Id, 3, now);
        await SavePrediction(futureCollection.Id, 3, now);
        var handler = new Handler(async request =>
        {
            var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToArray();
            var json = await parts.Single(p => p.Headers.ContentDisposition!.Name == "history").ReadAsStringAsync();
            using var history = JsonDocument.Parse(json);
            var samples = history.RootElement.EnumerateArray().ToArray();
            Assert.Equal(5, samples.Length);
            Assert.Equal(previousIds.Take(5), samples.Select(s => s.GetProperty("coletaId").GetInt32()));
            Assert.All(samples, s => Assert.Equal(1, s.GetProperty("baseRiskLevel").GetInt32()));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
        });
        await Service(handler).CriarAsync(coletaId, Upload(), Sample, default);
        Assert.Equal(1, handler.Calls);
    }

    private async Task SavePrediction(int id, int level, DateTimeOffset created)
    {
        db.PredicoesIA.Add(new back_end.src.IA.Domain.PredicaoIAEntity
        {
            ColetaId = id, CriadaEm = created.UtcDateTime, Tipo = "integrada", NomeArquivo = "rio.jpg",
            ContentTypeOriginal = "image/jpeg", ImagemOriginal = Jpeg,
            ContentTypeResultado = "image/jpeg", ImagemResultado = Jpeg,
            ResultadoJson = JsonSerializer.Serialize(new { baseRiskLevel = level, riskRuleVersion = PredicaoIAService.VersaoRegraRisco }),
        });
        await db.SaveChangesAsync();
    }

    private sealed class TestDbContext(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // SQLite não ordena DateTimeOffset; os testes conservam o instante UTC.
            modelBuilder.Entity<ColetaEntity>().Property(c => c.DataHora)
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return respond(request);
        }
    }
}

public sealed class RealIaFactAttribute : FactAttribute
{
    public RealIaFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WATERPATH_IA_TEST_URL"))
            || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WATERPATH_TEST_IMAGE")))
            Skip = "Defina WATERPATH_IA_TEST_URL e WATERPATH_TEST_IMAGE para testar os modelos reais via HTTP.";
    }
}
