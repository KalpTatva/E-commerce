using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.Core.Utils;

public class BaseValues
{
    public static string? GetEmail(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
               ?? httpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
    }

    public static string? GetRole(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    }
    public static string? GetUserName(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value
                ?? httpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Name)?.Value;
    }
}
