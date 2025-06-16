using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Core.Controllers;

public class DashboardController : Controller
{
    private readonly IUserService _userService;
    public DashboardController( IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult EditProfile()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        
        EditRegisteredUserViewModel? model = _userService.GetUserDetailsByEmail(email);    
        model.BaseEmail = email;
        return View(model);
    }
   
    public IActionResult UserDashboard()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        };        
        return View(baseViewModel);
    }
}
