namespace Infrastructure.Data.Tables;

using back_end.src.Domain.CorpoHidrico;
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
    }
}
