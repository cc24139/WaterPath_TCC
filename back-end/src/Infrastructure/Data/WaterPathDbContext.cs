namespace Infrastructure.Data
{
    using back_end.src.Domain.CianoBacteria;
    using back_end.src.Domain.Coleta;
    using back_end.src.Domain.CorpoHidrico;
    using back_end.src.Domain.Imagem;
    using back_end.src.Domain.MetalPesado;
    using back_end.src.Domain.Qualidade;
    using back_end.src.Domain.QualidadeFutura;
    using Domain.User;
    using Infrastructure.Data.Tables;
    using Microsoft.EntityFrameworkCore;

    public class WaterPathDbContext : DbContext
    {
        public WaterPathDbContext(DbContextOptions<WaterPathDbContext> options)
            : base(options) { }

        public DbSet<User> Usuarios { get; set; }
        public DbSet<CorpoHidricoEntity> CorposHidricos { get; set; }
        public DbSet<ColetaEntity> Coletas { get; set; }
        public DbSet<ImagemEntity> Imagens { get; set; }
        public DbSet<MetalPesadoEntity> MetaisPesados { get; set; }
        public DbSet<CianoBacteriaEntity> CianoBacterias { get; set; }
        public DbSet<QualidadeEntity> Qualidades { get; set; }
        public DbSet<QualidadeFuturaEntity> QualidadesFuturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserTableConfigure());
            modelBuilder.ApplyConfiguration(new CorpoHidricoTableConfigure());
            modelBuilder.ApplyConfiguration(new ColetaTableConfigure());
            modelBuilder.ApplyConfiguration(new ImagemTableConfigure());
            modelBuilder.ApplyConfiguration(new MetalPesadoTableConfigure());
            modelBuilder.ApplyConfiguration(new CianoBacteriaTableConfigure());
            modelBuilder.ApplyConfiguration(new QualidadeTableConfigure());
            modelBuilder.ApplyConfiguration(new QualidadeFuturaTableConfigure());

            base.OnModelCreating(modelBuilder);
        }
    }
}
