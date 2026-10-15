using back_end.src.Infrastructure.Repository;
using Domain.User;
using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WaterPath.Api.Tests;

public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WaterPathDbContext db = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new WaterPathDbContext(
            new DbContextOptionsBuilder<WaterPathDbContext>()
                .UseSqlite(connection)
                .Options);
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task AtualizarPreservaIdEAlteraCamposDaEntidadeExistente()
    {
        var persisted = new UserEntity("Nome original", "senha-original", "original@example.com");
        db.Usuarios.Add(persisted);
        await db.SaveChangesAsync();
        var persistedId = persisted.Id;
        db.ChangeTracker.Clear();

        var update = new UserEntity("Nome atualizado", "senha-atualizada", "atualizado@example.com");
        db.Entry(update).Property(nameof(UserEntity.Id)).CurrentValue = persistedId;
        db.Entry(update).State = EntityState.Detached;

        await new UserRepository(db).Atualizar(update);

        db.ChangeTracker.Clear();
        var saved = await db.Usuarios.SingleAsync();
        Assert.Equal(persistedId, saved.Id);
        Assert.Equal("Nome atualizado", saved.Nome);
        Assert.Equal("senha-atualizada", saved.Senha);
        Assert.Equal("atualizado@example.com", saved.Email);
    }
}
