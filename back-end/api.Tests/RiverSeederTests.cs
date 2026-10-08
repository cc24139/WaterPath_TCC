using Application.Handler.CorpoHidrico;
using Application.Queries.CorpoHidrico;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using Infrastructure.Data;
using Infrastructure.Data.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WaterPath.Api.Tests;

public class RiverSeederTests
{
    private sealed class SeedDb(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            model.Entity<ColetaEntity>().Property(c => c.DataHora).HasConversion(
                v => v.UtcDateTime, v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }

    [Fact]
    public async Task SeedIsIdempotentPreservesExistingDataAndProvidesRiskScenarios()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options;
        await using var db = new SeedDb(options);
        await db.Database.EnsureCreatedAsync();
        var existing = new CorpoHidricoEntity("Rio preexistente", "Local original", 42, false);
        db.Add(existing);
        await db.SaveChangesAsync();
        var password = Guid.NewGuid().ToString("N");
        var first = await RiverSeeder.SeedAsync(db, password);
        Assert.Equal(new SeedCounts(2, 6, 9, 31, 248, 30), first.Inseridos);
        db.ChangeTracker.Clear();
        var second = await RiverSeeder.SeedAsync(db, password);
        Assert.Equal(new SeedCounts(0, 0, 0, 0, 0, 0), second.Inseridos);
        Assert.Equal(first.Rios.Select(r => r.Id), second.Rios.Select(r => r.Id));
        Assert.Equal(7, await db.CorposHidricos.CountAsync());
        Assert.Equal(248, await db.Medicoes.CountAsync());
        Assert.Equal("Local original", (await db.CorposHidricos.FindAsync(existing.Id))!.Localizacao);
        var handler = new ObterRiscoAtualHandler(new CorpoHidricoRepository(db));
        foreach (var (index, risk) in new[] { (0, 1), (1, 3), (2, 3), (3, 2), (4, 1) })
        {
            var result = await handler.Handle(new QueryObterRiscoAtual(first.Rios[index].Id), default);
            Assert.NotNull(result);
            Assert.Equal(risk, result.NivelRisco);
            Assert.Contains("SINTETICO", result.Motivos.Single());
            Assert.Equal(first.Rios[index].ColetaIds.Last(), result.ColetaId);
        }
        Assert.Null(await handler.Handle(new QueryObterRiscoAtual(first.Rios[5].Id), default));
        Assert.All(await db.Usuarios.ToArrayAsync(), u => Assert.True(BCrypt.Net.BCrypt.Verify(password, u.Senha)));
        Assert.Equal(31, await db.Medicoes.CountAsync(m => m.censurado && m.valor == null && m.limite > 0));
    }

    [Fact]
    public async Task CollisionRollsBackAllInserts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new SeedDb(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new CorpoHidricoEntity("[SINTETICO v1] Rio Horizonte", "Não é a fixture", 1, false));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => RiverSeeder.SeedAsync(db, Guid.NewGuid().ToString("N")));
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.CorposHidricos.CountAsync());
        Assert.Equal(0, await db.Usuarios.CountAsync());
        Assert.Equal(0, await db.Coletas.CountAsync());
        Assert.Equal(0, await db.PredicoesIA.CountAsync());
    }
}
