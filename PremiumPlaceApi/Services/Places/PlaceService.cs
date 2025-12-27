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
            var existingPlace = _mapper.Map<PlaceDTO>(await _db.Places.FirstOrDefaultAsync(p => p.Name == createPlaceDTO.Name));

            if (existingPlace != null)
            {
                response.Success = false;
                response.Message = "Place with the same name already exists.";
                return response;
            }
            var place = _mapper.Map<Place>(createPlaceDTO);
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
                response.Success = false;
                response.Message = "Invalid place ID.";
                return response;
            }

            var place = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place == null)
            {
                response.Success = false;
                response.Message = "Place not found.";
                return response;
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
                    .Select(p => _mapper.Map<PlaceDTO>(p))
                    .ToListAsync();
            if (dbPlaces == null || dbPlaces.Count == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "No places found.";
                return serviceResponse;
            }
            serviceResponse.Data = dbPlaces;
            serviceResponse.Success = true;
            serviceResponse.Message = "Places retrieved successfully.";
            return serviceResponse;
        }

        public async Task<ServiceResponse<PlaceDTO>> GetPlaceByIdAsync(int id)
        {
            var serviceResponse = new ServiceResponse<PlaceDTO>();

            if (id <= 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Invalid place ID.";
                return serviceResponse;
            }
            var place = _mapper.Map<PlaceDTO>(await _db.Places.FirstOrDefaultAsync(p => p.Id == id));
            if (place == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Place not found.";
                return serviceResponse;
            }
            serviceResponse.Data = place;
            serviceResponse.Success = true;
            serviceResponse.Message = "Place retrieved successfully.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<PlaceDTO>> UpdatePlaceAsync(int id,PlaceUpdateDTO placeDTO)
        {
            var response = new ServiceResponse<PlaceDTO>();
            if (id <= 0) {
                response.Success = false;
                response.Message = "Invalid place ID.";
                return response;
            }

            var placeInDb = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);

            if (placeInDb == null) {
                response.Success = false;
                response.Message = "Place not found.";
                return response;
            }

            if (id != placeDTO.Id)
            {
                response.Success = false;
                response.Message = "Place ID mismatch.";
                return response;
            }

            var duplicatePlace = await _db.Places.FirstOrDefaultAsync(p => p.Name == placeDTO.Name && p.Id != placeDTO.Id);

            if (duplicatePlace != null)
            {
                response.Success = false;
                response.Message = "Another place with the same name already exists.";
                response.Data = _mapper.Map<PlaceDTO>(duplicatePlace);
                return response;
            }

            Place placeToAdd = _mapper.Map(placeDTO, placeInDb);
            placeInDb.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();

            response.Success = true;
            response.Data = _mapper.Map<PlaceDTO>(placeToAdd);
            response.Message = "Place updated successfully.";

            return response;
        }
    }
}
