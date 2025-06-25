using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Core.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using static Ecommerce.Repository.Helpers.Enums;
using Ecommerce.Repository.Models;
using Ecommerce.Core.Utils;

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

    /// <summary>
    /// index method for redirection based on cookie and sessions, returns login view
    /// </summary>
    /// <returns>View</returns>
    public IActionResult Index()
    {
        try
        {
            string? authToken = null;

            // Check for auth token in cookies or session
            string? cookieToken = CookieUtils.GetCookie(HttpContext, "auth_token");
            if (HttpContext.Session.TryGetValue("auth_token", out byte[]? sessionToken))
            {
                authToken = System.Text.Encoding.UTF8.GetString(sessionToken);
            }
            else if (cookieToken != null)
            {
                authToken = cookieToken;
            }

            // If no valid token or user is not authenticated, return login view
            if (string.IsNullOrEmpty(authToken) || User?.Identity?.IsAuthenticated != true)
            {
                SessionUtils.ClearSession(HttpContext);
                CookieUtils.ClearCookies(Response, "auth_token");
                return View();
            }

            string? role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            // Redirect based on role
            switch (role)
            {
                case nameof(RoleEnum.Admin):
                    return RedirectToAction("Index", "BuyerDashboard");
                case nameof(RoleEnum.Seller):
                    return RedirectToAction("UserDashboard", "Dashboard");
                case nameof(RoleEnum.Buyer):
                    return RedirectToAction("Index", "BuyerDashboard");
                default:
                    TempData["ErrorMessage"] = "Invalid user role. Please contact support.";
                    SessionUtils.ClearSession(HttpContext);
                    CookieUtils.ClearCookies(Response, "auth_token");
                    return View();
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            SessionUtils.ClearSession(HttpContext);
            CookieUtils.ClearCookies(Response, "auth_token");
            return View();
        }
    }


    /// <summary>
    /// post method for login 
    /// </summary>
    /// <param name="model">LoginViewModel</param>
    /// <returns>View</returns>
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

                // Store JWT token and session ID
                if (response.isPersistent)
                {
                    // Store JWT in cookie (temporary solution)
                    CookieUtils.SetJwtCookie(Response, "auth_token", response.token);
                    // Set session in HttpContext for persistent login
                    SessionUtils.SetSession(HttpContext, "auth_token", response.token);
                }
                else
                {
                    // Storing JWT in session only for non-persistent login
                    SessionUtils.SetSession(HttpContext,"auth_token", response.token);
                }

                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Invalid user credentials, please try again !";
            return View(model);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }

    /// <summary>
    /// logout method for clearing cookies and session and redirect to home
    /// </summary>
    /// <returns>redirect to buyer dashboard</returns>
    public IActionResult Logout()
    {
        try
        {
            // Invalidate session in database if stored
            // Clear session and cookies
            SessionUtils.ClearSession(HttpContext);
            CookieUtils.ClearCookies(Response, "auth_token");

            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Index", "BuyerDashboard"); 
        }
        catch
        {
            TempData["ErrorMessage"] = "An error occurred during logout. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion
    #region Forgot Password

    /// <summary>
    /// forgot password get method, returns view
    /// </summary>
    /// <returns>view</returns>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    /// <summary>
    /// forgot password post method, which generates reset password link 
    /// </summary>
    /// <param name="model">EmailViewModel</param>
    /// <returns>View</returns>
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(EmailViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.ToEmail))
        {
            TempData["ErrorMessage"] = "Email address is required.";
        }
        try
        {

            ResponsesViewModel response = await _userService.ForgotPassword(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }


        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }


    /// <summary>
    /// Reset password get method which ensures validity of reset password link
    /// </summary>
    /// <param name="token"></param>
    /// <returns>redirect to forget password</returns>
    [HttpGet]
    public ActionResult ResetPassword(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("ForgotPassword","Home");
            }
            ResponsesViewModel response = _userService.ValidateResetPasswordToken(token);
            if (response.IsSuccess)
            {
                return View(new ForgetPasswordViewModel { Token = token, Email = response.Message });
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                TempData.Remove("SuccessMessage");
                return RedirectToAction("ForgotPassword","Home");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating the reset password token.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return RedirectToAction("ForgotPassword","Home");
        }
    }

    /// <summary>
    /// Reset password post method for reseting new password
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Resset password view</returns>
    [HttpPost]
    public IActionResult ResetPassword(ForgetPasswordViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            TempData["ErrorMessage"] = "All fields are required.";
            return View(model);
        }
        try
        {
            ResponsesViewModel response = _userService.ResetPassword(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while resetting the password.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }



    #endregion

    #region Register User

    /// <summary>
    /// register user get method
    /// </summary>
    /// <returns>View</returns>
    public IActionResult RegisterUser()
    {
        return View();
    }

    /// <summary>
    /// register user post method, which registers a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns>View</returns>
    [HttpPost]
    public IActionResult RegisterUser(RegisterUserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid user details. Please check your input.";
                return View(model);
            }

            ResponsesViewModel response = _userService.RegisterUser(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }


    /// <summary>
    /// GetCountries method to fetch list of countries for registration
    /// </summary>
    /// <returns>Json</returns>
    // helpers
    [HttpGet]
    public IActionResult GetCountries()
    {
        try
        {
            List<Country>? countries = _userService.GetCountries();
            return PartialView("_countryoptionsPartial", countries);
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }

    /// <summary>
    /// GetStates method to fetch list of states based on selected country
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns>Json</returns>.
    [HttpGet]
    public IActionResult GetStates(int countryId)
    {
        try
        {
            List<State>? states = _userService.GetStates(countryId);
            return PartialView("_stateoptionPartial", states);
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }

    /// <summary>
    /// GetCities method to fetch list of cities based on selected state
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns>Json</returns>
    [HttpGet]
    public IActionResult GetCities(int stateId)
    {
        try
        {
            List<City>? cities = _userService.GetCities(stateId);
            return PartialView("_cityoptionPartial", cities);
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }

    #endregion


    #region Error pages

    // error views //
    public IActionResult Error403()
    {
        _logger.LogWarning("403 Forbidden error occurred.");
        return View();
    }
    public IActionResult Error401()
    {
        _logger.LogWarning("401 Unauthorized error occurred.");
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
    #endregion

}

