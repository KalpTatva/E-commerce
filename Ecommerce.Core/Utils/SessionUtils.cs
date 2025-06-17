namespace Ecommerce.Core.Utils;

public class SessionUtils
{

    /// <summary>
    /// Method to store user details in session
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="user"></param>
    public static void SetSession(HttpContext httpContext,string sessionName, string payLoad )
    {
        httpContext.Session.SetString(sessionName, payLoad);        
    }

    /// <summary>
    /// Method to clear all Session data
    /// </summary>
    /// <param name="httpContext"></param>
    public static void ClearSession(HttpContext httpContext) => httpContext.Session.Clear();
}
