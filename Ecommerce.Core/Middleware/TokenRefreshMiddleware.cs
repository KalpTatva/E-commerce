
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            string? emailClaim = context.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? context.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            string? roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
            string? token = context.Request.Cookies["auth_token"] ?? context.Session.GetString("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
                if (jwtToken.ValidTo < DateTime.Now.AddMinutes(5)) // Refresh if expiring soon
                {
                    ResponseTokenViewModel? response = userService.RefreshToken(emailClaim, roleClaim);
                    if (response.token != null)
                    {
                        if (context.Request.Cookies.ContainsKey("session_id"))
                        {
                            CookieOptions cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddDays(30)
                            };
                            context.Response.Cookies.Append("auth_token", response.token, cookieOptions);
                        }
                        else
                        {
                            context.Session.SetString("auth_token", response.token);
                        }
                    }
                }
            }
        }
        await _next(context);
    }
}

