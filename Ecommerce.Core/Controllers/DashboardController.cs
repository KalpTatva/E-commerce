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

    /// <summary>
    /// Index method for all type of users (not logged in, seller, buyer)
    /// </summary>
    /// <returns>View with base view model</returns>
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

    /// <summary>
    /// Index method for user dashboard
    /// </summary>
    /// <returns>View with base view model</returns>
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
   
    /// <summary>
    /// Method to edit user profile
    /// </summary>
    /// <returns>View with user details</returns>   
    [Authorize]
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

    /// <summary>
    /// Method to edit user profile details
    /// </summary>
    /// <param name="model">Model containing user details</param>
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


    /// <summary>
    /// Method to view user's favourite products
    /// </summary>
    /// <returns>View with base view model</returns>
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

    /// <summary>
    /// Method to get user's order history
    /// </summary>
    /// <returns>Partial view with user's order history</returns>
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

    /// <summary>
    /// Method to view seller's order list
    /// </summary>
    [Authorize(Roles = "Seller")]
    public IActionResult SellerOrderList()
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

    /// <summary>
    /// Method to get seller's orders
    /// </summary>
    [Authorize(Roles = "Seller")]
    [HttpGet]
    public async Task<IActionResult> GetSellerOrders()
    {   
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
            ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        List<SellerOrderViewModel>? model = await _orderService.GetSellerOrders(email ?? "");

        SellerOrderListViewModel sellerOrderListViewModel = new SellerOrderListViewModel
        {
            BaseEmail = email,
            BaseRole = role,
            SellerOrders = model ?? new List<SellerOrderViewModel>()
        };
        return PartialView("_SellerOrderList", sellerOrderListViewModel);   
    }

}
