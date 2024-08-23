using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FasonPortal.Models;

namespace FasonPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IsTipi> IsTipleri { get; set; }
        public DbSet<Atolye> Atolyeler { get; set; }
        public DbSet<AtolyeIs> AtolyeIsler { get; set; }
        public DbSet<Fabrika> Fabrikalar { get; set; }
        public DbSet<IsEmri> IsEmirleri { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AtolyeIs>()
                .HasOne(ai => ai.Atolye)
                .WithMany(a => a.AtolyeIsler)
                .HasForeignKey(ai => ai.AtolyeId);

            builder.Entity<AtolyeIs>()
                .HasOne(ai => ai.IsTipi)
                .WithMany(it => it.AtolyeIsler)
                .HasForeignKey(ai => ai.IsTipiId);

            builder.Entity<IsEmri>()
                .HasOne(ie => ie.Atolye)
                .WithMany(a => a.IsEmirleri) // İlişki burada tanımlanır
                .HasForeignKey(ie => ie.AtolyeId);

            builder.Entity<IsEmri>()
                .HasOne(ie => ie.IsTipi)
                .WithMany(it => it.IsEmirleri)
                .HasForeignKey(ie => ie.IsTipiId);

            builder.Entity<IsEmri>()
                .HasOne(ie => ie.Fabrika)
                .WithMany(f => f.IsEmirleri)
                .HasForeignKey(ie => ie.FabrikaId);
        }
    }
}
