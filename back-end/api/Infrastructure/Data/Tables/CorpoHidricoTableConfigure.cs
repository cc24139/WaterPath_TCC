namespace Infrastructure.Data.Tables;

using back_end.src.Domain.CorpoHidrico;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CorpoHidricoTableConfigure : IEntityTypeConfiguration<CorpoHidricoEntity>
{
    public void Configure(EntityTypeBuilder<CorpoHidricoEntity> builder)
    {
        builder.ToTable("CorposHidricos", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired();
        builder.Property(e => e.Localizacao).IsRequired();
        builder.Property(e => e.Tamanho).IsRequired();
        builder.Property<bool>("EhPrivado").HasColumnName("EhPrivado").IsRequired();

        builder
            .HasMany(e => e.users)
            .WithMany(e => e.CorpoHidricos)
            .UsingEntity<Dictionary<string, object>>(
                "UsuarioCorpoHidrico",
                right =>
                    right
                        .HasOne<UserEntity>()
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left.HasOne<CorpoHidricoEntity>()
                        .WithMany()
                        .HasForeignKey("CorpoHidricoId")
                        .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("UsuarioCorpoHidricos", "waterPath");
                    join.HasKey("UsuarioId", "CorpoHidricoId");
                }
            );
    }
}
