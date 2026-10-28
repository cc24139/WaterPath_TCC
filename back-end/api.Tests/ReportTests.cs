using back_end.src.Relatorios;
using back_end.src.IA.Application;
using Xunit;
using back_end.src.Authorization;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA.Domain;
using back_end.src.Medicoes.Domain;
using Domain.User;
using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace WaterPath.Api.Tests;

public class ReportTests
{
    private sealed class ReportDb(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            model.Entity<ColetaEntity>().Property(c => c.DataHora).HasConversion(v => v.UtcDateTime,
                v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }
    private static async Task WithData(Func<WaterPathDbContext, ReportDataService, Task> test)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ReportDb(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var owner = new UserEntity("Responsável", "senha-teste", "report@example.invalid");
        var body = new CorpoHidricoEntity("Rio de teste", "Recife / PE", 20, true);
        body.users.Add(owner); db.CorposHidricos.Add(body); await db.SaveChangesAsync();
        foreach (var day in new[] { 1, 10, 20 })
        {
            var c = new ColetaEntity { CorpoHidricoId = body.Id, DataHora = DateTimeOffset.Parse($"2026-10-{day:00}T12:00:00Z") };
            c.RegistrarResponsavel(day == 10 ? 2 : owner.Id, "Responsável da coleta");
            foreach (var (code, value, unit) in new[] { (Medicao.Temperatura, 24.3, "°C"), (Medicao.Ph, 7.2, "pH"), (Medicao.CondutividadeEletrica, 312.0, "µS/cm"), (Medicao.OxigenioDissolvido, 6.5, "mg/L") })
                c.AdicionarMedicao(new(code, value, unit, false, null));
            db.Coletas.Add(c);
        }
        await db.SaveChangesAsync();
        var current = await db.Coletas.OrderByDescending(c => c.Id).FirstAsync();
        foreach (var level in new[] { 1, 2 }) db.PredicoesIA.Add(new PredicaoIAEntity
        {
            ColetaId = current.Id, CorpoHidricoId = body.Id, DataColeta = current.DataHora.UtcDateTime,
            CriadaEm = new DateTime(2026, 10, 21, 12, level, 0, DateTimeKind.Utc), Tipo = "integrada",
            EntradaJson = "{\"temperatura\":24.3,\"ph\":7.2,\"condutividade_eletrica\":312,\"oxigenio_dissolvido\":6.5}",
            ResultadoJson = $$"""{"riskLevel":{{level}},"riskReasons":["Sinal visual: resíduos."],"estimatedMetals":[{"name":"Fe","value":0.18,"unit":"mg/L"}],"detections":[{"className":"Lixo","count":2}]}""",
            NomeArquivo = "rio.png", ContentTypeOriginal = "image/png", ContentTypeResultado = "image/png",
            ImagemOriginal = [], ImagemResultado = [],
        });
        await db.SaveChangesAsync();
        await test(db, new ReportDataService(db, new ResourceAuthorizationService(db)));
    }
    internal static ReportConfig Config(string length = "medio") => new(new(2026, 10, 1), new(2026, 10, 28),
        "medio", length, new(true, true, true, true, true, false), new(true, true));

    [Fact]
    public void Etapa1ConfiguracaoValidaDatasSecoesETipos()
    {
        var today = new DateOnly(2026, 10, 28);
        Config().Validate(today);
        foreach (var config in new[] { Config() with { StartDate = default }, Config() with { EndDate = new(2026, 10, 29) },
            Config() with { StartDate = new(2026, 11, 1) }, Config() with { DetailLevel = "invalido" },
            Config() with { StartDate = new(2025, 1, 1) }, Config() with { IncludedSections = new(false, false, false, false, false, false) } })
            Assert.Throws<IaException>(() => config.Validate(today));
        Assert.Equal("2026-10-01T03:00:00+00:00", ReportTime.Start(new(2026, 10, 1)).ToString("yyyy-MM-ddTHH:mm:sszzz"));
        Assert.Equal(new DateOnly(2026, 10, 28), ReportTime.Today(DateTimeOffset.Parse("2026-10-29T01:00:00Z")));
    }

    [Fact]
    public Task Etapa2SelecionaUmaAnalisePorColetaEPreservaPendencias() => WithData(async (db, service) =>
    {
        var report = await service.BuildAsync(new(1, Config()), 1, "Emissor", default);
        Assert.Equal(3, report.Collections.Length);
        Assert.Equal(2, report.Collections[0].Risk!.Level);
        Assert.Equal(2, report.Collections[0].PredictionId);
        Assert.Equal(2, report.Collections.Count(c => c.PredictionId is null));
        Assert.Single(report.Collections[0].Metals);
        Assert.Equal("Estimado", report.Collections[0].Metals[0].Source);
        Assert.Empty(report.Images);
        Assert.False(db.ChangeTracker.HasChanges());
    });

    [Fact]
    public Task Etapa2RespeitaEscopoCalendarioEPermissoes() => WithData(async (_, service) =>
    {
        Assert.Equal(403, (await Assert.ThrowsAsync<IaException>(() => service.BuildAsync(new(1, Config()), 999, "Outro", default))).StatusCode);
        var own = await service.BuildAsync(new(1, Config()), 2, "Autor sem vínculo", default);
        Assert.True(own.RestrictedScope); Assert.Single(own.Collections);
        var oneDay = await service.BuildAsync(new(1, Config() with { StartDate = new(2026, 10, 10), EndDate = new(2026, 10, 10) }), 1, "Emissor", default);
        Assert.Single(oneDay.Collections);
        Assert.Equal(422, (await Assert.ThrowsAsync<IaException>(() => service.BuildAsync(new(1, Config() with { StartDate = new(2026, 10, 22) }), 1, "Emissor", default))).StatusCode);
    });

    [Fact]
    public Task Etapa2MedicaoEditadaNaoHerdaRiscoAntigo() => WithData(async (db, service) =>
    {
        var measurement = await db.Medicoes.OrderByDescending(m => m.Id).FirstAsync();
        measurement.Atualizar(measurement.codigoMedicao, 4, measurement.unidade, false, null);
        await db.SaveChangesAsync();
        var report = await service.BuildAsync(new(1, Config()), 1, "Emissor", default);
        Assert.Null(report.Collections[0].Risk);
        Assert.Contains(report.Notices, n => n.Contains("correspondência"));
    });

    [Fact]
    public Task Etapa2ReutilizaConversaoDeUnidadesDaAmostra() => WithData(async (db, service) =>
    {
        var collection = await db.Coletas.Include(c => c.Medicoes).OrderByDescending(c => c.Id).FirstAsync();
        collection.AdicionarMedicao(new(Medicao.FosforoTotal, 0.02, "mg/L", false, null));
        var prediction = await db.PredicoesIA.OrderByDescending(p => p.Id).FirstAsync();
        prediction.EntradaJson = AmostraDaColeta.Montar(collection);
        await db.SaveChangesAsync();
        var data = await service.BuildAsync(new(1, Config()), 1, "Emissor", default);
        Assert.NotNull(data.Collections[0].Risk);
        Assert.Equal(0.02, data.Collections[0].Measurements.Single(m => m.Code == 12).Value);
    });

    [Fact]
    public Task Etapa3CompoeSecoesSemOcultarAlertasNoModoCompacto() => WithData(async (_, service) =>
    {
        var config = Config("curto") with { IncludedSections = new(false, false, false, true, false, false) };
        var report = ReportComposer.Compose(await service.BuildAsync(new(1, config), 1, "Emissor", default));
        Assert.DoesNotContain(report.Blocks, b => b.Key is "summary" or "current" or "history" or "visual");
        Assert.Contains(report.Blocks, b => b.Key == "alerts" && b.Paragraphs.Any(p => p.Contains("resíduos")));
        Assert.Contains(report.Blocks, b => b.Key == "method");
        Assert.Contains(report.Blocks.Single(b => b.Key == "metals").Table!.Rows, r => r.Contains("Estimado"));
    });

    [Fact]
    public Task Etapa3DetalhamentoTecnicoAdicionaRastreabilidadeSemAlterarResultados() => WithData(async (_, service) =>
    {
        var data = await service.BuildAsync(new(1, Config()), 1, "Emissor", default);
        var basic = ReportComposer.Compose(data);
        var technical = ReportComposer.Compose(data with { Config = data.Config with { DetailLevel = "tecnico" } });
        Assert.Equal(basic.Blocks.Single(b => b.Key == "history").Table!.Rows, technical.Blocks.Single(b => b.Key == "history").Table!.Rows);
        Assert.Contains(technical.Blocks.Single(b => b.Key == "method").Paragraphs, p => p.Contains("Rastreabilidade"));
        Assert.True(technical.Blocks.Single(b => b.Key == "method").Paragraphs.Length > basic.Blocks.Single(b => b.Key == "method").Paragraphs.Length);
    });

    [Fact]
    public Task Etapa4GeraPdfComPaginacaoEmTodosOsModos() => WithData(async (_, service) =>
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var data = await service.BuildAsync(new(1, Config()), 1, "Emissor de teste", default);
        foreach (var length in new[] { "curto", "medio", "completo" })
        {
            var configured = data with { Config = data.Config with { DocumentLength = length, DetailLevel = "tecnico" } };
            if (length == "completo") configured = configured with { Collections = Enumerable.Range(0, 45)
                .Select(i => data.Collections[i % 3] with { Id = 100 + i }).ToArray() };
            var bytes = new ReportPdfDocument().Generate(ReportComposer.Compose(configured));
            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(bytes, 0, 5));
            Assert.True(bytes.Length > 3000);
        }
        var output = Environment.GetEnvironmentVariable("WATERPATH_REPORT_SAMPLE");
        if (!string.IsNullOrWhiteSpace(output))
        {
            var imagePath = Environment.GetEnvironmentVariable("WATERPATH_REPORT_IMAGE");
            if (File.Exists(imagePath)) data = data with
            {
                Images = [new(data.Collections[0].Id, 2, await File.ReadAllBytesAsync(imagePath), await File.ReadAllBytesAsync(imagePath))],
                Notices = data.Notices.Where(n => !n.Contains("imagens selecionadas")).ToArray(),
            };
            await File.WriteAllBytesAsync(output, new ReportPdfDocument().Generate(ReportComposer.Compose(data)));
        }
    });

    [Fact]
    public Task Etapa4GraficoPreservaLacunasCensuraEContextos() => WithData(async (_, service) =>
    {
        var data = await service.BuildAsync(new(1, Config()), 1, "Emissor", default);
        var rows = data.Collections.ToArray();
        rows[1] = rows[1] with { Measurements = [] };
        var chart = ReportPdfDocument.Chart(data with { Collections = rows }, 1)!;
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(chart, "<circle").Count);
        Assert.DoesNotContain("stroke-width='2'", chart);
        rows[2] = rows[2] with { Depth = 5 };
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(ReportPdfDocument.Chart(data with { Collections = rows }, 1)!, "<circle"));
    });

    [Fact]
    public Task Etapa4EndpointAutenticadoRetornaPdfERejeitaConfiguracaoInvalida() => WithData(async (db, service) =>
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
        builder.Services.AddSingleton(db); builder.Services.AddSingleton(service);
        builder.Services.AddSingleton<ReportPdfDocument>(); TestJwt.Configure(builder.Services);
        builder.Services.AddControllers().AddApplicationPart(typeof(ControllerRelatorios).Assembly);
        await using var app = builder.Build();
        app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var api = new HttpClient { BaseAddress = new(address) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.PostAsJsonAsync("/api/relatorios/pdf", new ReportRequest(1, Config()))).StatusCode);
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Token(999));
        Assert.Equal(HttpStatusCode.Forbidden, (await api.PostAsJsonAsync("/api/relatorios/pdf", new ReportRequest(1, Config()))).StatusCode);
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Token());
        Assert.Equal(HttpStatusCode.BadRequest, (await api.PostAsJsonAsync("/api/relatorios/pdf", new ReportRequest(1, Config() with { DocumentLength = "invalido" }))).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await api.PostAsJsonAsync("/api/relatorios/pdf", new ReportRequest(1, Config() with { StartDate = new(2026, 10, 22) }))).StatusCode);
        var response = await api.PostAsJsonAsync("/api/relatorios/pdf", new ReportRequest(1, Config()));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.Contains(".pdf", response.Content.Headers.ContentDisposition!.FileNameStar);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString((await response.Content.ReadAsByteArrayAsync())[..5]));
        Assert.False(db.ChangeTracker.HasChanges());
    });
}
