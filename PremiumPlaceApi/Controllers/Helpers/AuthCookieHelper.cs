namespace PremiumPlace_API.Controllers.Helpers
{
    public class AuthCookieHelper
    {
        public const string AccessCookieName = "pp_access";
        public const string RefreshCookieName = "pp_refresh";

        public static void SetAuthCookies(
            HttpResponse response,
            string accessToken,
            string refreshToken,
            int accessTokenMinutes,
            int refreshTokenDays)
        {
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 // API е https
                SameSite = SameSiteMode.None,  // Angular е друг origin (http://localhost:4200)
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(accessTokenMinutes)
            };

            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenDays)
            };

            response.Cookies.Append(AccessCookieName, accessToken, accessOptions);
            response.Cookies.Append(RefreshCookieName, refreshToken, refreshOptions);
        }

        public static void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Append(AccessCookieName, "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UnixEpoch
            });

            response.Cookies.Append(RefreshCookieName, "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UnixEpoch
            });
        }
    }
}
