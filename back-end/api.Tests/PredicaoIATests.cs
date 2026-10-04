using System.Net;
using System.Net.Http.Headers;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA;
using back_end.src.IA.Application;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WaterPath.Api.Tests;

public class PredicaoIATests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly WaterPathDbContext context;
    private readonly byte[] jpeg = [255, 216, 255, 224, 1, 2, 3];
    private const string Data = "{\"temperatura\":22,\"ph\":7,\"condutividade_eletrica\":100,\"oxigenio_dissolvido\":6}";
    private const string Result = "{\"detections\":[],\"totalObjects\":0,\"metalPredictions\":[{\"name\":\"Iron\",\"value\":0.1,\"unit\":\"mg/L\"}]}";

    public PredicaoIATests()
    {
        connection.Open();
        context = new(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        var corpo = new CorpoHidricoEntity("Rio", "Campinas", 10, false);
        context.CorposHidricos.Add(corpo);
        context.SaveChanges();
        context.Coletas.Add(new ColetaEntity(corpo.Id, DateTimeOffset.UtcNow, null, null, null));
        context.SaveChanges();
    }

    private IFormFile Image() => new FormFile(new MemoryStream(jpeg), 0, jpeg.Length, "image", "rio.jpg")
    { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };

    private PredicaoIAService Service(FakeHandler handler) => new(context,
        new IaClient(new HttpClient(handler) { BaseAddress = new Uri("http://ia/") }));

    private HttpResponseMessage ImageResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(jpeg) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        return response;
    }

    [Fact]
    public async Task SalvaResultadoEImagensEPermiteConsulta()
    {
        var handler = new FakeHandler(request => request.RequestUri!.AbsolutePath == "/predict"
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Result) } : ImageResponse());
        var service = Service(handler);
        var p = await service.CriarAsync(1, Image(), Data, default);
        context.ChangeTracker.Clear();
        var saved = await context.PredicoesIA.SingleAsync();
        Assert.Equal(Result, saved.ResultadoJson);
        Assert.Equal(Data, saved.EntradaJson);
        Assert.Equal(jpeg, saved.ImagemOriginal);
        Assert.Equal(jpeg, saved.ImagemResultado);
        Assert.Equal(1, saved.ColetaId);
        Assert.Equal("integrada", saved.Tipo);
        Assert.Equal(new[] { "data", "image" }, handler.Fields[0]);
        Assert.Equal(new[] { "file" }, handler.Fields[1]);
        var controller = new ControllerIA(context, service);
        Assert.IsType<OkObjectResult>(await controller.Obter(p.Id, default));
        var file = Assert.IsType<FileContentResult>(await controller.Imagem(p.Id, default));
        Assert.Equal(jpeg, file.FileContents);
        Assert.IsType<NotFoundObjectResult>(await controller.Obter(999, default));
        var original = Assert.IsType<FileContentResult>(await controller.ImagemOriginal(p.Id, default));
        Assert.Equal(jpeg, original.FileContents);
        Assert.IsType<OkObjectResult>(await controller.Listar(1, default));
    }

    [Fact]
    public async Task FalhaNaSegundaChamadaNaoGravaPredicao()
    {
        var handler = new FakeHandler(request => request.RequestUri!.AbsolutePath == "/predict"
            ? new(HttpStatusCode.OK) { Content = new StringContent(Result) } : new(HttpStatusCode.InternalServerError));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(1, Image(), Data, default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await context.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"detections\":[],\"totalObjects\":\"bad\",\"metalPredictions\":[]}")]
    [InlineData("{}")]
    public async Task RespostaInvalidaDaIANaoGrava(string json)
    {
        var handler = new FakeHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent(json) });
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(1, Image(), Data, default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await context.PredicoesIA.ToListAsync());
    }

    [Theory]
    [InlineData(999, Data, 404)]
    [InlineData(1, "invalid", 400)]
    public async Task ValidaAntesDeChamarIA(int coletaId, string data, int status)
    {
        var handler = new FakeHandler(_ => throw new Exception("Não deve chamar IA"));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(coletaId, Image(), data, default));
        Assert.Equal(status, error.StatusCode);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    public async Task VisionSalvaSomenteImagensSemInventarMetais()
    {
        var handler = new FakeHandler(_ => ImageResponse());
        var p = await Service(handler).CriarAsync(1, Image(), null, default);
        Assert.Equal("vision", p.Tipo);
        Assert.Null(p.ResultadoJson);
        Assert.Single(handler.Bodies);
    }

    [Theory]
    [InlineData(422, 422)]
    [InlineData(500, 502)]
    public async Task TrataErrosDoServico(int upstream, int expected)
    {
        var handler = new FakeHandler(_ => new((HttpStatusCode)upstream));
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(1, Image(), Data, default));
        Assert.Equal(expected, error.StatusCode);
        Assert.Empty(await context.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task TimeoutRetorna504SemGravar()
    {
        var handler = new FakeHandler(_ => throw new TaskCanceledException());
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(1, Image(), Data, default));
        Assert.Equal(504, error.StatusCode);
        Assert.Empty(await context.PredicoesIA.ToListAsync());
    }

    [Fact]
    public async Task ImagemInvalidaRetornadaNaoGrava()
    {
        var handler = new FakeHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent("not an image") });
        var error = await Assert.ThrowsAsync<IaException>(() => Service(handler).CriarAsync(1, Image(), null, default));
        Assert.Equal(502, error.StatusCode);
        Assert.Empty(await context.PredicoesIA.ToListAsync());
    }

    public void Dispose() { context.Dispose(); connection.Dispose(); }

    private class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];
        public List<string[]> Fields { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            Fields.Add(((MultipartFormDataContent)request.Content).Select(part =>
                part.Headers.ContentDisposition!.Name!.Trim('"')).ToArray());
            return responder(request);
        }
    }
}
