namespace Ecommerce.Core.Utils;

public class CookieUtils
{
    /// <summary>
    /// for cheking if cookie exists or not
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cookieName"></param>
    /// <returns></returns>
    public static bool ContainsKey(HttpRequest request, string cookieName)
    {
        return request.Cookies.ContainsKey(cookieName);
    }

    /// <summary>
    /// setting cokkies data (auth_token, sessionid)
    /// </summary>
    /// <param name="response"></param>
    /// <param name="cokkieName"></param>
    /// <param name="payload"></param>
    public static void SetJwtCookie(HttpResponse response, string cokkieName, string payload)
    {
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(2)
        };
        response.Cookies.Append(cokkieName, payload, cookieOptions);
    }

    /// <summary>
    /// Method for clearing cookie
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="CokkieName"></param>
    public static void ClearCookies(HttpResponse response, string CokkieName)
    {
        response.Cookies.Delete(CokkieName);
    }

    /// <summary>
    /// Method to store user details in cookies
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="cookieName"></param>
    /// <param name="payLoad"></param>
    public static void SetCookie(HttpContext httpContext, string cookieName, string payLoad, int expirationMinutes = 30)
    {
        httpContext.Response.Cookies.Append(cookieName, payLoad, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        });
    }

    /// <summary>
    /// Method for getting data from cookies
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="cookieName"></param>
    /// <returns></returns>
    public static string? GetCookie(HttpContext httpContext, string cookieName)
    {
        httpContext.Request.Cookies.TryGetValue(cookieName, out string? value);
        return value;
    }

    /// <summary>
    /// Remove cookie by name
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="cookieName"></param>
    public static void RemoveCookie(HttpContext httpContext, string cookieName)
    {
        httpContext.Response.Cookies.Delete(cookieName);
    }
}
