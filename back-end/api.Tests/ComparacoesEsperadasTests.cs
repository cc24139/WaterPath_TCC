using back_end.src.Authorization;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.ValidacaoSintetica;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace WaterPath.Api.Tests;

public sealed class ComparacoesEsperadasTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;
    private int coletaId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new ComparacoesTestDbContext(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var river = new CorpoHidricoEntity("Rio de teste", "Local", 1, false);
        db.CorposHidricos.Add(river);
        await db.SaveChangesAsync();
        var collection = new ColetaEntity { CorpoHidricoId = river.Id, DataHora = DateTimeOffset.UtcNow };
        collection.RegistrarResponsavel(TestJwt.UserId, "Usuário de teste");
        db.Coletas.Add(collection);
        await db.SaveChangesAsync();
        coletaId = collection.Id;
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    private ComparacoesEsperadasController Controller()
    {
        var controller = new ComparacoesEsperadasController(db, new ResourceAuthorizationService(db));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = TestJwt.Principal() },
        };
        return controller;
    }

    private sealed class ComparacoesTestDbContext(DbContextOptions<WaterPathDbContext> options)
        : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ColetaEntity>().Property(c => c.DataHora)
                .HasConversion(v => v.UtcDateTime,
                    v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }

    private ComparacaoEsperadaInput Input() => new()
    {
        ColetaId = coletaId, AmostraId = "sintetico-2025-06-24", Parametro = "Fe",
        ValorEsperado = .12, Unidade = "mg/L", Origem = "script/sinteticos.csv", Sintetico = true,
    };

    [Fact]
    public async Task PersisteValorEsperadoSeparadoEPrevistoPermaneceAusente()
    {
        var result = await Controller().Criar(Input(), CancellationToken.None);
        Assert.IsType<CreatedAtActionResult>(result);
        var saved = await db.ComparacoesEsperadas.SingleAsync();
        Assert.Equal(.12, saved.ValorEsperado);
        Assert.Null(saved.ValorPrevisto);
        Assert.Equal(coletaId, saved.ColetaId);
    }

    [Fact]
    public async Task RepeticaoIdenticaEIdempotenteMasValorDivergenteConflita()
    {
        var controller = Controller();
        Assert.IsType<CreatedAtActionResult>(await controller.Criar(Input(), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.Criar(Input(), CancellationToken.None));
        var changed = Input();
        changed.ValorEsperado = .13;
        Assert.IsType<ConflictObjectResult>(await controller.Criar(changed, CancellationToken.None));
        Assert.Single(await db.ComparacoesEsperadas.ToListAsync());
    }

    [Fact]
    public async Task IdentificadorComEspacosUsaChaveNormalizadaEValorNegativoERejeitado()
    {
        var controller = Controller();
        var input = Input();
        input.AmostraId = "  sintetico-2025-06-24  ";
        Assert.IsType<CreatedAtActionResult>(await controller.Criar(input, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.Criar(Input(), CancellationToken.None));
        var negative = Input();
        negative.ValorEsperado = -.1;
        Assert.IsType<BadRequestObjectResult>(await controller.Criar(negative, CancellationToken.None));
        Assert.Single(await db.ComparacoesEsperadas.ToListAsync());
    }

    [Theory]
    [InlineData("Iqa", "pontos")]
    [InlineData("Fe", "µg/L")]
    [InlineData("PREDICAO", "mg/L")]
    public async Task RejeitaParametroQueNaoEAlvoMetalOuUnidadeIncorreta(string parameter, string unit)
    {
        var input = Input();
        input.Parametro = parameter;
        input.Unidade = unit;
        Assert.IsType<BadRequestObjectResult>(await Controller().Criar(input, CancellationToken.None));
        Assert.Empty(await db.ComparacoesEsperadas.ToListAsync());
    }
}
