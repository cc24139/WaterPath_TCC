namespace Infrastructure.Data.Tables;

using back_end.src.Domain.CorpoHidrico;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserTableConfigure : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Usuarios", "waterPath");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired();
        builder.Property(e => e.Senha).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder
            .HasMany(e => e.CorpoHidricos)
            .WithMany(e => e.users)
            .UsingEntity<Dictionary<string, object>>(
                "UsuarioCorpoHidrico",
                right =>
                    right
                        .HasOne<CorpoHidricoEntity>()
                        .WithMany()
                        .HasForeignKey("CorpoHidricoId")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left.HasOne<UserEntity>()
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("UsuarioCorpoHidricos", "waterPath");
                    join.HasKey("UsuarioId", "CorpoHidricoId");
                }
            );
    }
}
