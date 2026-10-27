using System.Net;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using back_end.src.Authorization;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA;
using back_end.src.IA.Application;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Medicoes.Domain;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
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
        collection.RegistrarResponsavel(TestJwt.UserId, "Usuário de teste");
        AddRequiredMeasurements(collection);
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

    private static void AddRequiredMeasurements(ColetaEntity collection, double oxygen = 6)
    {
        collection.AdicionarMedicao(new MedicoesEntity(Medicao.Temperatura, 22, "°C", false, null));
        collection.AdicionarMedicao(new MedicoesEntity(Medicao.Ph, 7, "pH", false, null));
        collection.AdicionarMedicao(new MedicoesEntity(Medicao.CondutividadeEletrica, 100, "uS/cm", false, null));
        collection.AdicionarMedicao(new MedicoesEntity(Medicao.OxigenioDissolvido, oxygen, "mg/L", false, null));
    }

    private static string ValidResponse(string history = "[]") => RiskFixture.Result(history: history);

    private PredicaoIAService Service(Handler handler, string? contentRoot = null) => new(db,
        new IaClient(new HttpClient(handler) { BaseAddress = new Uri("http://ia.test/") }),
        environment: contentRoot is null ? null : new TestEnvironment(contentRoot));

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "WaterPath.Api.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private ControllerIA Controller(Handler handler)
    {
        var controller = new ControllerIA(db, Service(handler), new ResourceAuthorizationService(db));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = TestJwt.Principal() },
        };
        return controller;
    }

    [Fact]
    public void ClienteIaPodeSerResolvidoPeloRegistroHttpDaAplicacao()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<IaClient>(client => client.BaseAddress = new Uri("http://127.0.0.1:18000/"));
        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IaClient>());
    }

    [Fact]
    public async Task RotasDeIaExigemJwtEPermissaoSobreAColeta()
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(ValidResponse()) }));
        await using var app = await HttpHost(new IaClient(new HttpClient(handler)
            { BaseAddress = new Uri("http://ia.test/") }));
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };

        static MultipartFormDataContent Form(int collectionId)
        {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(collectionId.ToString()), "coletaId");
            form.Add(new ByteArrayContent(Jpeg), "image", "rio.jpg");
            return form;
        }

        using (var unauthenticated = Form(coletaId))
            Assert.Equal(HttpStatusCode.Unauthorized,
                (await api.PostAsync("/api/ia/predicoes", unauthenticated)).StatusCode);

        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", TestJwt.Token(999));
        using (var forbidden = Form(coletaId))
            Assert.Equal(HttpStatusCode.Forbidden,
                (await api.PostAsync("/api/ia/predicoes", forbidden)).StatusCode);

        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public void ContratoPublicoNaoExpoeEntradaContextoOuVersoesDosModelos()
    {
        var prediction = new back_end.src.IA.Domain.PredicaoIAEntity
        {
            Id = 7, ColetaId = coletaId, CorpoHidricoId = 3, DataColeta = DateTime.UtcNow,
            CriadaEm = DateTime.UtcNow, Tipo = "integrada", ResultadoJson = ValidResponse(),
        };
        var json = JsonSerializer.SerializeToElement(PredicaoIAPublicContract.Criar(prediction),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var result = json.GetProperty("resultado");

        Assert.True(result.TryGetProperty("riskLevel", out _));
        Assert.True(result.TryGetProperty("metalPredictions", out _));
        Assert.False(result.TryGetProperty("visionModelVersion", out _));
        Assert.False(result.TryGetProperty("metalModelVersion", out _));
        Assert.False(result.TryGetProperty("riskInputs", out _));
        Assert.False(result.TryGetProperty("history", out _));
        Assert.False(result.TryGetProperty("collectionContext", out _));
        Assert.False(json.TryGetProperty("entrada", out _));
    }

    [Fact]
    public async Task ColetaSemMedicaoObrigatoriaRetorna422SemChamarIa()
    {
        var collection = new ColetaEntity { CorpoHidricoId = (await db.Coletas.SingleAsync()).CorpoHidricoId,
            DataHora = DateTimeOffset.UtcNow.AddMinutes(-1) };
        collection.RegistrarResponsavel(TestJwt.UserId, "Usuário de teste");
        db.Coletas.Add(collection);
        await db.SaveChangesAsync();
        var handler = new Handler(_ => throw new Exception("Entrada inválida não deve chamar IA"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(collection.Id, Upload(), default));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(0, handler.Calls);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task ColetaFuturaOuDataDivergenteNaoEAnaliseAtual()
    {
        var handler = new Handler(_ => throw new Exception("Não deve chamar IA"));
        var service = Service(handler);
        var coleta = await db.Coletas.SingleAsync();
        coleta.DataHora = DateTimeOffset.UtcNow.AddDays(1);
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<IaException>(() => service.CriarAsync(coletaId, Upload(), default));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(0, handler.Calls);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData("riskInputs", "{\"ph\":7,\"oxigenio_dissolvido\":4,\"visualClasses\":[\"Lixo\"]}")]
    [InlineData("riskInputs", "{\"ph\":7,\"oxigenio_dissolvido\":6,\"visualClasses\":[]}")]
    [InlineData("riskInputs", "null")]
    [InlineData("history", "{\"evaluatedCollections\":1,\"alertCollections\":1,\"adjustment\":0,\"samples\":[{\"predictionId\":123,\"coletaId\":123,\"baseRiskLevel\":2}]}")]
    [InlineData("riskLevel", "1")]
    [InlineData("riskReasons", "[1]")]
    public async Task RespostaDivergenteDosDadosUtilizadosNaoGrava(string field, string value)
    {
        var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ValidResponse())!;
        fields[field] = JsonSerializer.Deserialize<JsonElement>(value);
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonSerializer.Serialize(fields)) }));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task AmostraEHistoricoSaoMontadosPelaApiPrincipal()
    {
        string? sent = null;
        var handler = new Handler(async request =>
        {
            sent = await Assert.IsType<MultipartFormDataContent>(request.Content)
                .Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
        });
        var saved = await Service(handler).CriarAsync(coletaId, Upload(), default);
        Assert.Equal(sent, saved.EntradaJson);
        using var json = JsonDocument.Parse(saved.EntradaJson!);
        Assert.Equal(22, json.RootElement.GetProperty("temperatura").GetDouble());
        Assert.Equal(7, json.RootElement.GetProperty("ph").GetDouble());
        Assert.Equal(100, json.RootElement.GetProperty("condutividade_eletrica").GetDouble());
        using var result = JsonDocument.Parse(saved.ResultadoJson!);
        Assert.Equal(6, result.RootElement.GetProperty("riskInputs").GetProperty("oxigenio_dissolvido").GetDouble());
    }

    [Fact]
    public async Task UmaChamadaSalvaResultadoEImagensDaMesmaColeta()
    {
        var handler = new Handler(async request =>
        {
            Assert.Equal("/analyze", request.RequestUri!.AbsolutePath);
            var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
            var parts = multipart.ToArray();
            Assert.Equal(4, parts.Length);
            var sent = await parts.Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync();
            Assert.Equal(7, ContratoAmostra.Ler(sent).GetProperty("ph").GetDouble());
            Assert.True(ContratoAmostra.Ler(sent).TryGetProperty("data", out _));
            Assert.Equal(Jpeg, await parts.Single(p => p.Headers.ContentDisposition!.Name == "image").ReadAsByteArrayAsync());
            Assert.Equal("[]", await parts.Single(p => p.Headers.ContentDisposition!.Name == "history").ReadAsStringAsync());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
        });
        var controller = Controller(handler);
        var response = Assert.IsType<CreatedAtActionResult>(await controller.Predizer(
            new PredicaoIAInput { ColetaId = coletaId, Image = Upload() }, default));
        Assert.Equal(201, response.StatusCode);
        db.ChangeTracker.Clear();
        var saved = await db.PredicoesIA.SingleAsync();
        Assert.Equal(coletaId, saved.ColetaId);
        Assert.Equal(Jpeg, saved.ImagemOriginal);
        Assert.Equal(Jpeg, saved.ImagemResultado);
        Assert.Equal(7, ContratoAmostra.Ler(saved.EntradaJson!).GetProperty("ph").GetDouble());
        Assert.Equal((await db.Coletas.SingleAsync()).CorpoHidricoId, saved.CorpoHidricoId);
        Assert.Equal((await db.Coletas.SingleAsync()).DataHora.UtcDateTime, saved.DataColeta);
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
    public async Task RequisicaoComSomenteColetaIdUsaImagemVinculadaEPersisteQualidadeFutura()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var linkedPath = Path.Combine(root, "linked.jpg");
            var bodyPath = Path.Combine(root, "body-latest.jpg");
            var linkedBytes = new byte[] { 0xff, 0xd8, 0xff, 1 };
            var bodyBytes = new byte[] { 0xff, 0xd8, 0xff, 2 };
            await File.WriteAllBytesAsync(linkedPath, linkedBytes);
            await File.WriteAllBytesAsync(bodyPath, bodyBytes);
            var collection = await db.Coletas.SingleAsync(c => c.Id == coletaId);
            var river = await db.CorposHidricos.SingleAsync(c => c.Id == collection.CorpoHidricoId);
            db.Imagens.AddRange(
                new ImagemEntity { Url = "linked.jpg", Coleta = collection, CorpoHidrico = river, DataUpload = DateTime.UtcNow.AddDays(-1) },
                new ImagemEntity { Url = "body-latest.jpg", CorpoHidrico = river, DataUpload = DateTime.UtcNow });
            await db.SaveChangesAsync();

            byte[]? sentImage = null;
            var handler = new Handler(async request =>
            {
                var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToArray();
                var sentData = await parts.Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync();
                Assert.Equal(22, ContratoAmostra.Ler(sentData).GetProperty("temperatura").GetDouble());
                sentImage = await parts.Single(p => p.Headers.ContentDisposition!.Name == "image").ReadAsByteArrayAsync();
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
            });
            var service = Service(handler, root);
            var controller = new ControllerIA(db, service, new ResourceAuthorizationService(db));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = TestJwt.Principal() },
            };
            Assert.IsType<CreatedAtActionResult>(await controller.Predizer(
                new PredicaoIAInput { ColetaId = coletaId }, default));
            var saved = await db.PredicoesIA.SingleAsync();

            Assert.Equal(linkedBytes, sentImage);
            Assert.Equal(linkedBytes, saved.ImagemOriginal);
            Assert.Equal(coletaId, saved.ColetaId);
            var quality = await db.QualidadesFuturas.SingleAsync();
            Assert.Equal(coletaId, quality.ColetaId);
            Assert.Equal(collection.CorpoHidricoId, quality.CorpoHidricoId);
            Assert.Equal(DateOnly.FromDateTime(collection.DataHora.UtcDateTime), quality.DataReferencia);
            using var result = JsonDocument.Parse(quality.ResultadoJson!);
            Assert.Equal(2, result.RootElement.GetProperty("riskLevel").GetInt32());
            Assert.Equal(8, result.RootElement.GetProperty("metalPredictions").GetArrayLength());
            Assert.Equal(1, handler.Calls);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ImagemDoCorpoHidricoEscolhidaPorDataUploadENaoPorId()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var newerPath = Path.Combine(root, "newer.jpg");
            var olderPath = Path.Combine(root, "older.jpg");
            var newer = new byte[] { 0xff, 0xd8, 0xff, 3 };
            var older = new byte[] { 0xff, 0xd8, 0xff, 4 };
            await File.WriteAllBytesAsync(newerPath, newer);
            await File.WriteAllBytesAsync(olderPath, older);
            var collection = await db.Coletas.SingleAsync(c => c.Id == coletaId);
            var river = await db.CorposHidricos.SingleAsync(c => c.Id == collection.CorpoHidricoId);
            var uploadedAt = DateTime.UtcNow;
            db.Imagens.AddRange(
                new ImagemEntity { Url = "newer.jpg", CorpoHidrico = river, DataUpload = uploadedAt },
                new ImagemEntity { Url = "older.jpg", CorpoHidrico = river, DataUpload = uploadedAt.AddDays(-1) });
            await db.SaveChangesAsync();
            byte[]? sentImage = null;
            var handler = new Handler(async request =>
            {
                var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToArray();
                sentImage = await parts.Single(p => p.Headers.ContentDisposition!.Name == "image").ReadAsByteArrayAsync();
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse()) };
            });

            var saved = await Service(handler, root).CriarAsync(coletaId, null, default);

            Assert.Equal(newer, sentImage);
            Assert.Equal(newer, saved.ImagemOriginal);
            Assert.Single(await db.QualidadesFuturas.ToListAsync());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task SemImagemDisponivelRetornaErroSemChamarIaNemPersistir()
    {
        var handler = new Handler(_ => throw new Exception("Sem imagem, não chama a IA"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, null, default));
        Assert.Equal(422, error.StatusCode);
        Assert.Contains("Nenhuma imagem", error.Message);
        Assert.Equal(0, handler.Calls);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
        Assert.Empty(await db.QualidadesFuturas.ToListAsync());
    }

    [Fact]
    public async Task RepetirAnaliseComMesmaChaveNaoChamaIaNovamente()
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(ValidResponse()) }));
        var controller = Controller(handler);
        var key = Guid.NewGuid().ToString();
        var first = Assert.IsType<CreatedAtActionResult>(await controller.Predizer(
            new PredicaoIAInput { ColetaId = coletaId, Image = Upload() }, default, key));
        var repeated = Assert.IsType<CreatedAtActionResult>(await controller.Predizer(
            new PredicaoIAInput { ColetaId = coletaId, Image = Upload() }, default, key));
        Assert.Equal(JsonSerializer.SerializeToElement(first.Value).GetProperty("Id").GetInt32(),
            JsonSerializer.SerializeToElement(repeated.Value).GetProperty("Id").GetInt32());
        Assert.Equal(1, handler.Calls);
        Assert.Equal(1, await db.PredicoesIA.CountAsync());
        Assert.Equal(1, await db.QualidadesFuturas.CountAsync());

        Assert.IsType<ConflictObjectResult>(await controller.Predizer(
            new PredicaoIAInput { ColetaId = coletaId, Image = Upload(Encoding.UTF8.GetBytes("different")) },
            default, key));
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task SomenteResponsavelRetomaColetaSemAnaliseIntegrada()
    {
        var controller = Controller(new Handler(_ => throw new Exception("Nao deve chamar IA")));
        Assert.IsType<OkObjectResult>(await controller.RetomarColeta(coletaId, default));
        controller.HttpContext.User = TestJwt.Principal(999);
        Assert.IsType<NotFoundObjectResult>(await controller.RetomarColeta(coletaId, default));
    }

    [Fact]
    public async Task AnaliseDaColetaSempreValidaMedicoesAntesDaVisao()
    {
        var collection = new ColetaEntity { CorpoHidricoId = (await db.Coletas.SingleAsync()).CorpoHidricoId,
            DataHora = DateTimeOffset.UtcNow.AddMinutes(-1) };
        collection.RegistrarResponsavel(TestJwt.UserId, "Usuário de teste");
        db.Coletas.Add(collection);
        await db.SaveChangesAsync();
        var handler = new Handler(_ => throw new Exception("Não deve executar visão"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(collection.Id, Upload(), default));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(400, 400)]
    [InlineData(413, 413)]
    [InlineData(422, 422)]
    [InlineData(503, 503)]
    [InlineData(429, 503)]
    public async Task FalhaDaIaNaoDeixaRegistroParcial(int upstream, int expected)
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)upstream)));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(expected, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
        Assert.Empty(await db.QualidadesFuturas.ToListAsync());
    }

    [Theory]
    [InlineData(false, 503)]
    [InlineData(true, 504)]
    public async Task ConexaoOuTimeoutNaoGravam(bool timeout, int expected)
    {
        var handler = new Handler(_ => throw (timeout ? new TaskCanceledException() : new HttpRequestException()));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(expected, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
        Assert.Empty(await db.QualidadesFuturas.ToListAsync());
    }

    [Fact]
    public async Task FalhaAoSalvarQualidadeFuturaRevertePredicaoEImagens()
    {
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER falha_qualidade_futura BEFORE INSERT ON QualidadesFuturas BEGIN SELECT RAISE(ABORT, 'falha simulada'); END;");
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(ValidResponse()) }));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(500, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
        Assert.Empty(await db.QualidadesFuturas.ToListAsync());
        Assert.DoesNotContain(db.ChangeTracker.Entries<back_end.src.IA.Domain.PredicaoIAEntity>(), e => e.State == EntityState.Added);
        Assert.DoesNotContain(db.ChangeTracker.Entries<back_end.src.Domain.QualidadeFutura.QualidadeFuturaEntity>(), e => e.State == EntityState.Added);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("<html>erro</html>")]
    [InlineData("{\"detections\":[],\"totalObjects\":0,\"metalPredictions\":[],\"visionModelVersion\":\"v\",\"annotatedImageContentType\":3,\"annotatedImage\":\"invalid\"}")]
    public async Task RespostaInvalidaNaoGrava(string body)
    {
        var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task ArquivoInvalidoNaoChamaIa()
    {
        var handler = new Handler(_ => throw new Exception("A IA não deve ser chamada"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(Encoding.UTF8.GetBytes("texto")), default));
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
            AddRequiredMeasurements(previous, oxygen: 4);
            db.Coletas.Add(previous);
            await db.SaveChangesAsync();
            var previousPrediction = await service.CriarAsync(previous.Id, Upload(bytes), default);
            using var analysis = JsonDocument.Parse(previousPrediction.ResultadoJson!);
            Assert.InRange(analysis.RootElement.GetProperty("baseRiskLevel").GetInt32(), 2, 3);
        }
        await using var app = await HttpHost(ia);
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(coletaId.ToString()), "coletaId");
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
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        var riskResponse = await api.GetAsync($"/api/corpohidrico/{collection.CorpoHidricoId}/risco-atual");
        Assert.Equal(HttpStatusCode.OK, riskResponse.StatusCode);
        using var currentRisk = JsonDocument.Parse(await riskResponse.Content.ReadAsStringAsync());
        Assert.Equal(saved.Id, currentRisk.RootElement.GetProperty("predicaoId").GetInt32());
        Assert.Equal(coletaId, currentRisk.RootElement.GetProperty("coletaId").GetInt32());
        Assert.Equal(result.RootElement.GetProperty("riskLevel").GetInt32(), currentRisk.RootElement.GetProperty("nivelRisco").GetInt32());
        Assert.Equal(4, await db.PredicoesIA.CountAsync());
        using var invalid = new MultipartFormDataContent();
        invalid.Add(new StringContent(coletaId.ToString()), "coletaId");
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
        builder.Services.AddScoped<ResourceAuthorizationService>();
        builder.Services.AddScoped<back_end.src.Domain.CorpoHidrico.ICorpoHidricoRepository, back_end.src.Infrastructure.Repository.CorpoHidricoRepository>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ControllerIA).Assembly));
        TestJwt.Configure(builder.Services);
        builder.Services.AddControllers().AddApplicationPart(typeof(ControllerIA).Assembly);
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
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
            Assert.Equal(previousIds.Take(5).Reverse(), samples.Select(s => s.GetProperty("coletaId").GetInt32()));
            Assert.All(samples, s => Assert.Equal(1, s.GetProperty("baseRiskLevel").GetInt32()));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidResponse(json)) };
        });
        await Service(handler).CriarAsync(coletaId, Upload(), default);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task UnidadeDeMetalMedidoForaDoContratoRetorna422()
    {
        var collection = await db.Coletas.SingleAsync(c => c.Id == coletaId);
        db.MetaisPesados.Add(new MetalPesadoEntity("Pb", 1, "ppm", collection));
        await db.SaveChangesAsync();
        var handler = new Handler(_ => throw new Exception("A IA não deve ser chamada"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Upload(), default));
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(0, handler.Calls);
    }

    private static void MedicoesSalvas(ColetaEntity collection)
    {
        Adicionar(back_end.src.Medicoes.Domain.Medicao.Temperatura, 25, "°C");
        Adicionar(back_end.src.Medicoes.Domain.Medicao.Ph, 7, "pH");
        Adicionar(back_end.src.Medicoes.Domain.Medicao.OxigenioDissolvido, 6, "mg/L");
        Adicionar(back_end.src.Medicoes.Domain.Medicao.CondutividadeEletrica, 123, "µS/cm");
        Adicionar(back_end.src.Medicoes.Domain.Medicao.FosforoTotal, 0.02, "mg/L");
        Adicionar(back_end.src.Medicoes.Domain.Medicao.SolidosSuspensosTotais, null, "mg/L", true, 0.1);

        void Adicionar(back_end.src.Medicoes.Domain.Medicao code, double? value, string unit,
            bool censored = false, double? limit = null)
        {
            var existing = collection.Medicoes.SingleOrDefault(m => m.codigoMedicao == code);
            if (existing is null) collection.AdicionarMedicao(new(code, value, unit, censored, limit));
            else existing.Atualizar(code, value, unit, censored, limit);
        }
    }

    private static async Task<HttpResponseMessage> EcoAnalise(HttpRequestMessage request, bool echoContext = true)
    {
        var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToArray();
        var sample = await parts.Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync();
        var history = await parts.Single(p => p.Headers.ContentDisposition!.Name == "history").ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(RiskFixture.Result(sample, history))!;
        result["metalModelVersion"] = JsonSerializer.SerializeToElement("modelo-de-teste");
        result["metalPredictions"] = JsonSerializer.SerializeToElement(new[] { "Fe", "Mn", "Cr", "Ni", "Cu", "Zn", "Cd", "Pb" }
            .Select(symbol => new { name = symbol, value = 0.1, unit = symbol is "Fe" or "Mn" ? "mg/L" : "µg/L" }));
        if (echoContext)
            result["collectionContext"] = JsonSerializer.Deserialize<JsonElement>(await parts.Single(
                p => p.Headers.ContentDisposition!.Name == "collectionContext").ReadAsStringAsync());
        return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result)) };
    }

    [Fact]
    public async Task AnalisePorLagoViaHttpUsaBancoIsolaEOrdenaHistorico()
    {
        var current = await db.Coletas.SingleAsync();
        MedicoesSalvas(current);
        var previous = new List<ColetaEntity>();
        foreach (var days in new[] { 3, 1, 6, 2, 5, 4 })
        {
            var collection = new ColetaEntity { CorpoHidricoId = current.CorpoHidricoId, DataHora = current.DataHora.AddDays(-days) };
            MedicoesSalvas(collection);
            previous.Add(collection);
            db.Coletas.Add(collection);
        }
        var other = new CorpoHidricoEntity("Outro lago", "Outro local", 1, false);
        db.CorposHidricos.Add(other);
        await db.SaveChangesAsync();
        db.Coletas.Add(new() { CorpoHidricoId = other.Id, DataHora = current.DataHora.AddSeconds(1) });
        await db.SaveChangesAsync();
        foreach (var collection in previous) await SavePrediction(collection.Id, 2, current.DataHora);
        var handler = new Handler(async request =>
        {
            var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToArray();
            var input = JsonSerializer.Deserialize<JsonElement>(await parts.Single(p => p.Headers.ContentDisposition!.Name == "data").ReadAsStringAsync());
            Assert.Equal(25, input.GetProperty("temperatura").GetDouble());
            Assert.Equal(123, input.GetProperty("condutividade_eletrica").GetDouble());
            Assert.Equal(20, input.GetProperty("fosforo_total").GetDouble());
            Assert.Equal(JsonValueKind.Null, input.GetProperty("solidos_suspensos_totais").ValueKind);
            var context = JsonSerializer.Deserialize<JsonElement>(await parts.Single(p => p.Headers.ContentDisposition!.Name == "collectionContext").ReadAsStringAsync());
            Assert.Equal(current.Id, context.GetProperty("currentCollectionId").GetInt32());
            var records = context.GetProperty("history").EnumerateArray().ToArray();
            Assert.Equal(previous.OrderBy(c => c.DataHora).Select(c => c.Id), records.Select(r => r.GetProperty("collectionId").GetInt32()));
            Assert.All(records, r => Assert.Equal(current.CorpoHidricoId, r.GetProperty("waterBodyId").GetInt32()));
            return await EcoAnalise(request);
        });
        await using var app = await HttpHost(new IaClient(new HttpClient(handler) { BaseAddress = new Uri("http://ia.test/") }));
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Jpeg), "image", "lago.jpg");
        var response = await api.PostAsync($"/api/ia/predicoes/corpo-hidrico/{current.CorpoHidricoId}", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = await db.PredicoesIA.SingleAsync(p => p.ColetaId == current.Id);
        Assert.Equal(current.CorpoHidricoId, saved.CorpoHidricoId);
        Assert.Equal(current.DataHora.UtcDateTime, saved.DataColeta);
        var result = JsonSerializer.Deserialize<JsonElement>(saved.ResultadoJson!);
        Assert.Equal(6, result.GetProperty("collectionContext").GetProperty("history").GetArrayLength());
        Assert.Equal(5, result.GetProperty("history").GetProperty("evaluatedCollections").GetInt32());
        Assert.Equal(3, result.GetProperty("riskLevel").GetInt32());
        Assert.Equal(1, handler.Calls);
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/corpohidrico/{current.CorpoHidricoId}/risco-atual")).StatusCode);
    }

    [Theory]
    [InlineData("inexistente", 404)]
    [InlineData("sem-coletas", 422)]
    [InlineData("sem-medicoes", 422)]
    [InlineData("unidade", 422)]
    [InlineData("censurada", 422)]
    [InlineData("futura", 422)]
    public async Task AnaliseLagoSemDadosValidosNaoChamaIa(string situation, int status)
    {
        var current = await db.Coletas.SingleAsync();
        var riverId = current.CorpoHidricoId;
        if (situation == "inexistente") riverId = int.MaxValue;
        if (situation == "sem-coletas") db.Coletas.Remove(current);
        if (situation == "sem-medicoes") db.Medicoes.RemoveRange(current.Medicoes);
        if (situation is "unidade" or "censurada" or "futura")
        {
            MedicoesSalvas(current);
            var oxygen = current.Medicoes.Single(m => m.codigoMedicao == back_end.src.Medicoes.Domain.Medicao.OxigenioDissolvido);
            if (situation == "unidade") oxygen.Atualizar(oxygen.codigoMedicao, 6, "%", false, null);
            if (situation == "censurada") oxygen.Atualizar(oxygen.codigoMedicao, null, "mg/L", true, 5);
            if (situation == "futura") current.DataHora = DateTimeOffset.UtcNow.AddDays(1);
        }
        await db.SaveChangesAsync();
        var handler = new Handler(_ => throw new Exception("Não deve chamar IA"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).AnalisarLagoAsync(riverId, Upload(), default));
        Assert.Equal(status, error.StatusCode);
        Assert.Equal(0, handler.Calls);
        Assert.Empty(await db.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData("sucesso", 201)]
    [InlineData("contexto-ausente", 502)]
    [InlineData("contexto-alterado", 502)]
    [InlineData("metais-invalidos", 502)]
    [InlineData("invalida", 502)]
    [InlineData("indisponivel", 503)]
    [InlineData("timeout", 504)]
    [InlineData("salvar", 500)]
    public async Task AnaliseLagoHistoricoVazioEFalhasSemConclusaoParcial(string situation, int status)
    {
        var current = await db.Coletas.SingleAsync();
        MedicoesSalvas(current);
        await db.SaveChangesAsync();
        if (situation == "salvar") await db.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER falha_predicao BEFORE INSERT ON PredicoesIA BEGIN SELECT RAISE(ABORT, 'falha simulada'); END;");
        var handler = new Handler(async request =>
        {
            if (situation == "indisponivel") throw new HttpRequestException();
            if (situation == "timeout") throw new TaskCanceledException();
            if (situation == "invalida") return new(HttpStatusCode.OK) { Content = new StringContent("{}") };
            var response = await EcoAnalise(request, situation != "contexto-ausente");
            if (situation is "contexto-alterado" or "metais-invalidos")
            {
                var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await response.Content.ReadAsStringAsync())!;
                if (situation == "contexto-alterado") fields["collectionContext"] = JsonSerializer.SerializeToElement(new { waterBodyId = int.MaxValue });
                else fields["metalPredictions"] = JsonSerializer.SerializeToElement(new[] { new { name = "Pb", value = -1, unit = "mg/L" } });
                response.Content = new StringContent(JsonSerializer.Serialize(fields));
            }
            return response;
        }, echoContext: situation != "contexto-ausente");
        var controller = Controller(handler);
        if (situation == "sucesso")
        {
            var response = await controller.AnalisarLago(current.CorpoHidricoId, new() { Image = Upload() }, default);
            Assert.Equal(status, Assert.IsAssignableFrom<ObjectResult>(response).StatusCode);
            var saved = await db.PredicoesIA.SingleAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(saved.ResultadoJson!);
            Assert.Empty(result.GetProperty("collectionContext").GetProperty("history").EnumerateArray());
        }
        else
        {
            var error = await Assert.ThrowsAsync<IaException>(() =>
                controller.AnalisarLago(current.CorpoHidricoId, new() { Image = Upload() }, default));
            Assert.Equal(status, error.StatusCode);
            Assert.Empty(await db.PredicoesIA.ToListAsync());
            Assert.DoesNotContain(db.ChangeTracker.Entries<back_end.src.IA.Domain.PredicaoIAEntity>(), e => e.State == EntityState.Added);
            await db.SaveChangesAsync();
            Assert.Empty(await db.PredicoesIA.ToListAsync());
        }
    }

    [RealIaFact]
    public async Task AnaliseLagoComModelosReaisUsaMedicoesSalvas()
    {
        var current = await db.Coletas.SingleAsync();
        MedicoesSalvas(current);
        db.Coletas.AddRange(new ColetaEntity { CorpoHidricoId = current.CorpoHidricoId, DataHora = current.DataHora.AddDays(-1) },
            new ColetaEntity { CorpoHidricoId = current.CorpoHidricoId, DataHora = current.DataHora.AddDays(-2) });
        await db.SaveChangesAsync();
        using var http = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("WATERPATH_IA_TEST_URL")!) };
        var bytes = await File.ReadAllBytesAsync(Environment.GetEnvironmentVariable("WATERPATH_TEST_IMAGE")!);
        await using var app = await HttpHost(new IaClient(http));
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "image", "lago.jpg");
        var response = await api.PostAsync($"/api/ia/predicoes/corpo-hidrico/{current.CorpoHidricoId}", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = await db.PredicoesIA.SingleAsync();
        Assert.Equal(current.Id, saved.ColetaId);
        var result = JsonSerializer.Deserialize<JsonElement>(saved.ResultadoJson!);
        Assert.Equal(current.Id, result.GetProperty("collectionContext").GetProperty("currentCollectionId").GetInt32());
        Assert.Equal(8, result.GetProperty("metalPredictions").GetArrayLength());
        Assert.Equal(2, result.GetProperty("collectionContext").GetProperty("history").GetArrayLength());
        Assert.Equal(bytes, saved.ImagemOriginal);
    }

    [Fact]
    public async Task EmpateNaDataUsaMaiorIdESomenteInstantesAnterioresNoHistorico()
    {
        var original = await db.Coletas.SingleAsync();
        var tied = new ColetaEntity { CorpoHidricoId = original.CorpoHidricoId, DataHora = original.DataHora };
        MedicoesSalvas(tied);
        db.Coletas.Add(tied);
        await db.SaveChangesAsync();
        var handler = new Handler(request => EcoAnalise(request));
        var saved = await Service(handler).AnalisarLagoAsync(original.CorpoHidricoId, Upload(), default);
        Assert.Equal(tied.Id, saved.ColetaId);
        var result = JsonSerializer.Deserialize<JsonElement>(saved.ResultadoJson!);
        Assert.Empty(result.GetProperty("collectionContext").GetProperty("history").EnumerateArray());
    }

    private async Task SavePrediction(int id, int level, DateTimeOffset created)
    {
        var collection = await db.Coletas.SingleAsync(c => c.Id == id);
        db.PredicoesIA.Add(new back_end.src.IA.Domain.PredicaoIAEntity
        {
            ColetaId = id, CorpoHidricoId = collection.CorpoHidricoId, DataColeta = collection.DataHora.UtcDateTime, CriadaEm = created.UtcDateTime, Tipo = "integrada", NomeArquivo = "rio.jpg",
            ContentTypeOriginal = "image/jpeg", ImagemOriginal = Jpeg,
            ContentTypeResultado = "image/jpeg", ImagemResultado = Jpeg,
            EntradaJson = level == 3 ? Sample.Replace(":6", ":4") : Sample,
            ResultadoJson = RiskFixture.Result(sample: level == 3 ? Sample.Replace(":6", ":4") : Sample,
                classes: level == 1 ? [] : ["Lixo"]),
        });
        await db.SaveChangesAsync();
    }

    private async Task<JsonElement> Summaries(int userId = TestJwt.UserId, bool mine = true,
        int page = 1, int size = 10, int[]? ids = null, string? search = null, bool oldest = false)
        => JsonSerializer.SerializeToElement(await AnalysisSummaryQuery.ReadAsync(db, userId, null, mine,
            ids ?? [], search, oldest, page, size, default), new JsonSerializerOptions(JsonSerializerDefaults.Web)
            { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

    [Fact]
    public async Task ResumosUsamUltimaPredicaoIntegradaSemImagensNemFallbackParaRiscoAntigo()
    {
        var now = DateTimeOffset.UtcNow;
        await SavePrediction(coletaId, 1, now);
        await SavePrediction(coletaId, 3, now);
        var latest = await db.PredicoesIA.OrderByDescending(p => p.Id).FirstAsync();
        var summary = (await Summaries()).GetProperty("itens")[0];
        Assert.Equal(latest.Id, summary.GetProperty("predicaoId").GetInt32());
        Assert.Equal(3, summary.GetProperty("risco").GetProperty("level").GetInt32());
        Assert.False(summary.TryGetProperty("imagemOriginal", out _));
        Assert.False(summary.TryGetProperty("resultadoJson", out _));
        latest.ResultadoJson = "{invalid";
        await db.SaveChangesAsync();
        Assert.Equal(JsonValueKind.Null, (await Summaries()).GetProperty("itens")[0].GetProperty("risco").ValueKind);
        latest.Tipo = "visual";
        await db.SaveChangesAsync();
        Assert.Equal(1, (await Summaries()).GetProperty("itens")[0].GetProperty("risco").GetProperty("level").GetInt32());
    }

    [Fact]
    public async Task ResumosIncluemColetasPendentesComPaginacaoBuscaEIsolamentoDeUsuario()
    {
        var original = await db.Coletas.SingleAsync();
        var pending = new ColetaEntity { CorpoHidricoId = original.CorpoHidricoId, DataHora = original.DataHora.AddDays(1) };
        pending.RegistrarResponsavel(TestJwt.UserId, "Teste");
        var other = new ColetaEntity { CorpoHidricoId = original.CorpoHidricoId, DataHora = original.DataHora.AddDays(2) };
        other.RegistrarResponsavel(999, "Outro usuário");
        db.Coletas.AddRange(pending, other);
        await db.SaveChangesAsync();
        var first = await Summaries(size: 1);
        Assert.Equal(2, first.GetProperty("total").GetInt32());
        Assert.Equal(pending.Id, first.GetProperty("itens")[0].GetProperty("coletaId").GetInt32());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("itens")[0].GetProperty("predicaoId").ValueKind);
        Assert.Equal(coletaId, (await Summaries(page: 2, size: 1)).GetProperty("itens")[0].GetProperty("coletaId").GetInt32());
        Assert.Equal(coletaId, (await Summaries(size: 1, oldest: true)).GetProperty("itens")[0].GetProperty("coletaId").GetInt32());
        Assert.Equal(1, (await Summaries(ids: [coletaId, other.Id])).GetProperty("total").GetInt32());
        Assert.Equal(2, (await Summaries(search: "RIO DE TESTE")).GetProperty("total").GetInt32());
        Assert.Equal(0, (await Summaries(search: "inexistente")).GetProperty("total").GetInt32());
        Assert.Equal(0, (await Summaries(userId: 888, mine: false)).GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task RotaDeResumosExigeSessaoEValidaAcessoEPaginacao()
    {
        var handler = new Handler(_ => throw new Exception("Consultar resumos não chama a IA"));
        await using var app = await HttpHost(new IaClient(new HttpClient(handler)));
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new Uri(address) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/ia/predicoes/resumos")).StatusCode);
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token(999));
        var riverId = (await db.Coletas.SingleAsync()).CorpoHidricoId;
        Assert.Equal(HttpStatusCode.Forbidden, (await api.GetAsync($"/api/ia/predicoes/resumos?corpoHidricoId={riverId}")).StatusCode);
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestJwt.Token());
        Assert.Equal(HttpStatusCode.BadRequest, (await api.GetAsync("/api/ia/predicoes/resumos?tamanhoPagina=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.GetAsync("/api/ia/predicoes/resumos?coletaIds=-1")).StatusCode);
        var response = await api.GetAsync("/api/ia/predicoes/resumos?somenteMinhas=true");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, result.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(0, handler.Calls);
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

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond, bool echoContext = true) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            var response = await respond(request);
            if (echoContext && response.IsSuccessStatusCode && request.Content is MultipartFormDataContent multipart)
            {
                var contextPart = multipart.FirstOrDefault(part => part.Headers.ContentDisposition?.Name == "collectionContext");
                if (contextPart is not null)
                {
                    try
                    {
                        var responseNode = System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))?.AsObject();
                        if (responseNode is not null && responseNode["collectionContext"] is null)
                        {
                            responseNode["collectionContext"] = System.Text.Json.Nodes.JsonNode.Parse(
                                await contextPart.ReadAsStringAsync(cancellationToken));
                            response.Content = new StringContent(responseNode.ToJsonString(), Encoding.UTF8, "application/json");
                        }
                    }
                    catch (JsonException) { }
                }
            }
            return response;
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
