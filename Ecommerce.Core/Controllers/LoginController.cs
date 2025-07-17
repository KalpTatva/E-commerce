using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Core.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using static Ecommerce.Repository.Helpers.Enums;
using Ecommerce.Repository.Models;
using Ecommerce.Core.Utils;

namespace Ecommerce.Core.Controllers;

public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;
    private readonly IUserService _userService;
    public LoginController(ILogger<LoginController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }



    #region login

    /// <summary>
    /// index method for redirection based on cookie and sessions, returns login view
    /// </summary>
    /// <returns>View</returns>
    public IActionResult Index(string? ReturnURL = null)
    {
        try
        { 
            string? authToken = null;
            string? cookieToken = CookieUtils.GetCookie(HttpContext, "auth_token");
            if (HttpContext.Session.TryGetValue("auth_token", out byte[]? sessionToken))
            {
                authToken = System.Text.Encoding.UTF8.GetString(sessionToken);
            }
            else if (cookieToken != null)
            {
                authToken = cookieToken;
            }

            if (string.IsNullOrEmpty(authToken) || User?.Identity?.IsAuthenticated != true)
            {
                SessionUtils.ClearSession(HttpContext);
                CookieUtils.ClearCookies(Response, "auth_token");

                LoginViewModel model = new (){
                    ReturnURL = ReturnURL
                };

                return View(model);
            }


            string? role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            switch (role)
            {
                case nameof(RoleEnum.Admin):
                    return RedirectToAction("Admin", "Dashboard");
                case nameof(RoleEnum.Seller):
                    return RedirectToAction("UserDashboard", "Dashboard");
                case nameof(RoleEnum.Buyer):
                    return RedirectToAction("Index", "BuyerDashboard");
                default:
                    LoginViewModel model = new (){
                        ReturnURL = ReturnURL
                    };
                    SessionUtils.ClearSession(HttpContext);
                    CookieUtils.ClearCookies(Response, "auth_token");
                    return View(model);          
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
        if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            TempData["ErrorMessage"] = "Email and Password are required.";
            return View(model);
        }

        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid user credentials";
                return View(model);
            }
            ResponseTokenViewModel? response = _userService.UserLogin(model);
            if (response.token != null )
            {
                TempData["SuccessMessage"] = "User logged in successfully!";
                if (response.isPersistent)
                {
                    CookieUtils.SetJwtCookie(Response, "auth_token", response.token);
                    SessionUtils.SetSession(HttpContext, "auth_token", response.token);    
                }
                else
                {
                    SessionUtils.SetSession(HttpContext, "auth_token", response.token);
                    
                    // for Return URL
                    string? cookieToken = CookieUtils.GetCookie(HttpContext, "previous_user");
                    if(!string.IsNullOrEmpty(cookieToken) && cookieToken == response.UserName && !string.IsNullOrEmpty(model.ReturnURL))
                    {
                        string decryptedUrl = AesEncryptionHelper.DecryptString(model.ReturnURL);
                        if(Url.IsLocalUrl(decryptedUrl))
                        {
                            CookieUtils.SetCookie(HttpContext,"previous_user", response.UserName ?? "");
                            return Redirect(decryptedUrl);
                        }
                    }

                    CookieUtils.SetCookie(HttpContext,"previous_user", response.UserName ?? "");
                }

                // setup theme cookie
                CookieUtils.SetThemeCookie(HttpContext, "theme", response.BaseTheme ?? "system");
                
                return RedirectToAction("Index", "Login");
            }
            TempData["ErrorMessage"] = "Invalid user credentials, please try again!";
            return View(model);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }

    /// <summary>
    /// logout method for clearing cookies and session and redirect to login page
    /// </summary>
    /// <returns>redirect to login view</returns>
    public IActionResult Logout()
    {
        try
        {
            // Clear session and cookies
            SessionUtils.ClearSession(HttpContext);
            CookieUtils.ClearCookies(Response, "auth_token");
            CookieUtils.ClearCookies(Response, "previous_user");
            CookieUtils.ClearCookies(Response,"theme");
    
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Index", "Login"); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during logout.");
            TempData["ErrorMessage"] = "An error occurred during logout. Please try again.";
            return RedirectToAction("Index", "Login");
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
            return View(model);
        }
        try
        {

            ResponsesViewModel response = await _userService.ForgotPassword(model!);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index", "Login");
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
    public async Task<ActionResult> ResetPassword(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("ForgotPassword","Login");
            }
            ResponsesViewModel response = await _userService.ValidateResetPasswordToken(token);
            if (response.IsSuccess)
            {
                return View(new ForgetPasswordViewModel { Token = token, Email = response.Message });
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                TempData.Remove("SuccessMessage");
                return RedirectToAction("ForgotPassword","Login");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating the reset password token.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return RedirectToAction("ForgotPassword","Login");
        }
    }

    /// <summary>
    /// Reset password post method for reseting new password
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Resset password view</returns>
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ForgetPasswordViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            TempData["ErrorMessage"] = "All fields are required.";
            return View(model);
        }
        try
        {
            ResponsesViewModel response = await _userService.ResetPassword(model);
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
    public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid user details. Please check your input as per Guidlines.";
                return View(model);
            }

            ResponsesViewModel response = await _userService.RegisterUser(model);
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
    public async Task<IActionResult> GetCountries()
    {
        try
        {
            List<Country>? countries = await _userService.GetCountries();
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
    public async Task<IActionResult> GetStates(int countryId)
    {
        try
        {
            List<State>? states = await _userService.GetStates(countryId);
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
    public async Task<IActionResult> GetCities(int stateId)
    {
        try
        {
            List<City>? cities = await _userService.GetCities(stateId);
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

