using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Handler.CorpoHidrico;
using Application.Queries.CorpoHidrico;
using back_end.src.Controllers.CorpoHidrico;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA.Application;
using back_end.src.IA.Domain;
using back_end.src.Infrastructure.Repository;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace WaterPath.Api.Tests;

public class RiscoAtualTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;
    private int corpoId;
    private readonly DateTimeOffset date = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new TestDbContext(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var corpo = new CorpoHidricoEntity("Lago de teste", "Local de teste", 10, false);
        db.CorposHidricos.Add(corpo);
        await db.SaveChangesAsync();
        corpoId = corpo.Id;
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    private Task<Application.DTOs.RiscoAtualDTO?> Query(int? id = null) =>
        new ObterRiscoAtualHandler(new CorpoHidricoRepository(db))
            .Handle(new QueryObterRiscoAtual(id ?? corpoId), default);

    private static string Risk(int level = 2, string[]? reasons = null) => JsonSerializer.Serialize(new
    {
        riskLevel = level, baseRiskLevel = level,
        riskLabel = level == 1 ? "baixo" : level == 2 ? "moderado" : "alto",
        riskReasons = reasons ?? ["Sinal visual: Lixo."],
        riskRuleVersion = PredicaoIAService.VersaoRegraRisco, history = new { adjustment = 0 },
    });

    private async Task<ColetaEntity> Collection(DateTimeOffset at, int? body = null)
    {
        var coleta = new ColetaEntity { CorpoHidricoId = body ?? corpoId, DataHora = at };
        db.Coletas.Add(coleta);
        await db.SaveChangesAsync();
        return coleta;
    }

    private async Task<PredicaoIAEntity> Prediction(ColetaEntity coleta, DateTimeOffset at,
        string? json, string type = "integrada")
    {
        var predicao = new PredicaoIAEntity
        {
            ColetaId = coleta.Id, CriadaEm = at.UtcDateTime, Tipo = type, ResultadoJson = json,
            NomeArquivo = "teste.jpg", ContentTypeOriginal = "image/jpeg", ImagemOriginal = [1],
            ContentTypeResultado = "image/jpeg", ImagemResultado = [2],
        };
        db.PredicoesIA.Add(predicao);
        await db.SaveChangesAsync();
        return predicao;
    }

    [Fact]
    public async Task ColetaMaisRecentePrevaleceSobreReanaliseDeColetaAntiga()
    {
        var antiga = await Collection(date.AddDays(-1));
        var atual = await Collection(date);
        await Prediction(antiga, date.AddDays(1), Risk(3));
        var expected = await Prediction(atual, date, Risk());
        var otherBody = new CorpoHidricoEntity("Outro lago", "Outro local", 10, false);
        db.CorposHidricos.Add(otherBody);
        await db.SaveChangesAsync();
        await Prediction(await Collection(date.AddDays(2), otherBody.Id), date.AddDays(2), Risk(3));
        // A coleta mais nova sem predição não descarta o último resultado válido.
        await Collection(date.AddDays(3));
        var result = await Query();
        Assert.NotNull(result);
        Assert.Equal(corpoId, result.CorpoHidricoId);
        Assert.Equal(expected.Id, result.PredicaoId);
        Assert.Equal(atual.Id, result.ColetaId);
        Assert.Equal(date, result.DataColeta);
        Assert.Equal(expected.CriadaEm, result.DataPredicao);
        Assert.Equal(2, result.NivelRisco);
        Assert.Equal("moderado", result.RotuloRisco);
        Assert.Equal(new[] { "Sinal visual: Lixo." }, result.Motivos);
    }

    [Fact]
    public async Task EmpatesUsamIdColetaDepoisDataEIdPredicao()
    {
        var first = await Collection(date);
        var second = await Collection(date);
        await Prediction(first, date.AddDays(2), Risk(3));
        await Prediction(second, date.AddMinutes(-1), Risk(1));
        await Prediction(second, date, Risk(2));
        var expected = await Prediction(second, date, Risk(3));
        Assert.Equal(expected.Id, (await Query())!.PredicaoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("malformado")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{\"riskLevel\":0}")]
    [InlineData("{\"riskLevel\":4}")]
    [InlineData("{\"riskLevel\":\"2\"}")]
    public async Task ResultadoInvalidoNaoOcultaResultadoValidoAnterior(string? invalid)
    {
        var older = await Collection(date.AddDays(-1));
        var expected = await Prediction(older, date, Risk());
        await Prediction(await Collection(date), date, invalid);
        Assert.Equal(expected.Id, (await Query())!.PredicaoId);
    }

    [Theory]
    [InlineData("riskRuleVersion", "\"desconhecida\"")]
    [InlineData("riskLevel", "true")]
    [InlineData("riskLevel", "4")]
    [InlineData("baseRiskLevel", "0")]
    [InlineData("riskReasons", "[1]")]
    [InlineData("riskReasons", "null")]
    [InlineData("history", "[]")]
    public async Task ContratoIncompativelNaoViraRiscoBaixo(string field, string value)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Risk())!;
        data[field] = JsonSerializer.Deserialize<JsonElement>(value);
        await Prediction(await Collection(date), date, JsonSerializer.Serialize(data));
        Assert.Null(await Query());
    }

    [Fact]
    public async Task AnaliseSomenteVisualNaoAtribuiRiscoMesmoSeJsonContiverNivel()
    {
        await Prediction(await Collection(date), date, Risk(), "vision");
        Assert.Null(await Query());
    }

    [Fact]
    public async Task RiscoBaixoEMotivosVaziosPersistidosSaoPreservadosSemGravar()
    {
        var expected = await Prediction(await Collection(date), date, Risk(1, []));
        db.ChangeTracker.Clear();
        var result = await Query();
        Assert.Equal(1, result!.NivelRisco);
        Assert.Empty(result.Motivos);
        Assert.Equal(expected.Id, result.PredicaoId);
        Assert.Equal(1, await db.PredicoesIA.CountAsync());
        Assert.False(db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData("0", 400, "ID")]
    [InlineData("-1", 400, "ID")]
    [InlineData("abc", 400, "id")]
    [InlineData("2147483648", 400, "id")]
    [InlineData("999999", 404, "Corpo hídrico não encontrado")]
    [InlineData("1", 404, "sem resultado de risco válido")]
    public async Task HttpDistingueIdInvalidoInexistenteESemRisco(string id, int status, string message)
    {
        await using var host = await HttpHost();
        using var client = Client(host);
        var response = await client.GetAsync($"/api/corpohidrico/{id}/risco-atual");
        Assert.Equal(status, (int)response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        // Erros do controller seguem o padrão text/plain; erros de binding usam ProblemDetails.
        Assert.Contains(message, text);
    }

    [Fact]
    public async Task HttpExigeJwtERetornaDtoSemDependerDoClienteIa()
    {
        var expected = await Prediction(await Collection(date), date, Risk());
        await using var host = await HttpHost();
        using var client = Client(host);
        client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/corpohidrico/{corpoId}/risco-atual")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Token());
        var response = await client.GetAsync($"/api/corpohidrico/{corpoId}/risco-atual");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(corpoId, json.RootElement.GetProperty("corpoHidricoId").GetInt32());
        Assert.Equal(expected.Id, json.RootElement.GetProperty("predicaoId").GetInt32());
        Assert.Equal("Sinal visual: Lixo.", json.RootElement.GetProperty("motivos")[0].GetString());
        Assert.Equal(1, await db.PredicoesIA.CountAsync());
    }

    private async Task<WebApplication> HttpHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(db);
        builder.Services.AddScoped<ICorpoHidricoRepository, CorpoHidricoRepository>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ControllerCorpoHidrico).Assembly));
        builder.Services.AddControllers().AddApplicationPart(typeof(ControllerCorpoHidrico).Assembly);
        TestJwt.Configure(builder.Services);
        // Nenhum IaClient é registrado: consultar risco não precisa da IA.
        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private static HttpClient Client(WebApplication host)
    {
        var address = host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var client = new HttpClient { BaseAddress = new Uri(address) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Token());
        return client;
    }

    private sealed class TestDbContext(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ColetaEntity>().Property(c => c.DataHora)
                .HasConversion(v => v.UtcDateTime, v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }
}

internal static class TestJwt
{
    private static readonly SymmetricSecurityKey Key = new(Encoding.ASCII.GetBytes("waterpath-chave-apenas-para-testes-jwt-123456789"));

    internal static void Configure(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = Key,
                ValidateIssuer = false, ValidateAudience = false,
            });
    }

    internal static string Token() => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)));
}
