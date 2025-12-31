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

            var wifi = new Amenitie { Name = "Free Wi-Fi" };
            var pool = new Amenitie { Name = "Swimming Pool" };
            var gym = new Amenitie { Name = "Gym Access" };
            var roomService = new Amenitie { Name = "24/7 Room Service" };
            var shuttle = new Amenitie { Name = "Airport Shuttle" };

            db.Amenities.AddRange(wifi, pool, gym, roomService, shuttle);
            await db.SaveChangesAsync();

            await db.Places.AddRangeAsync(
                new Place
                {
                    Name = "Premium Place A",
                    Details = "A luxurious place to stay.",
                    Rate = 250.00m,
                    SquareFeet = 1500,
                    Occupancy = 4,
                    ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa1.jpg",
                    Amenities = { wifi, pool, gym }

                },
                new Place
                {
                    Name = "Premium Place B",
                    Details = "An elegant and comfortable place.",
                    Rate = 300.00m,
                    SquareFeet = 1800,
                    Occupancy = 5,
                    ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa2.jpg",
                    Amenities = { wifi, roomService, shuttle }
                },
                new Place
                {
                    Name = "Premium Place C",
                    Details = "A modern place with all amenities.",
                    Rate = 200.00m,
                    SquareFeet = 1200,
                    Occupancy = 3,
                    ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa3.jpg",
                    Amenities = { pool, gym, shuttle }
                },
                new Place
                {
                    Name = "Premium Place D",
                    Details = "Premium place with spa facilities and concierge services.",
                    Rate = 900.00m,
                    SquareFeet = 4000,
                    Occupancy = 10,
                    ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa4.jpg",
                    Amenities = { wifi, pool, gym, roomService, shuttle }
                },
                new Place
                {
                    Name = "Premium Place I",
                    Details = "Elegant villa with marble interiors and panoramic mountain views.",
                    Rate = 750.00m,
                    SquareFeet = 3200,
                    Occupancy = 6,
                    ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa5.jpg",
                    Amenities = { wifi, pool, gym, roomService }
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
