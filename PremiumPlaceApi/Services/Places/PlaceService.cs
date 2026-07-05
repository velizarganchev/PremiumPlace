using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PremiumPlace.DTO;
using PremiumPlace_API.Data;
using PremiumPlace_API.Models;
using System.Text.RegularExpressions;

namespace PremiumPlace_API.Services.Places
{
    public class PlaceService : IPlaceService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public PlaceService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<PlaceDTO>> CreatePlaceAsync(PlaceCreateDTO dto)
        {
            if (dto is null) return Fail<PlaceDTO>("Place data is required.");

            var name = dto.Name.Trim();
            var existingPlace = await _db.Places.AnyAsync(p => p.Name == name);
            if (existingPlace) return Fail<PlaceDTO>("Place with the same name already exists.", ServiceErrorType.Conflict);

            var (cityOk, cityId, cityError) = await ResolveCityIdAsync(dto.CityId, dto.CityName);
            if (!cityOk) return Fail<PlaceDTO>(cityError ?? "Invalid city.");

            var place = _mapper.Map<Place>(dto);
            place.Name = name;
            place.CityId = cityId;
            place.CreatedAt = DateTime.UtcNow;

            var (ok, amenities, error) = await ResolveAmenitiesAsync(dto.AmenityIds);
            if (!ok) return Fail<PlaceDTO>(error ?? "Invalid amenities.");
            place.Amenitys = amenities;

            await _db.Places.AddAsync(place);
            await _db.SaveChangesAsync();

