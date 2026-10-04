using back_end.src.IA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Tables;

public class PredicaoIATableConfigure : IEntityTypeConfiguration<PredicaoIAEntity>
{
    public void Configure(EntityTypeBuilder<PredicaoIAEntity> builder)
    {
        builder.ToTable("PredicoesIA", "waterPath");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Tipo).HasMaxLength(20).IsRequired();
        builder.Property(p => p.EntradaJson).HasColumnType("jsonb");
        builder.Property(p => p.ResultadoJson).HasColumnType("jsonb");
        builder.Property(p => p.NomeArquivo).IsRequired();
        builder.Property(p => p.ContentTypeOriginal).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ContentTypeResultado).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ImagemOriginal).IsRequired();
        builder.Property(p => p.ImagemResultado).IsRequired();
        builder.HasOne(p => p.Coleta).WithMany().HasForeignKey(p => p.ColetaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => new { p.ColetaId, p.CriadaEm });
    }
}
