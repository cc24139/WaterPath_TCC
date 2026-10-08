using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using back_end.src.Controllers.CorpoHidrico;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using back_end.src.Medicoes.Domain;
using Domain.User;
using Infrastructure.Data;
using Infrastructure.Data.Seeding;
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
using Xunit.Abstractions;

namespace WaterPath.Api.Tests;

public class RiverCollectionHttpTests(ITestOutputHelper output)
{
    private sealed class HttpDb(DbContextOptions<WaterPathDbContext> options) : WaterPathDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            model.Entity<ColetaEntity>().Property(c => c.DataHora).HasConversion(
                v => v.UtcDateTime, v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }

    [Fact]
    public async Task GeneratedCollectionRunsAgainstRealControllersAndReportsContractDivergences()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "back-end/api/testing/run-collection.cjs")))
            root = root.Parent;
        Assert.NotNull(root);
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new HttpDb(new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var password = Guid.NewGuid().ToString("N");
        await RiverSeeder.SeedAsync(db, password);
        db.ChangeTracker.Clear();
        var jwtKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var oldKey = Environment.GetEnvironmentVariable("JWT_KEY");
        Environment.SetEnvironmentVariable("JWT_KEY", jwtKey);
        var report = Path.GetTempFileName();
        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();
            // Como em produção: um contexto novo por requisição; conexão SQLite compartilhada,
            // sem compartilhar o rastreamento EF entre cadastro, exclusão e consultas.
            builder.Services.AddScoped<WaterPathDbContext>(_ => new HttpDb(
                new DbContextOptionsBuilder<WaterPathDbContext>().UseSqlite(connection).Options));
            builder.Services.AddScoped<ICorpoHidricoRepository, CorpoHidricoRepository>();
            builder.Services.AddScoped<IColetaRepository, ColetaRepository>();
            builder.Services.AddScoped<IMedicoesRepository, MedicoesRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ControllerCorpoHidrico).Assembly));
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                    ValidateIssuer = false, ValidateAudience = false
                });
            builder.Services.AddControllers().AddApplicationPart(typeof(ControllerCorpoHidrico).Assembly)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
            await using var app = builder.Build();
            app.Use(async (context, next) =>
            {
                try { await next(context); }
                catch (Exception ex)
                {
                    // Somente tipo/local, sem SQL, corpos de requisição ou configuração.
                    output.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}: {ex.GetType().Name}; {ex.TargetSite?.DeclaringType?.Name}; inner={ex.InnerException?.GetType().Name}");
                    throw;
                }
            });
            app.MapControllers();
            await app.StartAsync();
            var url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            var start = new ProcessStartInfo("node") { RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = root!.FullName };
            start.ArgumentList.Add("back-end/api/testing/run-collection.cjs");
            start.Environment["WATERPATH_TEST_BASE_URL"] = url;
            start.Environment["WATERPATH_SEED_PASSWORD"] = password;
            start.Environment["WATERPATH_TEST_CONFIRM_DEV_DATABASE"] = "true";
            start.Environment["WATERPATH_TEST_REPORT"] = report;
            using var process = Process.Start(start)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch { process.Kill(entireProcessTree: true); throw; }
            output.WriteLine(await stdout);
            output.WriteLine(await stderr);
            using var result = JsonDocument.Parse(await File.ReadAllTextAsync(report));
            var failures = result.RootElement.GetProperty("failures").EnumerateArray().ToArray();
            // O verificador deve continuar e limpar os próprios dados mesmo após a falha do PUT.
            Assert.Equal(65, result.RootElement.GetProperty("requests").GetArrayLength());
            Assert.True(result.RootElement.GetProperty("assertions").GetInt32() > 100);
            Assert.Equal(6, failures.Length);
            Assert.All(failures, f => Assert.Contains(f.GetProperty("request").GetString(),
                new[] { "req_rios_update_river", "req_rios_register_missing", "req_rios_seed_name",
                    "req_rios_missing_name", "req_rios_seed_period", "req_rios_all_measures" }));
            Assert.Contains(failures, f => f.GetProperty("request").GetString() == "req_rios_update_river");
            Assert.Equal(1, process.ExitCode);
            db.ChangeTracker.Clear();
            Assert.Equal(6, await db.CorposHidricos.CountAsync());
            Assert.Equal(31, await db.Coletas.CountAsync());
            Assert.Equal(248, await db.Medicoes.CountAsync());
            Assert.Equal(30, await db.PredicoesIA.CountAsync());
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_KEY", oldKey);
            File.Delete(report);
        }
    }
}
