using PremiumPlace.DTO;

namespace PremiumPlace_API.Services.Places
{
    public interface IPlaceService
    {
        Task<ServiceResponse<List<PlaceDTO>>> GetAllPlacesAsync();
        Task<ServiceResponse<PlaceDTO>> GetPlaceByIdAsync(int id);
        Task<ServiceResponse<PlaceDTO>> CreatePlaceAsync(PlaceCreateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> UpdatePlaceAsync(int id, PlaceUpdateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> DeletePlaceAsync(int id);
    }
}
