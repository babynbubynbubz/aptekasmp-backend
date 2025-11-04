using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Box> Boxes { get; set; }
        public DbSet<DispensingLog> DispensingLogs { get; set; }
        public DbSet<ReceivingLog> ReceivingLogs { get; set; }
        public DbSet<Medkit> Medkits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DispensingLog -> Box
            modelBuilder.Entity<DispensingLog>()
                .HasOne(d => d.Box)
                .WithMany()
                .HasForeignKey(d => d.BoxId)
                .OnDelete(DeleteBehavior.Restrict);

            // DispensingLog -> Medkit
            modelBuilder.Entity<DispensingLog>()
                .HasOne(d => d.Medkit)
                .WithMany()
                .HasForeignKey(d => d.MedkitId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReceivingLog -> Box
            modelBuilder.Entity<ReceivingLog>()
                .HasOne(r => r.Box)
                .WithMany()
                .HasForeignKey(r => r.BoxId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}