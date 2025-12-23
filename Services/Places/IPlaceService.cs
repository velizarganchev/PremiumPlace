using PremiumPlace_API.Models;
using PremiumPlace_API.Models.DTO;

namespace PremiumPlace_API.Services.Places
{
    public interface IPlaceService
    {
        Task<ServiceResponse<List<PlaceDTO>>> GetAllPlaces();
        Task<ServiceResponse<PlaceDTO>> GetPlaceById(int id);
        Task<ServiceResponse<PlaceDTO>> CreatePlace(PlaceCreateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> UpdatePlace(int id, PlaceUpdateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> DeletePlace(int id);
    }
}
