using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ecommerce.Core.Controllers;

public class OrderController : Controller
{
    private readonly IProductService _productService;
    public OrderController(IProductService productService)
    {
        _productService = productService;
    }


    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public IActionResult Index(string orders,decimal TotalPrice,decimal TotalDiscount,decimal TotalQuantity)
    {
        List<int>? CategoryIdList = string.IsNullOrEmpty(orders) 
            ? new List<int>() 
            : JsonConvert.DeserializeObject<List<int>>(orders);
        

        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        }; 



        return View(baseViewModel);
    }

}
