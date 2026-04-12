namespace Infrastructure.Data.Tables;

using back_end.src.Domain.MetalPesado;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MetalPesadoTableConfigure : IEntityTypeConfiguration<MetalPesadoEntity>
{
    public void Configure(EntityTypeBuilder<MetalPesadoEntity> builder)
    {
        builder.ToTable("MetaisPesados", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired();
        builder.Property(e => e.Concentracao).IsRequired();
        builder.Property(e => e.Unidade).IsRequired();

        builder
            .HasOne(e => e.Coleta)
            .WithMany(e => e.MetaisPesados)
            .HasForeignKey("ColetaId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
