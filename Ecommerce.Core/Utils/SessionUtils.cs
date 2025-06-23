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
    /// Method for getting session from session data from its session id name
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="sessionName"></param>
    /// <returns></returns>
    public static string? GetSession(HttpContext httpContext,string sessionName)
    {
        return httpContext.Session.GetString(sessionName);  
    } 
    
    /// <summary>
    /// remove session data by session id
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="sessionName"></param>
    public static void RemoveSessionById(HttpContext httpContext,string sessionName)
    {
        httpContext.Session.Remove("sessionName");
    }

    /// <summary>
    /// Method to clear all Session data
    /// </summary>
    /// <param name="httpContext"></param>
    public static void ClearSession(HttpContext httpContext) => httpContext.Session.Clear();
}
