using Microsoft.EntityFrameworkCore;
using PremiumPlace_API.Models;

namespace PremiumPlace_API.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Places.AnyAsync())
                return;

            await db.Places.AddRangeAsync(
                new Place
                {
                    Name = "Premium Place A",
                    Details = "A luxurious place to stay.",
                    Rate = 250.00m,
                    SquareFeet = 1500,
                    Occupancy = 4,
                    ImageUrl = "https://example.com/images/placeA.jpg"
                },
                new Place
                {
                    Name = "Premium Place B",
                    Details = "An elegant and comfortable place.",
                    Rate = 300.00m,
                    SquareFeet = 1800,
                    Occupancy = 5,
                    ImageUrl = "https://example.com/images/placeB.jpg"
                },
                new Place
                {
                    Name = "Premium Place C",
                    Details = "A modern place with all amenities.",
                    Rate = 200.00m,
                    SquareFeet = 1200,
                    Occupancy = 3,
                    ImageUrl = "https://example.com/images/placeC.jpg"
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
