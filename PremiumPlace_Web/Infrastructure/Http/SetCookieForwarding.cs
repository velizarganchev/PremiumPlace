namespace PremiumPlace_Web.Infrastructure.Http
{
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
