using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Models;

namespace PharmacyQueue.Data
{
    /// Database context for the Pharmacy Queue system.
    /// Manages database connections and entity configurations.
    public class PharmacyDbContext : DbContext
    {
        // Constructor that accepts database configuration options
        public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options)
            : base(options)
        {
        }

        // Database tables for each entity
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<QueueSetting> QueueSettings { get; set; }

        /// <summary>
        /// Configures the database model and relationships between entities.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TimeOnly properties to be stored as time in MySQL
            // This is necessary because MySQL doesn't have a native TimeOnly type
            modelBuilder.Entity<QueueSetting>()
                .Property(q => q.WorkingHoursStart)
                .HasConversion(
                    v => v.ToTimeSpan(),
                    v => TimeOnly.FromTimeSpan(v));

            modelBuilder.Entity<QueueSetting>()
                .Property(q => q.WorkingHoursEnd)
                .HasConversion(
                    v => v.ToTimeSpan(),
                    v => TimeOnly.FromTimeSpan(v));

            // Configure string length constraints for Appointment properties
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Purpose)
                .HasMaxLength(500);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.QueueNumber)
                .HasMaxLength(20);

            // Configure string length constraints for Admin properties
            modelBuilder.Entity<Admin>()
                .Property(a => a.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Admin>()
                .Property(a => a.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Admin>()
                .Property(a => a.Password)
                .HasMaxLength(100);

            modelBuilder.Entity<Admin>()
                .Property(a => a.Role)
                .HasMaxLength(20);

            // Configure string length constraints for Notification properties
            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasMaxLength(50);

            modelBuilder.Entity<Notification>()
                .Property(n => n.EmailContent)
                .HasMaxLength(500);

            // Configure the relationship between Notification and Appointment
            // When an appointment is deleted, all related notifications are also deleted
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Appointment)
                .WithMany()
                .HasForeignKey(n => n.AppointmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed the database with a default admin user
            // This ensures there's always at least one admin account available
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                AdminID = 1,
                Name = "Admin",
                Email = "admin@pharmacy.com",
                Password = "Admin@123",
                Role = "Administrator",
                IsActive = true,
                CreatedDate = new DateTime(2024, 3, 21, 0, 0, 0, DateTimeKind.Utc),
                LastLogin = new DateTime(2024, 3, 21, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}