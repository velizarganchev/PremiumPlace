using Microsoft.Net.Http.Headers;

namespace PremiumPlace_Web.Infrastructure.Http
{
    public sealed class PremiumPlaceApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        public PremiumPlaceApiClient(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void AttachAuthCookies(HttpRequestMessage req, string? accessOverride = null, string? refreshOverride = null)
        {
            var httpCtx = _ctx.HttpContext;
            if (httpCtx is null) return;

            // Use override cookies if provided (e.g. right after refresh inside same request)
            var access = accessOverride ?? (httpCtx.Request.Cookies.TryGetValue("pp_access", out var a) ? a : null);
            var refresh = refreshOverride ?? (httpCtx.Request.Cookies.TryGetValue("pp_refresh", out var r) ? r : null);

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(access)) parts.Add($"pp_access={access}");
            if (!string.IsNullOrWhiteSpace(refresh)) parts.Add($"pp_refresh={refresh}");

            if (parts.Count > 0)
            {
                req.Headers.Remove(HeaderNames.Cookie);
                req.Headers.Add(HeaderNames.Cookie, string.Join("; ", parts));
            }
        }

        private static (string? access, string? refresh) ExtractAuthCookiesFromSetCookie(HttpResponseMessage resp)
        {
            if (!resp.Headers.TryGetValues("Set-Cookie", out var values))
                return (null, null);

            string? access = null;
            string? refresh = null;

            foreach (var v in values)
            {
                // very simple parse: "pp_access=...; Path=/; HttpOnly; ..."
                if (v.StartsWith("pp_access=", StringComparison.OrdinalIgnoreCase))
                    access = v.Substring("pp_access=".Length).Split(';')[0];

                if (v.StartsWith("pp_refresh=", StringComparison.OrdinalIgnoreCase))
                    refresh = v.Substring("pp_refresh=".Length).Split(';')[0];
            }

            return (access, refresh);
        }

        public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body, CancellationToken ct = default)
            => SendAsync(() =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = JsonContent.Create(body)
                };

                // For login/register you usually DON'T have cookies yet - that's fine.
                AttachAuthCookies(req);
                return req;
            }, ct, allowRefreshRetry: false);

        public Task<HttpResponseMessage> GetAsync(string path, CancellationToken ct = default)
            => SendAsync(() =>
            {
                var req = new HttpRequestMessage(HttpMethod.Get, path);
                AttachAuthCookies(req);
                return req;
            }, ct, allowRefreshRetry: true);

        public Task<HttpResponseMessage> DeleteJsonAsync<T>(string path, T body, CancellationToken ct = default)
            => SendAsync(() =>
            {
                var req = new HttpRequestMessage(HttpMethod.Delete, path)
                {
                    Content = JsonContent.Create(body)
                };
                AttachAuthCookies(req);
                return req;
            }, ct, allowRefreshRetry: true);

        private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> build, CancellationToken ct, bool allowRefreshRetry)
        {
            var httpCtx = _ctx.HttpContext;

            // 1) original
            var resp = await _http.SendAsync(build(), ct);
            if (!allowRefreshRetry || resp.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                return resp;

            resp.Dispose();

            // 2) refresh (must send pp_refresh from incoming request)
            var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
            AttachAuthCookies(refreshReq); // праща стария pp_refresh

            var refreshResp = await _http.SendAsync(refreshReq, ct);
            // IMPORTANT: forward Set-Cookie to browser so the browser stores new cookies
            if (httpCtx is not null)
                SetCookieForwarding.CopySetCookieHeaders(refreshResp, httpCtx.Response);

            if (!refreshResp.IsSuccessStatusCode)
                return refreshResp;

            // Extract fresh cookies for retry inside SAME request
            var (newAccess, newRefresh) = ExtractAuthCookiesFromSetCookie(refreshResp);
            refreshResp.Dispose();

            // 3) retry once WITH OVERRIDES (otherwise you'll resend old cookies)
            var retryReq = build();
            AttachAuthCookies(retryReq, accessOverride: newAccess, refreshOverride: newRefresh);

            return await _http.SendAsync(retryReq, ct);
        }

        public void ForwardSetCookieToBrowser(HttpResponseMessage apiResp)
        {
            var httpCtx = _ctx.HttpContext;
            if (httpCtx is null) return;

            SetCookieForwarding.CopySetCookieHeaders(apiResp, httpCtx.Response);
        }

        public async Task<T> ReadJsonAsync<T>(HttpResponseMessage resp, CancellationToken ct)
        {
            var data = await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            if (data is null)
                throw new InvalidOperationException("API returned empty JSON body.");
            return data;
        }
    }

}
