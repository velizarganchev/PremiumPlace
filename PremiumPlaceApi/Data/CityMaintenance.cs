using Microsoft.EntityFrameworkCore;
using PremiumPlace_API.Models;

namespace PremiumPlace_API.Data
{
    /// <summary>
    /// One-time / idempotent housekeeping for the Cities table. Collapses cities
    /// that are duplicates once normalized (case- and whitespace-insensitive),
    /// repointing their places to the surviving canonical city.
    /// </summary>
    public static class CityMaintenance
    {
        public static async Task<int> MergeDuplicatesAsync(ApplicationDbContext db)
        {
            var cities = await db.Cities.ToListAsync();

            var duplicateGroups = cities
                .GroupBy(c => CityNames.Key(c.Name))
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicateGroups.Count == 0)
                return 0;

            var removed = 0;

            foreach (var group in duplicateGroups)
            {
                var ordered = group.OrderBy(c => c.Id).ToList();
                var canonical = ordered.First();

                // Normalize the surviving name so the stored value is clean.
                canonical.Name = CityNames.Normalize(canonical.Name);

                var duplicates = ordered.Skip(1).ToList();
                var duplicateIds = duplicates.Select(c => c.Id).ToList();

                var places = await db.Places
                    .Where(p => duplicateIds.Contains(p.CityId))
                    .ToListAsync();

                foreach (var place in places)
                    place.CityId = canonical.Id;

                db.Cities.RemoveRange(duplicates);
                removed += duplicates.Count;
            }

            await db.SaveChangesAsync();
            return removed;
        }
    }
}
