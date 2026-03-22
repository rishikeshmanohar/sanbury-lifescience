using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new AppUser
            {
                FullName = "Admin User",
                Email = "admin@sanbury.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsApproved = true
            });
        }

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product
                {
                    Name = "Paracetamol 500mg",
                    Category = "Tablet",
                    Brand = "Sanbury",
                    BatchNumber = "PARA500-A1",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    Price = 25,
                    GstPercent = 12,
                    StockQuantity = 200,
                    Description = "Pain relief tablet",
                    ImageUrl = "/images/paracetamol.jpg"
                },
                new Product
                {
                    Name = "Vitamin C Syrup",
                    Category = "Syrup",
                    Brand = "Sanbury",
                    BatchNumber = "VITC-S2",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    Price = 80,
                    GstPercent = 12,
                    StockQuantity = 100,
                    Description = "Immunity support syrup",
                    ImageUrl = "/images/syrup.jpg"
                }
            );
        }

        db.SaveChanges();
    }
}