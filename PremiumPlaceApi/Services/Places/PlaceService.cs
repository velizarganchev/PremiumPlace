using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PremiumPlace.DTO;
using PremiumPlace_API.Data;
using PremiumPlace_API.Models;

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
            if (existingPlace) return Fail<PlaceDTO>("Place with the same name already exists.");

            var cityExists = await _db.Cities.AnyAsync(c => c.Id == dto.CityId);
            if (!cityExists) return Fail<PlaceDTO>("Invalid city.");

            var place = _mapper.Map<Place>(dto);
            place.Name = name;
            place.CreatedAt = DateTime.UtcNow;

            var (ok, amenities, error) = await ResolveAmenitiesAsync(dto.AmenityIds);
            if (!ok) return Fail<PlaceDTO>(error ?? "Invalid amenities.");
            place.Amenitys = amenities;

            await _db.Places.AddAsync(place);
            await _db.SaveChangesAsync();

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Data = _mapper.Map<PlaceDTO>(place),
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

            if (placeInDb is null) return Fail<PlaceDTO>("Place not found.");

            var name = dto.Name.Trim();

            var duplicate = await _db.Places
                .AsNoTracking()
                .AnyAsync(p => p.Name == name && p.Id != id);

            if (duplicate) return Fail<PlaceDTO>("Another place with the same name already exists.");

            var cityExists = await _db.Cities.AnyAsync(c => c.Id == dto.CityId);
            if (!cityExists) return Fail<PlaceDTO>("Invalid city.");

            _mapper.Map(dto, placeInDb);
            placeInDb.Name = name;
            placeInDb.UpdatedAt = DateTime.UtcNow;

            if (dto.AmenityIds is not null)
            {
                var (ok, amenities, error) = await ResolveAmenitiesAsync(dto.AmenityIds);
                if (!ok) return Fail<PlaceDTO>(error ?? "Invalid amenities.");

                placeInDb.Amenitys.Clear();
                foreach (var a in amenities)
                    placeInDb.Amenitys.Add(a);
            }

            await _db.SaveChangesAsync();

            return new ServiceResponse<PlaceDTO>
            {
                Success = true,
                Data = _mapper.Map<PlaceDTO>(placeInDb),
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

            if (placeInDb is null) return Fail<PlaceDTO>("Place not found.");

            if (dto.Name is not null)
            {
                var name = dto.Name.Trim();

                var duplicate = await _db.Places
                    .AsNoTracking()
                    .AnyAsync(p => p.Name == name && p.Id != id);

                if (duplicate) return Fail<PlaceDTO>("Another place with the same name already exists.");

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

            if (dbPlaces.Count == 0)
                return new ServiceResponse<List<PlaceDTO>> { Success = false, Message = "No places found." };

            return new ServiceResponse<List<PlaceDTO>>
            {
                Success = true,
                Data = _mapper.Map<List<PlaceDTO>>(dbPlaces),
                Message = "Places retrieved successfully."
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

            if (placeInDb is null) return Fail<PlaceDetailsDTO>("Place not found.");

            return new ServiceResponse<PlaceDetailsDTO>
            {
                Success = true,
                Data = _mapper.Map<PlaceDetailsDTO>(placeInDb),
                Message = "Place retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PlaceDTO>> DeletePlaceAsync(int id)
        {
            if (id <= 0) return Fail<PlaceDTO>("Invalid place ID.");

            var place = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place is null) return Fail<PlaceDTO>("Place not found.");

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

        private static ServiceResponse<T> Fail<T>(string message)
            => new() { Success = false, Message = message };
    }
}
