using PremiumPlace.DTO.Auth;
using PremiumPlace_Web.Application.Results;
using PremiumPlace_Web.Infrastructure.Http;

namespace PremiumPlace_Web.Application.Abstractions.Api
{
    public class AuthApi : IAuthApi
    {
        private readonly PremiumPlaceApiClient _client;

        public AuthApi(PremiumPlaceApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResult<AuthResponseDTO>> LoginAsync(LoginRequestDTO dto, CancellationToken ct = default)
        {
            var resp = await _client.PostJsonAsync("/api/auth/login", dto, ct);

            // IMPORTANT: login/register/logout трябва да forward-нат Set-Cookie към браузъра
            _client.ForwardSetCookieToBrowser(resp);

            if (resp.IsSuccessStatusCode)
            {
                var data = await _client.ReadJsonAsync<AuthResponseDTO>(resp, ct);
                return ApiResult<AuthResponseDTO>.Ok(data, (int)resp.StatusCode);
            }

            var (msg, errors) = await ApiErrorParser.ParseAsync(resp, ct);
            return ApiResult<AuthResponseDTO>.Fail((int)resp.StatusCode, msg, errors);
        }

        public async Task<ApiResult<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO dto, CancellationToken ct = default)
        {
            var resp = await _client.PostJsonAsync("/api/auth/register", dto, ct);
            _client.ForwardSetCookieToBrowser(resp);

            if (resp.IsSuccessStatusCode)
            {
                var data = await _client.ReadJsonAsync<AuthResponseDTO>(resp, ct);
                return ApiResult<AuthResponseDTO>.Ok(data, (int)resp.StatusCode);
            }

            var (msg, errors) = await ApiErrorParser.ParseAsync(resp, ct);
            return ApiResult<AuthResponseDTO>.Fail((int)resp.StatusCode, msg, errors);
        }

        public async Task<ApiResult> LogoutAsync(CancellationToken ct = default)
        {
            var resp = await _client.PostJsonAsync("/api/auth/logout", new { }, ct);
            _client.ForwardSetCookieToBrowser(resp);

            if (resp.IsSuccessStatusCode)
                return ApiResult.Ok((int)resp.StatusCode);

            var (msg, errors) = await ApiErrorParser.ParseAsync(resp, ct);
            return ApiResult.Fail((int)resp.StatusCode, msg, errors);
        }

        public async Task<ApiResult<AuthResponseDTO>> MeAsync(CancellationToken ct = default)
        {
            // allowRefreshRetry е вътре в клиента (Get/Delete)
            var resp = await _client.GetAsync("/api/auth/me", ct);

            if (resp.IsSuccessStatusCode)
            {
                var data = await _client.ReadJsonAsync<AuthResponseDTO>(resp, ct);
                return ApiResult<AuthResponseDTO>.Ok(data, (int)resp.StatusCode);
            }

            var (msg, errors) = await ApiErrorParser.ParseAsync(resp, ct);
            return ApiResult<AuthResponseDTO>.Fail((int)resp.StatusCode, msg, errors);
        }

        public async Task<ApiResult> DeleteMeAsync(DeleteMeRequestDTO dto, CancellationToken ct = default)
        {
            var resp = await _client.DeleteJsonAsync("/api/auth/me", dto, ct);
            _client.ForwardSetCookieToBrowser(resp);

            if (resp.IsSuccessStatusCode)
                return ApiResult.Ok((int)resp.StatusCode);

            var (msg, errors) = await ApiErrorParser.ParseAsync(resp, ct);
            return ApiResult.Fail((int)resp.StatusCode, msg, errors);
        }
    }
}
