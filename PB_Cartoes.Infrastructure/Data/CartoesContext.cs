using Microsoft.EntityFrameworkCore;
using PB_Cartoes.Domain.Entities;

namespace PB_Cartoes.Infrastructure.Data
{
    public class CartoesContext : DbContext
    {
        public CartoesContext(DbContextOptions<CartoesContext> options) : base(options)
        {
        }

        public DbSet<Cartao> Cartoes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cartao>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.NumeroCartao).IsRequired().HasMaxLength(50);
                b.Property(x => x.Limite).HasColumnType("decimal(18,2)");
                b.Property(x => x.CriadoEmUtc).IsRequired();
            });
        }
    }
}