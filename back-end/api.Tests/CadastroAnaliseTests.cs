using System.Security.Claims;
using System.Text.Json;
using Application.DTOs;
using Application.DTOs.Coleta;
using back_end.src.Controllers.Coleta;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using back_end.src.Medicoes.Application;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Domain.User;
using back_end.src.Authorization;
using Xunit;

namespace WaterPath.Api.Tests;

public class CadastroAnaliseTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;
    private int riverId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new WaterPathDbContext(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var river = new CorpoHidricoEntity("Rio privado", "Local de teste", 10, true);
        var user = new UserEntity("Pessoa de teste", "senha-teste", "teste@example.org");
        river.users.Add(user);
        db.Add(river);
        await db.SaveChangesAsync();
        riverId = river.Id;
    }
    public async Task DisposeAsync() { await db.DisposeAsync(); await connection.DisposeAsync(); }

    private ColetaCadastroInput Input(IReadOnlyCollection<MetalMedidoInput>? metals = null) => new(
        riverId, DateTimeOffset.Parse("2026-01-01T10:00:00-03:00"),
        Medicoes: [new("Temperatura", -1.5, "°C"), new("Ph", 7, "pH"), new("CondutividadeEletrica", 0, "µS/cm"), new("OxigenioDissolvido", 6, "mg/L")],
        MetaisPesados: metals);

    [Fact]
    public void JsonCadastroAceitaContratoComMetaisParciais()
    {
        var json = JsonSerializer.Serialize(Input([new("Fe", 0.123456789012, "mg/L"), new("Pb", 0, "µg/L")]), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var input = JsonSerializer.Deserialize<ColetaCadastroInput>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var collection = input!.ToEntity();
        Assert.Equal(4, collection.Medicoes.Count);
        Assert.Equal(2, collection.MetaisPesados.Count);
        Assert.Equal(0.123456789012, collection.MetaisPesados[0].Concentracao);
        Assert.Equal(0, collection.MetaisPesados[1].Concentracao);
        Assert.Equal(TimeSpan.Zero, collection.DataHora.Offset);
    }

    [Fact]
    public void MetaisAusentesPermanecemAusentes()
    {
        Assert.Empty(Input().ToEntity().MetaisPesados);
        Assert.Empty(Input([]).ToEntity().MetaisPesados);
    }

    [Theory]
    [InlineData("Hg", 1, "µg/L")]
    [InlineData("Fe", -1, "mg/L")]
    [InlineData("Fe", 1, "µg/L")]
    [InlineData("Pb", 1, "mg/L")]
    [InlineData("Fe", double.NaN, "mg/L")]
    [InlineData("Fe", double.PositiveInfinity, "mg/L")]
    public void MetaisInvalidosSaoRejeitadosAntesDaPersistencia(string name, double value, string unit)
    {
        Assert.Throws<ArgumentException>(() => Input([new(name, value, unit)]).ToEntity());
        Assert.Empty(db.Coletas);
    }

    [Fact]
    public void DuplicacaoValorAusenteEDataFuturaSaoRejeitados()
    {
        Assert.Throws<ArgumentException>(() => Input([new("Fe", 1, "mg/L"), new("Fe", 2, "mg/L")]).ToEntity());
        Assert.Throws<ArgumentException>(() => Input([new("Fe", null, "mg/L")]).ToEntity());
        Assert.Throws<ArgumentException>(() => new ColetaCadastroInput(riverId, DateTimeOffset.UtcNow.AddDays(1)).ToEntity());
        Assert.Throws<ArgumentException>(() => new ColetaCadastroInput(riverId, default).ToEntity());
        Assert.Throws<ArgumentException>(() => new ColetaCadastroInput(riverId, DateTimeOffset.UtcNow.AddMinutes(-1), Medicoes: [new("Ph", 15, "pH")]).ToEntity());
    }

    [Fact]
    public async Task CadastroPersisteMedicoesMetaisEResponsavelDoToken()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddScoped<IColetaRepository, ColetaRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ControllerColeta).Assembly));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var userId = await db.Usuarios.Select(u => u.Id).SingleAsync();
        var controller = new ControllerColeta(scope.ServiceProvider.GetRequiredService<IMediator>(), new ResourceAuthorizationService(db), db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Name, "Pessoa de teste")], "Test"));
        var key = Guid.NewGuid().ToString();
        var input = Input([new("Fe", 0.123456789012, "mg/L"), new("Pb", 0, "µg/L")]);
        var response = Assert.IsType<CreatedResult>(await controller.Cadastrar(input, CancellationToken.None, key));
        var created = Assert.IsType<ColetaEntity>(response.Value);
        var repeated = Assert.IsType<CreatedResult>(await controller.Cadastrar(input, CancellationToken.None, key));
        Assert.Equal(created.Id, Assert.IsType<ColetaEntity>(repeated.Value).Id);
        Assert.IsType<ConflictObjectResult>(await controller.Cadastrar(
            Input([new("Fe", 0.5, "mg/L")]), CancellationToken.None, key));
        Assert.Equal(1, await db.Coletas.CountAsync());
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Name, "Outra pessoa")], "Test"));
        Assert.IsType<ForbidResult>(await controller.Cadastrar(input, CancellationToken.None, key));
        Assert.IsType<ForbidResult>(await controller.Deletar(created.Id, CancellationToken.None));
        Assert.Equal(1, await db.Coletas.CountAsync());
        db.ChangeTracker.Clear();
        var saved = (await new ColetaRepository(db).ObterPorId(created.Id))!;
        Assert.Equal(userId, saved.ResponsavelId);
        Assert.Equal("Pessoa de teste", saved.ResponsavelNome);
        Assert.Equal(4, saved.Medicoes.Count);
        Assert.Equal(2, saved.MetaisPesados.Count);
        Assert.Equal(0.123456789012, saved.MetaisPesados.Single(m => m.Nome == "Fe").Concentracao);
        Assert.DoesNotContain("\"Coleta\"", JsonSerializer.Serialize(saved.MetaisPesados));
    }

    [Fact]
    public async Task UsuarioNaoVinculadoPodeCadastrarColetaEmRioPublicoMasNaoEmPrivado()
    {
        var outsider = new UserEntity("Outra pessoa", "senha-teste", "outro@example.org");
        var publicRiver = new CorpoHidricoEntity("Rio público", "Outro local", 10, false);
        db.AddRange(outsider, publicRiver);
        await db.SaveChangesAsync();
        Assert.False(CorpoHidricoDTO.FromEntity(publicRiver).EhPrivado);
        Assert.True(CorpoHidricoDTO.FromEntity(await db.CorposHidricos.SingleAsync(c => c.Id == riverId)).EhPrivado);
        var authorization = new ResourceAuthorizationService(db);
        Assert.True(await authorization.PodeIniciarAnaliseCorpoHidricoAsync(publicRiver.Id, outsider.Id, CancellationToken.None));
        Assert.False(await authorization.PodeIniciarAnaliseCorpoHidricoAsync(riverId, outsider.Id, CancellationToken.None));
        Assert.False(await authorization.PodeGerenciarCorpoHidricoAsync(publicRiver.Id, outsider.Id, CancellationToken.None));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddScoped<IColetaRepository, ColetaRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ControllerColeta).Assembly));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var controller = new ControllerColeta(scope.ServiceProvider.GetRequiredService<IMediator>(),
            authorization, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, outsider.Id.ToString()),
            new Claim(ClaimTypes.Name, outsider.Nome)], "Test"));

        Assert.IsType<ForbidResult>(await controller.Cadastrar(Input(), CancellationToken.None));
        var created = Assert.IsType<CreatedResult>(await controller.Cadastrar(
            Input() with { CorpoHidricoId = publicRiver.Id }, CancellationToken.None));
        Assert.Equal(outsider.Id, Assert.IsType<ColetaEntity>(created.Value).ResponsavelId);
        Assert.Equal(publicRiver.Id, Assert.IsType<ColetaEntity>(created.Value).CorpoHidricoId);
        Assert.True(await authorization.PodeAnalisarColetaAsync(
            Assert.IsType<ColetaEntity>(created.Value).Id, outsider.Id, CancellationToken.None));
    }

    [Fact]
    public async Task FalhaAoGravarMetalDesfazColetaEMedicoes()
    {
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER fail_metal BEFORE INSERT ON MetaisPesados BEGIN SELECT RAISE(ABORT, 'test failure'); END;");
        var collection = Input([new("Fe", 1, "mg/L")]).ToEntity();
        await Assert.ThrowsAsync<DbUpdateException>(() => new ColetaRepository(db).Cadastrar(collection, riverId));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Coletas.ToListAsync());
        Assert.Empty(await db.Medicoes.ToListAsync());
        Assert.Empty(await db.MetaisPesados.ToListAsync());
    }
}
