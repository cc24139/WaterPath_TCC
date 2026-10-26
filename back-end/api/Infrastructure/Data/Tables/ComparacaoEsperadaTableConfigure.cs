using back_end.src.ValidacaoSintetica.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Tables;

public sealed class ComparacaoEsperadaTableConfigure : IEntityTypeConfiguration<ComparacaoEsperadaEntity>
{
    public void Configure(EntityTypeBuilder<ComparacaoEsperadaEntity> builder)
    {
        builder.ToTable("ComparacoesEsperadas", "waterPath");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AmostraId).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Parametro).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Unidade).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Origem).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ValorEsperado).IsRequired();
        builder.Property(x => x.ValorPrevisto);
        builder.Property(x => x.CriadoEm).IsRequired();
        builder.HasIndex(x => new { x.AmostraId, x.Parametro }).IsUnique();
        builder.HasIndex(x => x.ColetaId);
        builder.HasOne(x => x.Coleta).WithMany().HasForeignKey(x => x.ColetaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
