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
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };
        response.Cookies.Append(cokkieName, payload, cookieOptions);
    }



    ///<summary>
    /// Method for clearing cookie
    ///</summary>
    ///<param name="httpContext"></param>
    ///<param name="CokkieName"></param>
    public static void ClearCookies(HttpResponse response,string CokkieName)
    {
       response.Cookies.Delete(CokkieName);
    }

    /// <summary>
    /// Method for getting cokkie deta
    /// </summary>
    /// <param name="request"></param>
    /// <param name="CokkieName"></param>
    /// <returns></returns>
    public static string? GetCokkieData(HttpRequest request, string CokkieName)
    {
        if (request.Cookies.TryGetValue(CokkieName, out var cookieValue))
        {
            return cookieValue;
        }
        return null;
    }
}
