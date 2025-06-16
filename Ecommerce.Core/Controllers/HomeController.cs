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
    /// <returns></returns>
    public IActionResult Index()
    {
        try
        {
            string? authToken = null;
            string? sessionId = null;

            // Check for session ID in cookies
            if (CookieUtils.ContainsKey(Request, "session_id"))
            {
                sessionId = Request.Cookies["session_id"];
                Session? sessionDetails = _userService.GetSessionDetails(sessionId ?? "");
                if (sessionDetails == null || !sessionDetails.IsActive || sessionDetails.ExpiresAt < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    // Clear invalid session data
                    HttpContext.Session.Clear();
                    CookieUtils.ClearCookies(Response, "session_id");
                    CookieUtils.ClearCookies(Response, "auth_token");
                    return View();
                }
            }

            // Check for auth token in cookies or session
            string? cookieToken = CookieUtils.GetCokkieData(Request, "auth_token");
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
                HttpContext.Session.Clear();
                CookieUtils.ClearCookies(Response, "auth_token");
                return View();
            }

            // Redirect based on role
            string? role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == RoleEnum.Admin.ToString())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (role == RoleEnum.Buyer.ToString() || role == RoleEnum.Seller.ToString())
            {
                return RedirectToAction("UserDashboard", "Dashboard");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid user role. Please contact support.";
                HttpContext.Session.Clear();
                CookieUtils.ClearCookies(Response, "session_id");
                CookieUtils.ClearCookies(Response, "auth_token");
                return View();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            HttpContext.Session.Clear();
            CookieUtils.ClearCookies(Response, "session_id");
            CookieUtils.ClearCookies(Response, "auth_token");
            return View();
        }
    }


    /// <summary>
    /// post method for login 
    /// </summary>
    /// <param name="model">LoginViewModel</param>
    /// <returns></returns>
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
                    // Store session ID and JWT in secure cookies
                    CookieUtils.SetJwtCookie(Response, "session_id", response.sessionId ?? "");
                    CookieUtils.SetJwtCookie(Response, "auth_token", response.token);// Store JWT in cookie

                }
                else
                {
                    // Storing JWT in session only for non-persistent login
                    HttpContext.Session.SetString("auth_token", response.token);
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
    /// <returns></returns>
    public IActionResult Logout()
    {
        try
        {
            // Invalidate session in database if stored
            string? sessionId = CookieUtils.GetCokkieData(Request, "session_id");
            if (sessionId != null)
            {
                Session? session = _userService.GetSessionDetails(sessionId);
                if (session != null)
                {
                    session.IsActive = false;
                    _userService.UpdateSession(session);
                }
            }

            // Clear session and cookies
            HttpContext.Session.Clear();
            CookieUtils.ClearCookies(Response, "session_id");
            CookieUtils.ClearCookies(Response, "auth_token");

            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("UserDashboard", "Dashboard"); // Redirect to login page
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
    public IActionResult ForgotPassword()
    {
        return View();
    }

    /// <summary>
    /// forgot password post method, which generates reset password link 
    /// </summary>
    /// <param name="model">EmailViewModel</param>
    /// <returns></returns>
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
    /// <returns></returns>
    [HttpGet]
    public ActionResult ResetPassword(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("ForgetPassword");
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
                return RedirectToAction("ForgetPassword");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating the reset password token.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return RedirectToAction("ForgetPassword");
        }
    }

    /// <summary>
    /// Reset password post method for reseting new password
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
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
    /// <returns></returns>
    public IActionResult RegisterUser()
    {
        return View();
    }

    /// <summary>
    /// register user post method, which registers a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <returns></returns>.
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
    /// <returns></returns>
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

