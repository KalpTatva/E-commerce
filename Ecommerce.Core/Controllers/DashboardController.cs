using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Core.Controllers;

public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    public DashboardController( IUserService userService, IOrderService orderService)
    {
        _userService = userService;
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        };        
        return View(baseViewModel);
    }

   
    public IActionResult UserDashboard()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        };        
        return View(baseViewModel);
    }
   
   
    public IActionResult EditProfile()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        EditRegisteredUserViewModel? model = _userService.GetUserDetailsByEmail(email ?? ""); 
        if(model!=null)
        {
            model.BaseEmail = email;
            model.BaseRole = role;
        }   
        return View(model);
    }

    public IActionResult EditUser(EditRegisteredUserViewModel model)
    {
        try
        {
            ResponsesViewModel responses = _userService.EditUserDetails(model);
            if(responses.IsSuccess)
            {
                return Json(new {success= true,message=responses.Message});
            }
            return Json(new {success= false,message=responses.Message});

        }
        catch(Exception e)
        {
            return Json(new{success = false, message=e.Message});
        }
    }


    [Authorize(Roles ="Buyer")]
    public IActionResult MyOrders()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        };        
        return View(baseViewModel);
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value; 

        List<MyOrderViewModel>? model = await _orderService.GetMyOrderHistoryByEmail(email ?? ""); 
        OrderAtMyOrderViewModel result = new ();
        result.BaseEmail = email;
        result.BaseRole = role;
        if(model!=null)
        {
            result.myOrderViewModels = model;
        }
        return PartialView("_MyOrdersPartial", result);
    }
}
