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

        builder.Property(e => e.Nome).IsRequired();
        builder.Property(e => e.Data).IsRequired();
        builder.Property(e => e.Ph).IsRequired();
        builder.Property(e => e.OxigenioDissolvido).IsRequired();
        builder.Property(e => e.Turbidez).IsRequired();
        builder.Property(e => e.CloroResidual).IsRequired();
        builder.Property(e => e.Floretos).IsRequired();
        builder.Property(e => e.ColiformesTotais).IsRequired();
        builder.Property(e => e.EscherichiaColi).IsRequired();

        builder
            .HasOne(e => e.CorpoHidrico)
            .WithMany()
            .HasForeignKey("CorpoHidricoId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
