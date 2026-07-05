using PremiumPlace.DTO;

namespace PremiumPlace_API.Services.Places
{
    public interface IPlaceService
    {
        Task<ServiceResponse<List<PlaceDTO>>> GetAllPlacesAsync();
        Task<ServiceResponse<PagedResult<PlaceDTO>>> SearchPlacesAsync(PlaceQueryDTO query);
        Task<ServiceResponse<List<string>>> GetCityNamesAsync();
        Task<ServiceResponse<PlaceDetailsDTO>> GetPlaceByIdAsync(int id);
        Task<ServiceResponse<PlaceOptionsDTO>> GetPlaceOptionsAsync();
        Task<ServiceResponse<PlaceDTO>> CreatePlaceAsync(PlaceCreateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> UpdatePlaceAsync(int id, PlaceUpdateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> UpdatePlacePartialAsync(int id, PlacePatchUpdateDTO placeDTO);
        Task<ServiceResponse<PlaceDTO>> DeletePlaceAsync(int id);
    }
}
