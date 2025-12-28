using PremiumPlace.DTO.Auth;
using PremiumPlace_Web.Application.Results;

namespace PremiumPlace_Web.Application.Abstractions.Api
{
    public interface IAuthApi
    {
        Task<ApiResult<AuthResponseDTO>> LoginAsync(LoginRequestDTO dto, CancellationToken ct = default);
        Task<ApiResult<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO dto, CancellationToken ct = default);
        Task<ApiResult> LogoutAsync(CancellationToken ct = default);
        Task<ApiResult<AuthResponseDTO>> MeAsync(CancellationToken ct = default);
        Task<ApiResult> DeleteMeAsync(DeleteMeRequestDTO dto, CancellationToken ct = default);
    }
}
