using Microsoft.EntityFrameworkCore;
using EventManagement.API.Models;

namespace EventManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Event>()
                .Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Event>()
                .Property(e => e.AdditionalInfo)
                .HasMaxLength(1000);

            modelBuilder.Entity<Participant>()
                .Property(p => p.AdditionalInfo)
                .HasMaxLength(5000);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 