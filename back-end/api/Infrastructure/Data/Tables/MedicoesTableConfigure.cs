namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Coleta;
using back_end.src.Medicoes.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MedicoesTableConfigure : IEntityTypeConfiguration<MedicoesEntity>
{
    public void Configure(EntityTypeBuilder<MedicoesEntity> builder)
    {
        builder.ToTable("Medicoes", "waterPath");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.codigoMedicao).HasConversion<int>().IsRequired();
        builder.Property(e => e.valor).IsRequired(false);
        builder.Property(e => e.unidade).IsRequired();
        builder.Property(e => e.censurado).IsRequired();
        builder.Property(e => e.limite).IsRequired(false);
        builder.HasOne(e => e.Coleta)
            .WithMany(e => e.Medicoes)
            .HasForeignKey(e => e.ColetaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.ColetaId, e.codigoMedicao })
            .IsUnique()
            .HasDatabaseName("IX_Medicoes_ColetaId_codigoMedicao");
    }
}
