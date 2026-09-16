// Contexto isolado: permite validar o módulo enquanto Codigo/Coleta impedem o build da API.
using back_end.src.Domain.Coleta;
using back_end.src.Medicoes.Domain;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data.Tables;

namespace Infrastructure.Data;

public class WaterPathDbContext(DbContextOptions<WaterPathDbContext> options) : DbContext(options)
{
    public DbSet<ColetaEntity> Coletas => Set<ColetaEntity>();
    public DbSet<MedicoesEntity> Medicoes => Set<MedicoesEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColetaEntity>().ToTable("Coletas", "waterPath");
        modelBuilder.ApplyConfiguration(new MedicoesTableConfigure());
    }
}
