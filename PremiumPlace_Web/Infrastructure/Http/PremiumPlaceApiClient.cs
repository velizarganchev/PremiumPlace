namespace PremiumPlace_Web.Infrastructure.Http
{
    /// <summary>
    /// Thin wrapper around HttpClient for PremiumPlace API.
    /// Keeps controllers clean and centralizes API calls.
    /// </summary>
    public sealed class PremiumPlaceApiClient
    {
        private readonly HttpClient _http;

        public PremiumPlaceApiClient(HttpClient http)
        {
            _http = http;
        }

        public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body, CancellationToken ct = default)
            => _http.PostAsJsonAsync(path, body, ct);

        public Task<HttpResponseMessage> GetAsync(string path, CancellationToken ct = default)
            => _http.GetAsync(path, ct);

        public Task<HttpResponseMessage> DeleteJsonAsync<T>(string path, T body, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, path)
            {
                Content = JsonContent.Create(body)
            };
            return _http.SendAsync(req, ct);
        }
    }
}
