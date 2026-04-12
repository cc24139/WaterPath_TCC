using System.Reflection;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Domain.Qualidade;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.Infrastructure.Repository;
using Domain.User;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
);
builder.Services.AddDbContext<WaterPathDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Registro dos repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICianoBacteriaRepository, CianoBacteriaRepository>();
builder.Services.AddScoped<IColetaRepository, ColetaRepository>();
builder.Services.AddScoped<ICorpoHidricoRepository, CorpoHidricoRepository>();
builder.Services.AddScoped<IImagemRepository, ImagemRepository>();
builder.Services.AddScoped<IMetalPesadoRepository, MetalPesadoRepository>();
builder.Services.AddScoped<IQualidadeRepository, QualidadeRepository>();
builder.Services.AddScoped<IQualidadeFuturaRepository, QualidadeFuturaRepository>();

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
