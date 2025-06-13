using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Repository.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Core.Controllers;

public class DashboardController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult EditProfile()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            Email = email
        };        
        return View(baseViewModel);
    }
   
    public IActionResult UserDashboard()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            Email = email
        };        
        return View(baseViewModel);
    }
}
