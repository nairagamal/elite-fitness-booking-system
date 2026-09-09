using elite.Models;

namespace elite.Data
{
    // Data/DataSeeder.cs
    public class DataSeeder
    {
        private readonly GymDbContext _context;

        public DataSeeder(GymDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            // Seed trainers if none exist
            if (!_context.Trainers.Any())
            {
                var trainers = new List<Trainer>
            {
                new Trainer
    {
        Name = "Sarah Johnson",
        Specialization = "Yoga",
        ExperienceYears = 5,
        Certifications = "RYT 500",
        Bio = "Experienced yoga instructor with 5 years of teaching experience",
        ImageUrl = null // Set to null since we're not seeding images
    },
                new Trainer
                {
                    Name = "Emily Davis",
                    Specialization = "Pilates",
                    ExperienceYears = 7,
                    Certifications = "Pilates Method Alliance",
                    Bio = "Pilates expert with international certification",
                     ImageUrl = null
                },
                new Trainer
                {
                    Name = "Jessica Wilson",
                    Specialization = "HIIT",
                    ExperienceYears = 4,
                    Certifications = "ACE Certified",
                    Bio = "High-intensity interval training specialist",
                     ImageUrl = null
                }
            };

                _context.Trainers.AddRange(trainers);
                await _context.SaveChangesAsync();
            }

            // Seed admin user if none exists
            if (!_context.Admins.Any())
            {
                var admin = new Admin
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@gym.com",
                    Role = "Admin"
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
            }

            // Seed membership types if none exist
            if (!_context.MembershipTypes.Any())
            {
                var membershipTypes = new List<MembershipType>
{
    new MembershipType
    {
        Name = "Basic",
        Description = "Basic membership with 10 hours",
        DurationMonths = 1,
        TotalHours = 10.0m,      // Use decimal notation
        Price = 49.99m,
        IsActive = true
    },
    new MembershipType
    {
        Name = "Medium",
        Description = "Medium membership with 25 hours",
        DurationMonths = 3,
        TotalHours = 25.0m,      // Use decimal notation
        Price = 99.99m,
        IsActive = true
    },
    new MembershipType
    {
        Name = "VIP",
        Description = "VIP membership with 50 hours",
        DurationMonths = 6,
        TotalHours = 50.0m,      // Use decimal notation
        Price = 199.99m,
        IsActive = true
    }
};

                _context.MembershipTypes.AddRange(membershipTypes);
                await _context.SaveChangesAsync();
            }

            // Seed sample products if none exist
            if (!_context.Products.Any())
            {
                var products = new List<Product>
            {
                new Product
                {
                    Name = "Yoga Mat",
                    Description = "High-quality non-slip yoga mat",
                    Price = 29.99m,
                    ImageUrl = "/images/products/yoga-mat.jpg",
                    Category = "Apparel",
                    StockQuantity = 50
                },
                new Product
                {
                    Name = "Protein Powder",
                    Description = "Whey protein powder for muscle recovery",
                    Price = 39.99m,
                    ImageUrl = "/images/products/protein-powder.jpg",
                    Category = "Supplements",
                    StockQuantity = 30
                },
                new Product
                {
                    Name = "Workout Gloves",
                    Description = "Comfortable gloves for weight training",
                    Price = 19.99m,
                    ImageUrl = "/images/products/workout-gloves.jpg",
                    Category = "Apparel",
                    StockQuantity = 25
                }
            };

                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
            }
        }
    }
}
