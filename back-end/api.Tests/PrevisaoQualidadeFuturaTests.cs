using System.Net;
using System.Text;
using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA.Application;
using back_end.src.Medicoes.Domain;
using back_end.src.QualidadeFutura.Application;
using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WaterPath.Api.Tests;

public sealed class PrevisaoQualidadeFuturaTests : IAsyncLifetime
{
    private sealed class TestDbContext(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            // Como nos demais testes: SQLite compara/ordena o instante UTC como DateTime.
            model.Entity<ColetaEntity>().Property(c => c.DataHora).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
        }
    }

    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;
    private int riverId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new TestDbContext(new DbContextOptionsBuilder<WaterPathDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var river = new CorpoHidricoEntity("Rio", "Teste", 10, false);
        db.CorposHidricos.Add(river);
        await db.SaveChangesAsync();
        riverId = river.Id;
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task EnviaHistoricoDoMesmoContextoEPersisteResposta()
    {
        var coleta = new ColetaEntity
        {
            CorpoHidricoId = riverId, DataHora = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Latitude = -23.5, Longitude = -46.6, ProfundidadeMetros = 2,
        };
        coleta.AdicionarMedicao(new MedicoesEntity(Medicao.Ph, 7, "pH", false, null));
        coleta.AdicionarMedicao(new MedicoesEntity(Medicao.OxigenioDissolvido, null, "mg/L", true, 0.1));
        db.Coletas.Add(coleta);
        await db.SaveChangesAsync();

        string? enviado = null;
        var handler = new Handler(async request =>
        {
            Assert.Equal("/previsao-geral", request.RequestUri!.AbsolutePath);
            enviado = await request.Content!.ReadAsStringAsync();
            var ponto = "-23.5,-46.600000000000001";
            var parametros = new Dictionary<string, object>();
            foreach (var nome in new[] { "temperatura", "ph", "condutividade_eletrica",
                         "oxigenio_dissolvido", "solidos_suspensos_totais",
                         "carbono_organico_total", "fosforo_total" })
                parametros[nome] = new { valor = nome == "ph" ? (double?)7 : null,
                    unidade = nome == "ph" ? "pH" : "mg/L", evidencia = "limitada" };
            var json = JsonSerializer.Serialize(new
            {
                data_referencia = "2026-01-01T12:00:00", data_alvo = "2026-01-08T12:00:00",
                horizonte_dias = 7, ponto, profundidade = 2, parametros,
            });
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var service = Service(handler, "previsao-geral");

        var result = await service.CriarAsync(new(riverId, 7, -23.5, -46.6, 2), default);

        Assert.True(result.Id > 0);
        Assert.Equal("2026-01-08", result.DataAlvo?.ToString("yyyy-MM-dd"));
        Assert.Single(await service.ListarAsync(riverId, 7, default));
        using var payload = JsonDocument.Parse(enviado!);
        var sample = payload.RootElement.GetProperty("coletas")[0];
        Assert.Equal(7, sample.GetProperty("ph").GetProperty("valor").GetDouble());
        Assert.Equal(JsonValueKind.Null, sample.GetProperty("oxigenio_dissolvido").ValueKind);
        Assert.Equal(2, sample.GetProperty("profundidade").GetDouble());
    }

    [Fact]
    public async Task SemRotaIaConfiguradaRetornaIndisponivelESemPersistir()
    {
        db.Coletas.Add(new ColetaEntity
        { CorpoHidricoId = riverId, DataHora = DateTimeOffset.UtcNow.AddDays(-1) });
        await db.SaveChangesAsync();
        var service = Service(new Handler(_ => throw new Exception("Não deveria chamar IA")), null);
        var error = await Assert.ThrowsAsync<IaException>(() => service.CriarAsync(new(riverId, 7), default));
        Assert.Equal(503, error.StatusCode);
        Assert.Empty(db.QualidadesFuturas);
    }

    private PrevisaoQualidadeFuturaService Service(HttpMessageHandler handler, string? route)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["IA:PredictGeralPath"] = route }).Build();
        return new PrevisaoQualidadeFuturaService(db,
            new HttpClient(handler) { BaseAddress = new Uri("http://ia.test/") }, config);
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => respond(request);
    }
}
