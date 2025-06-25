
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    // middleware details
    // here the below condition checks 
    // if the user is authenticated permanently (persistent connection) the cookie will generated 
    // with jwt token and session will generated with jwt token
    // if the user is authenticated temporarily (non-persistent connection) the cookie will not be generated
    // and session will be generated with jwt token
    // so we will check for the session variable first and then cookie
    // Get the JWT token from session or cookie
    // if the user is authenticated temporarily then no need to refresh the token into session
    // if the user is authenticated permanently then we will refresh the token into session and cookie

    // expiration details
    // jwt token expire is 60 minutes, so we will refresh the token if it is going to expire in next 5 minutes
    // for temporary session it will be expired in 30 minutes due to session expiration
    // for persistent connection it will be expired in 30 days due to cookie expiration 
    
    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            string? emailClaim = context.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value
                ?? context.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            
            string? roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
            
            string? token =  SessionUtils.GetSession(context, "auth_token") ?? CookieUtils.GetCookie(context, "auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
                    if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5) || CookieUtils.ContainsKey(context.Request,"auth_token")) // Refresh if expiring soon (within 5 minutes)
                    {
                        ResponseTokenViewModel? response = userService.RefreshToken(emailClaim, roleClaim);
                        if (response.token != null)
                        {
                            if (CookieUtils.ContainsKey(context.Request,"auth_token")) // MEANING persistent connection
                            {
                                // will directly append cookie for 30 day validity
                                CookieUtils.SetJwtCookie(context.Response, "auth_token", response.token);
                                // Set session variable for auth token
                                SessionUtils.SetSession(context, "auth_token", response.token);
                            }  
                            else
                            {
                                // Clear session and cookies on token error
                                SessionUtils.ClearSession(context);
                                CookieUtils.ClearCookies(context.Response, "auth_token");
                                context.Response.Redirect("/BuyerDashboard/Index");
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    // Clear session and cookies on token error
                    SessionUtils.ClearSession(context);
                    CookieUtils.ClearCookies(context.Response, "auth_token");
                    context.Response.Redirect("/BuyerDashboard/Index");
                    return;
                }
            }
            else
            {
                // Clear session and cookies if no token is found
                SessionUtils.ClearSession(context);
                CookieUtils.ClearCookies(context.Response, "auth_token");
                context.Response.Redirect("/BuyerDashboard/Index");
                return;
            }
        }

        await _next(context);
    }
}