namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ColetaTableConfigure : IEntityTypeConfiguration<ColetaEntity>
{
    public void Configure(EntityTypeBuilder<ColetaEntity> builder)
    {
        builder.ToTable("Coletas", "waterPath");
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
            .WithMany(e => e.Coletas)
            .HasForeignKey("CorpoHidricoId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
