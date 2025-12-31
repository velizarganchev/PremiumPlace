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
        public async Task<ServiceResponse<PlaceDTO>> CreatePlaceAsync(PlaceCreateDTO createPlaceDTO)
        {
            var response = new ServiceResponse<PlaceDTO>();

            var existingPlace = await _db.Places.AnyAsync(p => p.Name == createPlaceDTO.Name.Trim());

            if (existingPlace)
            {
                return Fail("Place with the same name already exists.");
            }

            Place place = _mapper.Map<Place>(createPlaceDTO);

            var (ok, amenities, error) = await ResolveAmenitiesAsync(createPlaceDTO.AmenitieIds);

            if (!ok)
            {
                response.Success = false;
                response.Message = error;
                return response;
            }

            place.Amenities = amenities;

            await _db.Places.AddAsync(place);
            await _db.SaveChangesAsync();

            response.Success = true;
            response.Data = _mapper.Map<PlaceDTO>(place);
            response.Message = "Place created successfully.";
            return response;
        }

        public async Task<ServiceResponse<PlaceDTO>> DeletePlaceAsync(int id)
        {
            var response = new ServiceResponse<PlaceDTO>();
            if (id <= 0)
            {
                return Fail("Invalid place ID.");
            }

            var place = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place == null)
            {
                return Fail("Place not found.");
            }

            _db.Places.Remove(place);
            await _db.SaveChangesAsync();

            response.Success = true;
            response.Message = "Place deleted successfully.";
            return response;
        }

        public async Task<ServiceResponse<List<PlaceDTO>>> GetAllPlacesAsync()
        {
            var serviceResponse = new ServiceResponse<List<PlaceDTO>>();

            var dbPlaces = await _db.Places
                    .AsNoTracking()
                    .Include(p => p.Amenities)
                    .ToListAsync();

            if (dbPlaces == null || dbPlaces.Count == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "No places found.";
                return serviceResponse;
            }

            var dbPlacesDto = _mapper.Map<List<PlaceDTO>>(dbPlaces);

            serviceResponse.Data = dbPlacesDto;
            serviceResponse.Success = true;
            serviceResponse.Message = "Places retrieved successfully.";
            return serviceResponse;
        }

        public async Task<ServiceResponse<PlaceDTO>> GetPlaceByIdAsync(int id)
        {
            var serviceResponse = new ServiceResponse<PlaceDTO>();

            if (id <= 0)
            {
                return Fail("Invalid place ID.");
            }

            var placeInDb = await _db.Places.AsNoTracking().Include(p => p.Amenities).FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb == null)
            {
                return Fail("Place not found.");
            }

            var place = _mapper.Map<PlaceDTO>(placeInDb);
            serviceResponse.Data = place;
            serviceResponse.Success = true;
            serviceResponse.Message = "Place retrieved successfully.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<PlaceDTO>> UpdatePlaceAsync(int id, PlaceUpdateDTO placeDTO)
        {
            var response = new ServiceResponse<PlaceDTO>();

            if (id <= 0)
            {
                return Fail("Invalid place ID.");
            }

            if (placeDTO is null)
            {
                return Fail("Place data is required.");
            }

            if (placeDTO.Id != id)
            {
                return Fail("Place ID mismatch.");
            }

            var placeInDb = await _db.Places
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb == null)
            {
                response.Success = false;
                response.Message = "Place not found.";
                return response;
            }

            var name = placeDTO.Name.Trim();
            var existingPlace = await _db.Places
                .AsNoTracking()
                .AnyAsync(p => p.Name == name && p.Id != id);

            if (existingPlace)
            {
                response.Success = false;
                response.Message = "Another place with the same name already exists.";
                return response;
            }

            _mapper.Map(placeDTO, placeInDb);
            placeInDb.UpdatedDate = DateTime.UtcNow;

            if (placeDTO.AmenitieIds is not null)
            {
                var (ok, amenities, error) = await ResolveAmenitiesAsync(placeDTO.AmenitieIds);
                if (!ok)
                {
                    response.Success = false;
                    response.Message = error;
                    return response;
                }

                // replace
                placeInDb.Amenities.Clear();
                foreach (var amenity in amenities)
                    placeInDb.Amenities.Add(amenity);
            }

            await _db.SaveChangesAsync();

            response.Success = true;
            response.Data = _mapper.Map<PlaceDTO>(placeInDb);
            response.Message = "Place updated successfully.";
            return response;
        }

        private async Task<(bool ok, List<Amenitie> amenities, string? error)>
            ResolveAmenitiesAsync(List<int>? amenitieIds)
        {
            if (amenitieIds is not { Count: > 0 })
                return (true, new List<Amenitie>(), null);

            var ids = amenitieIds
                .Distinct()
                .ToList();

            var amenities = await _db.Amenities
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            if (amenities.Count != ids.Count)
            {
                var found = amenities.Select(a => a.Id).ToHashSet();
                var missing = ids.Where(id => !found.Contains(id));
                return (
                    false,
                    new List<Amenitie>(),
                    $"Invalid amenity ids: {string.Join(", ", missing)}"
                );
            }

            return (true, amenities, null);
        }

        private static ServiceResponse<PlaceDTO> Fail(string message) => new() { Success = false, Message = message };
    }
}
