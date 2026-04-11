namespace Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Infrastructure.Data.Tables; 
    using Domain.User;
    public class WaterPathDbContext : DbContext
    {
        public WaterPathDbContext(DbContextOptions<WaterPathDbContext> options) : base(options)
        {
        }

        public DbSet<User> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired();
                entity.Property(e => e.Senha).IsRequired();
                entity.Property(e => e.Email).IsRequired();
            }
            );

        }
    }
}