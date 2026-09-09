using elite.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace elite.Data
{
    // Data/GymDbContext.cs
    public class GymDbContext : DbContext
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<OnlineSession> OnlineSessions { get; set; }
        public DbSet<OnlineSessionSchedule> OnlineSessionSchedules { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Membership>()
                .HasOne(m => m.User)
                .WithOne(u => u.Membership)
                .HasForeignKey<Membership>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassSchedule>()
                .HasCheckConstraint("CK_ClassSchedule_AvailableSlots", "[AvailableSlots] >= 0");

            // Configure PromotionUsage relationships
            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Promotion)
                .WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure a user can only use a promotion once
            modelBuilder.Entity<PromotionUsage>()
                .HasIndex(pu => new { pu.PromotionId, pu.UserId })
                .IsUnique();

            // Seed data
            modelBuilder.Entity<Trainer>().HasData(
                new Trainer { Id = 1, Name = "Sarah Johnson", Specialization = "Yoga", ExperienceYears = 5, Certifications = "RYT 500", Bio = "Experienced yoga instructor", ImageUrl = "/images/trainers/sarah.jpg" },
                new Trainer { Id = 2, Name = "Emily Davis", Specialization = "Pilates", ExperienceYears = 7, Certifications = "Pilates Method Alliance", Bio = "Pilates expert", ImageUrl = "/images/trainers/emily.jpg" }
            );
            modelBuilder.Entity<MembershipType>().HasData(
                new MembershipType
                {
                    Id = 1,
                    Name = "Basic",
                    Description = "Basic membership with 10 hours",
                    DurationMonths = 1,
                    TotalHours = 10,
                    Price = 49.99m,
                    IsActive = true
                },
                new MembershipType
                {
                    Id = 2,
                    Name = "Medium",
                    Description = "Medium membership with 25 hours",
                    DurationMonths = 3,
                    TotalHours = 25,
                    Price = 99.99m,
                    IsActive = true
                },
                new MembershipType
                {
                    Id = 3,
                    Name = "VIP",
                    Description = "VIP membership with 50 hours",
                    DurationMonths = 6,
                    TotalHours = 50,
                    Price = 199.99m,
                    IsActive = true
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
