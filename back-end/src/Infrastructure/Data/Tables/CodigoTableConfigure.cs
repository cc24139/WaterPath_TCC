namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Codigo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CodigoTableConfigure : IEntityTypeConfiguration<CodigoEntity>
{
    public void Configure(EntityTypeBuilder<CodigoEntity> builder)
    {
        builder.ToTable("Codigos", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Codigo).IsRequired().HasMaxLength(6);
        builder.Property(e => e.DataGeracao).IsRequired();
        builder.Property(e => e.DataExpiracao).IsRequired();
        builder.Property(e => e.Usado).IsRequired();
        builder.Property(e => e.UsuarioId).IsRequired();

        builder
            .HasOne(e => e.Usuario)
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
