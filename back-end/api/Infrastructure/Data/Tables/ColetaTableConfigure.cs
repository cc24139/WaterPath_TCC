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

        builder.Property(e => e.CorpoHidricoId).IsRequired();
        builder.Property(e => e.DataHora).IsRequired();
        builder.Property(e => e.Latitude);
        builder.Property(e => e.Longitude);
        builder.Property(e => e.ProfundidadeMetros);


        builder
            .HasOne(e => e.CorpoHidrico)
            .WithMany(e => e.Coletas)
            .HasForeignKey(e => e.CorpoHidricoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
