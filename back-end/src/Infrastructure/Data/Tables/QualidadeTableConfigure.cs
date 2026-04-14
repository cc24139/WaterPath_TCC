namespace Infrastructure.Data.Tables;

using back_end.src.Domain.Qualidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class QualidadeTableConfigure : IEntityTypeConfiguration<QualidadeEntity>
{
    public void Configure(EntityTypeBuilder<QualidadeEntity> builder)
    {
        builder.ToTable("Qualidades", "waterPath");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.IQA).IsRequired();

        builder
            .HasOne(e => e.CorpoHidrico)
            .WithMany()
            .HasForeignKey("CorpoHidricoId")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(e => e.QualidadeFutura)
            .WithMany(e => e.Qualidade)
            .HasForeignKey("QualidadeFuturaId")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(e => e.MetaisPesados)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "QualidadeMetaisPesados",
                right =>
                    right
                        .HasOne<back_end.src.Domain.MetalPesado.MetalPesadoEntity>()
                        .WithMany()
                        .HasForeignKey("MetalPesadoId")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left.HasOne<QualidadeEntity>()
                        .WithMany()
                        .HasForeignKey("QualidadeId")
                        .OnDelete(DeleteBehavior.NoAction),
                join =>
                {
                    join.ToTable("QualidadeMetaisPesados", "waterPath");
                    join.HasKey("QualidadeId", "MetalPesadoId");
                }
            );

        builder
            .HasMany(e => e.CianoBacterias)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "QualidadeCianoBacterias",
                right =>
                    right
                        .HasOne<back_end.src.Domain.CianoBacteria.CianoBacteriaEntity>()
                        .WithMany()
                        .HasForeignKey("CianoBacteriaId")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left.HasOne<QualidadeEntity>()
                        .WithMany()
                        .HasForeignKey("QualidadeId")
                        .OnDelete(DeleteBehavior.NoAction),
                join =>
                {
                    join.ToTable("QualidadeCianoBacterias", "waterPath");
                    join.HasKey("QualidadeId", "CianoBacteriaId");
                }
            );
    }
}
