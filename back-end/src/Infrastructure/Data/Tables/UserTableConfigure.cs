namespace Infrastructure.Data.Tables;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserTableConfigure : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Usuarios", "waterPath");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired();
        builder.Property(e => e.Senha).IsRequired();
        builder.Property(e => e.Email).IsRequired();
    }
}