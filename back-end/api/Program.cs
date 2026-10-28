using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Domain.Qualidade;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.Infrastructure.Repository;
using back_end.src.Medicoes.Domain;
using Infrastructure.Data;
using Domain.User;
using DotNetEnv;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using back_end.src.IA.Application;
using back_end.src.Authorization;
using back_end.src.QualidadeFutura.Application;
using Infrastructure.Data.Seeding;

// Variáveis explícitas do processo prevalecem sobre o .env local (inclusive na população).
Env.NoClobber().Load();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();

// Comando explícito: não inicia HTTP, JWT, e-mail ou inferência da IA.
if (args.Contains("--seed-rivers") || args.Contains("--seed-rivers-check"))
{
    Environment.ExitCode = await RiverSeedCommand.RunAsync(builder.Configuration, builder.Environment,
        checkOnly: args.Contains("--seed-rivers-check"));
    return;
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
);
var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
builder.Services.AddDbContext<WaterPathDbContext>(optins => optins.UseNpgsql(connectionString));

//Token
var key = Environment.GetEnvironmentVariable("JWT_KEY");
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
builder.Services.AddAuthorization();

// Registro dos repositórios
builder.Services.AddScoped<IMedicoesRepository, MedicoesRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICianoBacteriaRepository, CianoBacteriaRepository>();
builder.Services.AddScoped<IColetaRepository, ColetaRepository>();
builder.Services.AddScoped<ICorpoHidricoRepository, CorpoHidricoRepository>();
builder.Services.AddScoped<IImagemRepository, ImagemRepository>();
builder.Services.AddScoped<IMetalPesadoRepository, MetalPesadoRepository>();
builder.Services.AddScoped<IQualidadeRepository, QualidadeRepository>();
builder.Services.AddScoped<IQualidadeFuturaRepository, QualidadeFuturaRepository>();
builder.Services.AddScoped<PredicaoIAService>();
builder.Services.AddScoped<ResourceAuthorizationService>();
builder.Services.AddScoped<back_end.src.Relatorios.ReportDataService>();
builder.Services.AddSingleton<back_end.src.Relatorios.ReportPdfDocument>();
// Projeto acadêmico: revisar a licença se o contexto de uso mudar.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var configuredIaUrl = builder.Configuration["IA:BaseUrl"];
if (!Uri.TryCreate(configuredIaUrl, UriKind.Absolute, out var iaUrl)
    || (iaUrl.Scheme != Uri.UriSchemeHttp && iaUrl.Scheme != Uri.UriSchemeHttps))
    throw new InvalidOperationException(
        "Configure IA__BaseUrl no ambiente do servidor com uma URL HTTP(S) válida.");
var iaTimeoutSeconds = builder.Configuration.GetValue("IA:TimeoutSeconds", 120);
if (iaTimeoutSeconds is < 1 or > 300)
    throw new InvalidOperationException("IA__TimeoutSeconds deve estar entre 1 e 300 segundos.");
builder.Services.AddHttpClient<IaClient>(client =>
{
    client.BaseAddress = new Uri(iaUrl.ToString().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(iaTimeoutSeconds);
});
builder.Services.AddHttpClient<PrevisaoQualidadeFuturaService>(client =>
{
    client.BaseAddress = new Uri(iaUrl.ToString().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(iaTimeoutSeconds);
});

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                .WithExposedHeaders("X-Predicao-Id", "Location");
        }
    );
});

builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
