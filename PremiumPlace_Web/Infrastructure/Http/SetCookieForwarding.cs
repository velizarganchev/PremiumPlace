namespace PremiumPlace_Web.Infrastructure.Http
{
    /// <summary>
    /// Copies Set-Cookie headers from API response to the MVC response.
    /// This is critical in BFF mode so the browser stores cookies for the MVC origin (https://localhost:7073).
    /// </summary>
    public static class SetCookieForwarding
    {
        public static void CopySetCookieHeaders(HttpResponseMessage apiResponse, HttpResponse mvcResponse)
        {
            // HttpClient stores Set-Cookie in response headers (not in Cookies collection).
            if (apiResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                // Multiple Set-Cookie headers are allowed; append all.
                foreach (var v in setCookieValues)
                    mvcResponse.Headers.Append("Set-Cookie", v);
            }
        }
    }
}