            var created = await LoadPlaceDtoAsync(place.Id);

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Data = created,
                Message = "Place created successfully."
            };
        }

        public async Task<ServiceResponse<PlaceDTO>> UpdatePlaceAsync(int id, PlaceUpdateDTO dto)
        {
            if (id <= 0) return Fail<PlaceDTO>("Invalid place ID.");
            if (dto is null) return Fail<PlaceDTO>("Place data is required.");
            if (dto.Id != id) return Fail<PlaceDTO>("Place ID mismatch.");

            var placeInDb = await _db.Places
                .Include(p => p.Amenitys)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb is null) return Fail<PlaceDTO>("Place not found.", ServiceErrorType.NotFound);

            var name = dto.Name.Trim();

            var duplicate = await _db.Places
                .AsNoTracking()
                .AnyAsync(p => p.Name == name && p.Id != id);

            if (duplicate) return Fail<PlaceDTO>("Another place with the same name already exists.", ServiceErrorType.Conflict);

            var (cityOk, cityId, cityError) = await ResolveCityIdAsync(dto.CityId, dto.CityName);
            if (!cityOk) return Fail<PlaceDTO>(cityError ?? "Invalid city.");

            _mapper.Map(dto, placeInDb);
            placeInDb.Name = name;
            placeInDb.CityId = cityId;
            placeInDb.UpdatedAt = DateTime.UtcNow;
            ApplyFeatures(placeInDb.Features, dto.Features);

            if (dto.AmenityIds is not null)
            {
                var (ok, amenities, error) = await ResolveAmenitiesAsync(dto.AmenityIds);
                if (!ok) return Fail<PlaceDTO>(error ?? "Invalid amenities.");

                placeInDb.Amenitys.Clear();
                foreach (var a in amenities)
                    placeInDb.Amenitys.Add(a);
            }

            await _db.SaveChangesAsync();

            var updated = await LoadPlaceDtoAsync(id);

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Data = updated,
                Message = "Place updated successfully."
            };
        }

        public async Task<ServiceResponse<PlaceDTO>> UpdatePlacePartialAsync(int id, PlacePatchUpdateDTO dto)
        {
            if (id <= 0) return Fail<PlaceDTO>("Invalid place ID.");
            if (dto is null) return Fail<PlaceDTO>("Place data is required.");

            var placeInDb = await _db.Places
                .Include(p => p.Amenitys)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb is null) return Fail<PlaceDTO>("Place not found.", ServiceErrorType.NotFound);

            if (dto.Name is not null)
            {
                var name = dto.Name.Trim();

                var duplicate = await _db.Places
                    .AsNoTracking()
                    .AnyAsync(p => p.Name == name && p.Id != id);

                if (duplicate) return Fail<PlaceDTO>("Another place with the same name already exists.", ServiceErrorType.Conflict);

                placeInDb.Name = name;
            }

            if (dto.Details is not null) placeInDb.Details = dto.Details;

            if (dto.GuestCapacity.HasValue) placeInDb.GuestCapacity = dto.GuestCapacity.Value;
            if (dto.Rate.HasValue) placeInDb.Rate = dto.Rate.Value;
            if (dto.Beds.HasValue) placeInDb.Beds = dto.Beds.Value;
            if (dto.CheckInHour.HasValue) placeInDb.CheckInHour = dto.CheckInHour.Value;
            if (dto.CheckOutHour.HasValue) placeInDb.CheckOutHour = dto.CheckOutHour.Value;
            if (dto.SquareFeet.HasValue) placeInDb.SquareFeet = dto.SquareFeet.Value;

            if (dto.ImageUrl is not null) placeInDb.ImageUrl = dto.ImageUrl;

            if (dto.CityId.HasValue)
            {
                var cityExists = await _db.Cities.AnyAsync(c => c.Id == dto.CityId.Value);
                if (!cityExists) return Fail<PlaceDTO>("Invalid city.");
                placeInDb.CityId = dto.CityId.Value;
            }

            if (dto.Features is not null)
            {
                var f = placeInDb.Features;
                var pf = dto.Features;

                if (pf.Internet.HasValue) f.Internet = pf.Internet.Value;
                if (pf.AirConditioned.HasValue) f.AirConditioned = pf.AirConditioned.Value;
                if (pf.PetsAllowed.HasValue) f.PetsAllowed = pf.PetsAllowed.Value;
                if (pf.Parking.HasValue) f.Parking = pf.Parking.Value;
                if (pf.Entertainment.HasValue) f.Entertainment = pf.Entertainment.Value;
                if (pf.Kitchen.HasValue) f.Kitchen = pf.Kitchen.Value;
                if (pf.Refrigerator.HasValue) f.Refrigerator = pf.Refrigerator.Value;
                if (pf.Washer.HasValue) f.Washer = pf.Washer.Value;
                if (pf.Dryer.HasValue) f.Dryer = pf.Dryer.Value;
                if (pf.SelfCheckIn.HasValue) f.SelfCheckIn = pf.SelfCheckIn.Value;
            }

            if (dto.AmenityIds is not null)
            {
                var (ok, amenities, error) = await ResolveAmenitiesAsync(dto.AmenityIds);
                if (!ok) return Fail<PlaceDTO>(error ?? "Invalid amenities.");

                placeInDb.Amenitys.Clear();
                foreach (var a in amenities)
                    placeInDb.Amenitys.Add(a);
            }

            placeInDb.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Data = _mapper.Map<PlaceDTO>(placeInDb),
                Message = "Place updated successfully."
            };
        }

        public async Task<ServiceResponse<List<PlaceDTO>>> GetAllPlacesAsync()
        {
            var dbPlaces = await _db.Places
                .AsNoTracking()
                .Include(p => p.City)
                .Include(p => p.Amenitys)
                .Include(p => p.Reviews)
                .ToListAsync();

            return new ServiceResponse<List<PlaceDTO>>
            {
                Success = true,
                Data = _mapper.Map<List<PlaceDTO>>(dbPlaces),
                Message = dbPlaces.Count == 0 ? "No places found." : "Places retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResult<PlaceDTO>>> SearchPlacesAsync(PlaceQueryDTO query)
        {
            query ??= new PlaceQueryDTO();

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

            var q = _db.Places
                .AsNoTracking()
                .Include(p => p.City)
                .Include(p => p.Amenitys)
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var city = query.City.Trim();
                q = q.Where(p => p.City.Name == city);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = $"%{query.Search.Trim()}%";
                q = q.Where(p =>
                    EF.Functions.Like(p.Name, term) ||
                    EF.Functions.Like(p.City.Name, term) ||
                    (p.Details != null && EF.Functions.Like(p.Details, term)) ||
                    p.Amenitys.Any(a => EF.Functions.Like(a.Name, term)));
            }

            q = query.Sort switch
            {
                "priceAsc" => q.OrderBy(p => p.Rate).ThenBy(p => p.Id),
                "priceDesc" => q.OrderByDescending(p => p.Rate).ThenBy(p => p.Id),
                "capacityDesc" => q.OrderByDescending(p => p.GuestCapacity).ThenBy(p => p.Id),
                _ => q.OrderBy(p => p.Id)
            };

            var total = await q.CountAsync();

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ServiceResponse<PagedResult<PlaceDTO>>
            {
                Success = true,
                Data = new PagedResult<PlaceDTO>
                {
                    Items = _mapper.Map<List<PlaceDTO>>(items),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                },
                Message = "Places retrieved successfully."
            };
        }

        public async Task<ServiceResponse<List<string>>> GetCityNamesAsync()
        {
            var cities = await _db.Places
                .AsNoTracking()
                .Select(p => p.City.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            return new ServiceResponse<List<string>>
            {
                Success = true,
                Data = cities,
                Message = "Cities retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PlaceDetailsDTO>> GetPlaceByIdAsync(int id)
        {
            if (id <= 0) return Fail<PlaceDetailsDTO>("Invalid place ID.");

            var placeInDb = await _db.Places
                .AsNoTracking()
                .Include(p => p.City)
                .Include(p => p.Amenitys)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb is null) return Fail<PlaceDetailsDTO>("Place not found.", ServiceErrorType.NotFound);

            return new ServiceResponse<PlaceDetailsDTO>
            {
                Success = true,
                Data = _mapper.Map<PlaceDetailsDTO>(placeInDb),
                Message = "Place retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PlaceOptionsDTO>> GetPlaceOptionsAsync()
        {
            var cities = await _db.Cities
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var amenities = await _db.Amenitys
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();

            return new ServiceResponse<PlaceOptionsDTO>
            {
                Success = true,
                Data = new PlaceOptionsDTO
                {
                    Cities = _mapper.Map<List<CityDTO>>(cities),
                    Amenities = _mapper.Map<List<AmenityDTO>>(amenities)
                },
                Message = "Place options retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PlaceDTO>> DeletePlaceAsync(int id)
        {
            if (id <= 0) return Fail<PlaceDTO>("Invalid place ID.");

            var place = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place is null) return Fail<PlaceDTO>("Place not found.", ServiceErrorType.NotFound);

            var hasBookings = await _db.Bookings.AnyAsync(b => b.PlaceId == id);
            if (hasBookings)
                return Fail<PlaceDTO>("Place cannot be deleted because it has bookings.", ServiceErrorType.Conflict);

            _db.Places.Remove(place);
            await _db.SaveChangesAsync();

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Message = "Place deleted successfully."
            };
        }

        private async Task<(bool ok, List<Amenity> amenities, string? error)> ResolveAmenitiesAsync(List<int>? amenityIds)
        {
            if (amenityIds is not { Count: > 0 })
                return (true, new List<Amenity>(), null);

            var ids = amenityIds.Distinct().ToList();

            var amenities = await _db.Amenitys
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            if (amenities.Count != ids.Count)
            {
                var found = amenities.Select(a => a.Id).ToHashSet();
                var missing = ids.Where(id => !found.Contains(id));
                return (false, new List<Amenity>(), $"Invalid amenity ids: {string.Join(", ", missing)}");
            }

            return (true, amenities, null);
        }

        private async Task<(bool ok, int cityId, string? error)> ResolveCityIdAsync(int cityId, string? cityName)
        {
            if (cityId > 0)
            {
                var exists = await _db.Cities.AnyAsync(c => c.Id == cityId);
                return exists
                    ? (true, cityId, null)
                    : (false, 0, "Invalid city.");
            }

            var name = CityNames.Normalize(cityName);
            if (string.IsNullOrWhiteSpace(name))
                return (false, 0, "City is required.");

            // Case-insensitive match so "Berlin", "berlin" and "berlin " resolve to one city.
            var lowered = name.ToLower();
            var existing = await _db.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == lowered);
            if (existing is not null)
                return (true, existing.Id, null);

            var city = new City { Name = name };
            await _db.Cities.AddAsync(city);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Lost a race (or hit a unique constraint) — reuse the city that now exists.
                _db.Entry(city).State = EntityState.Detached;
                var raced = await _db.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == lowered);
                if (raced is null) throw;
                return (true, raced.Id, null);
            }

            return (true, city.Id, null);
        }

        private async Task<PlaceDTO> LoadPlaceDtoAsync(int id)
        {
            var place = await _db.Places
                .AsNoTracking()
                .Include(p => p.City)
                .Include(p => p.Amenitys)
                .Include(p => p.Reviews)
                .FirstAsync(p => p.Id == id);

            return _mapper.Map<PlaceDTO>(place);
        }

        private static void ApplyFeatures(PlaceFeatures target, PlaceFeaturesDTO source)
        {
            target.Internet = source.Internet;
            target.AirConditioned = source.AirConditioned;
            target.PetsAllowed = source.PetsAllowed;
            target.Parking = source.Parking;
            target.Entertainment = source.Entertainment;
            target.Kitchen = source.Kitchen;
            target.Refrigerator = source.Refrigerator;
            target.Washer = source.Washer;
            target.Dryer = source.Dryer;
            target.SelfCheckIn = source.SelfCheckIn;
        }

        private static ServiceResponse<T> Fail<T>(string message, ServiceErrorType type = ServiceErrorType.Validation)
            => new() { Success = false, Message = message, ErrorType = type };
    }
}
