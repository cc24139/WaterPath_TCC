namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Coleta;
using back_end.src.Medicoes.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MedicoesTableConfigure : IEntityTypeConfiguration<MedicoesEntity>
{
    public void Configure(EntityTypeBuilder<MedicoesEntity> builder)
    {
        builder.ToTable("Medicoes", "waterPath", table => table.HasCheckConstraint(
            "CK_Medicoes_ExactlyOneOwner",
            "(\"ColetaId\" IS NOT NULL AND \"QualidadeId\" IS NULL AND \"QualidadeFuturaId\" IS NULL) OR " +
            "(\"ColetaId\" IS NULL AND \"QualidadeId\" IS NOT NULL AND \"QualidadeFuturaId\" IS NULL) OR " +
            "(\"ColetaId\" IS NULL AND \"QualidadeId\" IS NULL AND \"QualidadeFuturaId\" IS NOT NULL)"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.codigoMedicao).HasConversion<int>().IsRequired();
        builder.Property(e => e.valor).IsRequired(false);
        builder.Property(e => e.unidade).IsRequired();
        builder.Property(e => e.censurado).IsRequired();
        builder.Property(e => e.limite).IsRequired(false);
        builder.HasOne(e => e.Coleta)
            .WithMany(e => e.Medicoes)
            .HasForeignKey(e => e.ColetaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.ColetaId, e.codigoMedicao })
            .IsUnique()
            .HasDatabaseName("IX_Medicoes_ColetaId_codigoMedicao");
        builder.HasOne(e => e.Qualidade)
            .WithMany(e => e.Medicoes)
            .HasForeignKey(e => e.QualidadeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.QualidadeId, e.codigoMedicao })
            .HasDatabaseName("IX_Medicoes_QualidadeId_codigoMedicao");
        builder.HasOne(e => e.QualidadeFutura)
            .WithMany(e => e.Medicoes)
            .HasForeignKey(e => e.QualidadeFuturaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.QualidadeFuturaId, e.codigoMedicao })
            .HasDatabaseName("IX_Medicoes_QualidadeFuturaId_codigoMedicao");
    }
}
