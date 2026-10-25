using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

// Permite executar migrations sem inicializar os serviços HTTP, JWT ou IA.
public sealed class WaterPathDbContextFactory : IDesignTimeDbContextFactory<WaterPathDbContext>
{
    public WaterPathDbContext CreateDbContext(string[] args)
    {
        Env.NoClobber().Load();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure ConnectionString no ambiente ou no .env do diretório atual para executar migrations.");

        var options = new DbContextOptionsBuilder<WaterPathDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new WaterPathDbContext(options);
    }
}
