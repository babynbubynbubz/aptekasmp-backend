using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<Box> Boxes { get; set; }
        public DbSet<Medkit> Medkits { get; set; }
        public DbSet<DispensingLog> DispensingLogs { get; set; }
        public DbSet<ReceivingLog> ReceivingLogs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Box>()
                .HasIndex(b => new { b.GId, b.SerialNumber })
                .IsUnique();


            modelBuilder.Entity<DispensingLog>()
                .HasOne(dl => dl.Box)
                .WithMany(b => b.DispensingLogs)
                .HasForeignKey(dl => dl.BoxId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DispensingLog>()
                .HasOne(dl => dl.Medkit)
                .WithMany(m => m.DispensingLogs)
                .HasForeignKey(dl => dl.MedkitId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ReceivingLog>()
                .HasOne(rl => rl.Box)
                .WithMany(b => b.ReceivingLogs)
                .HasForeignKey(rl => rl.BoxId)
                .OnDelete(DeleteBehavior.Restrict);
            
        }
    }
}