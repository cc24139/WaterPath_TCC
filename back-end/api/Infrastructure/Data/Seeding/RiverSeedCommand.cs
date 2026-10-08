using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Data.Seeding;

public static class RiverSeedCommand
{
    public static async Task<int> RunAsync(IConfiguration config, IHostEnvironment environment, bool checkOnly = false)
    {
        try
        {
            if (environment.EnvironmentName is not ("Development" or "Testing" or "Test"))
                throw new InvalidOperationException("População permitida somente em Development, Testing ou Test.");
            var connection = new NpgsqlConnectionStringBuilder(config["ConnectionString"]
                ?? throw new InvalidOperationException("Configure ConnectionString fora do repositório."));
            var target = $"{connection.Host}/{connection.Database}";
            if (!checkOnly && config["WATERPATH_SEED_TARGET"] != target)
                throw new InvalidOperationException("Identifique o banco de desenvolvimento/testes em WATERPATH_SEED_TARGET (host/database). Nenhuma gravação realizada.");
            var password = config["WATERPATH_SEED_PASSWORD"];
            if (!checkOnly && (string.IsNullOrWhiteSpace(password) || password.Length < 12))
                throw new InvalidOperationException("Configure WATERPATH_SEED_PASSWORD com pelo menos 12 caracteres. Não grave a senha em arquivos versionados.");
            var options = new DbContextOptionsBuilder<WaterPathDbContext>().UseNpgsql(connection.ConnectionString).Options;
            await using var db = new WaterPathDbContext(options);
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
            if (checkOnly)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { Target = target, Environment = environment.EnvironmentName,
                    PendingMigrations = pending, ReadOnly = true }, new JsonSerializerOptions { WriteIndented = true }));
                return 0;
            }
            if (pending.Length > 0)
                throw new InvalidOperationException($"Migrations pendentes: {string.Join(", ", pending)}. A rotina não altera o esquema; revise as migrations separadamente.");
            var result = await RiverSeeder.SeedAsync(db, password!);
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception ex)
        {
            // Mensagens de provedores podem conter a conexão; imprimir somente erros controlados.
            Console.Error.WriteLine(ex is InvalidOperationException or ArgumentException
                ? ex.Message : $"Comando interrompido ({ex.GetType().Name}); nenhuma transação de população confirmada. Verifique conexão/permissões sem divulgar credenciais.");
            return 1;
        }
    }
}
