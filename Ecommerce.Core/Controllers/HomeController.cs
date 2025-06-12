using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Core.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Core.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

     #region login
    public IActionResult Index()
    {
        try
        {
            string? authToken = null;
            string? sessionId = null;

            // Check for session ID in cookies
            if (Request.Cookies.ContainsKey("session_id"))
            {
                sessionId = Request.Cookies["session_id"];
            }

            // Check for authentication token in session
            else if (HttpContext.Session.TryGetValue("auth_token", out var sessionToken))
            {
                authToken = System.Text.Encoding.UTF8.GetString(sessionToken);
            }

            if (!string.IsNullOrEmpty(authToken) && !string.IsNullOrEmpty(sessionId) && User?.Identity?.IsAuthenticated == true)
            {
                // Retrieve session details based on session ID
                // var sessionDetails = _userService.GetSessionDetails(sessionId);
                // if (sessionDetails == null)
                // {
                //     TempData["ErrorMessage"] = "Session details not found. Please log in again.";
                //     return View();
                // }

                // Retrieve user role from claims
                string? role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (role == RoleEnum.Admin.ToString())
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (role == RoleEnum.Buyer.ToString())
                {
                    return RedirectToAction("", "Dashboard");
                }
                else if (role == RoleEnum.Seller.ToString())
                {
                    return RedirectToAction("", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid user role. Please contact support.";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
        catch{
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            return View();
        }
    }

    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid user credentials";
                return View(model);
            }
            ResponseTokenViewModel? response = _userService.UserLogin(model);
            if (response.token != null)
            {
                TempData["SuccessMessage"] = "User logged in successfully!";

                if (response.isPersistent)
                {
                    // Store session ID in a secure cookie for "Remember Me"
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    };
                    Response.Cookies.Append("session_id", response.sessionId ?? "", cookieOptions);

                    // Store JWT token in session
                    HttpContext.Session.SetString("auth_token", response.token);
                }
                else
                {
                    // Store JWT token in session only
                    HttpContext.Session.SetString("auth_token", response.token);
                }

                return RedirectToAction("User2FaAuth", new { Email = model.Email });
            }

            TempData["ErrorMessage"] = "Invalid user credentials!";
            return View(model);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }

    #endregion


    // error views
    public IActionResult Error403()
    {
        _logger.LogWarning("403 Forbidden error occurred.");
        return View();
    }
    public IActionResult Error404()
    {
        _logger.LogWarning("404 Not Found error occurred.");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
