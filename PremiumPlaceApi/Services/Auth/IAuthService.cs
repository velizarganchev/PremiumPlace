
using PremiumPlace.DTO.Auth;

namespace PremiumPlace_API.Services.Auth

{
    public interface IAuthService
    {
        Task<ServiceResponse<AuthResultDTO>> RegisterAsync(RegisterRequestDTO dto, string ip, string? userAgent);
        Task<ServiceResponse<AuthResultDTO>> LoginAsync(LoginRequestDTO dto, string ip, string? userAgent);
        Task<ServiceResponse<AuthResultDTO>> RefreshAsync(string refreshToken, string ip, string? userAgent);
        Task<ServiceResponse<bool>> LogoutAsync(string refreshToken, string ip);
        Task<ServiceResponse<bool>> DeleteMeAsync(int userId, string password, string ip);

    }
}
