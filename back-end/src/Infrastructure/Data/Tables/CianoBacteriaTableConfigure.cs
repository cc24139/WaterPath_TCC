namespace Infrastructure.Data.Tables;

using back_end.src.Domain.CianoBacteria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CianoBacteriaTableConfigure : IEntityTypeConfiguration<CianoBacteriaEntity>
{
    public void Configure(EntityTypeBuilder<CianoBacteriaEntity> builder)
    {
        builder.ToTable("CianoBacterias", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Tipo).IsRequired();
        builder.Property(e => e.Concentracao).IsRequired();
        builder.Property(e => e.Unidade).IsRequired();

        builder
            .HasOne(e => e.Coleta)
            .WithMany(e => e.CianoBacterias)
            .HasForeignKey("ColetaId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
