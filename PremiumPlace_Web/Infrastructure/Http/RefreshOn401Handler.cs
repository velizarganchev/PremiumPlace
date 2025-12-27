using System.Net;

namespace PremiumPlace_Web.Infrastructure.Http
{
    /// <summary>
    /// If the API returns 401, we try to refresh tokens via /api/auth/refresh and then retry the original request once.
    /// This keeps UI logic clean: the browser just calls MVC, and MVC guarantees valid cookies when possible.
    /// </summary>
    public sealed class RefreshOn401Handler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Prevent multiple parallel refresh calls (e.g. 5 parallel requests all get 401).
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);

        // Used to avoid infinite loops: do not refresh when calling refresh endpoint itself.
        private static readonly string RefreshPath = "/api/refresh";

        public RefreshOn401Handler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1) Send original request
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            // If this is already a refresh call, don't attempt refresh again.
            if (request.RequestUri is not null &&
                request.RequestUri.AbsolutePath.Equals(RefreshPath, StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }

            // 2) Attempt refresh (single-flight)
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                // Another request might already have refreshed tokens while we waited.
                // Try one more time quickly: if cookies were updated, retry may succeed.
                response.Dispose();
                var retry1 = await base.SendAsync(await CloneHttpRequestMessageAsync(request), cancellationToken);
                if (retry1.StatusCode != HttpStatusCode.Unauthorized)
                    return retry1;

                retry1.Dispose();

                // 3) Call refresh endpoint
                var baseUri = new Uri(request.RequestUri!.GetLeftPart(UriPartial.Authority));
                var refreshUri = new Uri(baseUri, RefreshPath);
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, refreshUri);

                var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

                // Forward Set-Cookie from API refresh to browser (MVC response)
                ForwardSetCookiesToBrowser(refreshResponse);

                if (!refreshResponse.IsSuccessStatusCode)
                {
                    // Refresh failed -> return original 401 behavior
                    return refreshResponse;
                }

                refreshResponse.Dispose();

                // 4) Retry original request once
                var retry2 = await base.SendAsync(await CloneHttpRequestMessageAsync(request), cancellationToken);
                return retry2;
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        private void ForwardSetCookiesToBrowser(HttpResponseMessage apiResponse)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return;

            if (apiResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var v in setCookieValues)
                    ctx.Response.Headers.Append("Set-Cookie", v);
            }
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            // Copy headers
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copy content (if any)
            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;

                var newContent = new StreamContent(ms);
                foreach (var header in request.Content.Headers)
                    newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

                clone.Content = newContent;
            }

            return clone;
        }
    }
}
