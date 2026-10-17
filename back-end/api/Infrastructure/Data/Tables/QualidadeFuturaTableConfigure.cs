namespace Infrastructure.Data.Tables;

using back_end.src.Domain.QualidadeFutura;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class QualidadeFuturaTableConfigure : IEntityTypeConfiguration<QualidadeFuturaEntity>
{
    public void Configure(EntityTypeBuilder<QualidadeFuturaEntity> builder)
    {
        builder.ToTable("QualidadesFuturas", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ResultadoJson).HasColumnType("jsonb");
        builder.HasIndex(e => new { e.CorpoHidricoId, e.DataAlvo, e.CriadaEm });

        builder
            .HasOne(e => e.CorpoHidrico)
            .WithMany()
            .HasForeignKey(e => e.CorpoHidricoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
