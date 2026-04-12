namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Imagem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImagemTableConfigure : IEntityTypeConfiguration<ImagemEntity>
{
    public void Configure(EntityTypeBuilder<ImagemEntity> builder)
    {
        builder.ToTable("Imagens", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Url).IsRequired();
        builder.Property(e => e.DataUpload).IsRequired();

        builder
            .HasOne(e => e.CorpoHidrico)
            .WithMany(e => e.Imagens)
            .HasForeignKey("CorpoHidricoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.Coleta)
            .WithMany(e => e.Imagens)
            .HasForeignKey("ColetaId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
