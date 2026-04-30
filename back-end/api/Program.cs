using System.Reflection;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.Codigo;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Domain.Qualidade;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.Infrastructure.Repository;
using Domain.User;
using DotNetEnv;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


Env.Load();
var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Registro dos repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICianoBacteriaRepository, CianoBacteriaRepository>();
builder.Services.AddScoped<IColetaRepository, ColetaRepository>();
builder.Services.AddScoped<ICorpoHidricoRepository, CorpoHidricoRepository>();
builder.Services.AddScoped<IImagemRepository, ImagemRepository>();
builder.Services.AddScoped<IMetalPesadoRepository, MetalPesadoRepository>();
builder.Services.AddScoped<IQualidadeRepository, QualidadeRepository>();
builder.Services.AddScoped<IQualidadeFuturaRepository, QualidadeFuturaRepository>();
builder.Services.AddScoped<ICodigoRepository, CodigoRepository>();

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
