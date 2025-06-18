using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Core.Controllers;

public class BuyerDashboardController : Controller
{

    private readonly IProductService _productService;
    public BuyerDashboardController(IProductService productService)
    {
        _productService = productService;
    }

    
    public IActionResult Index()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }


    [HttpGet]
    public async Task<IActionResult> GetProducts(string? search = null, int? category = null)
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
    
        ProductsViewModel model = await _productService.GetProducts(search, category);
        List<int> favourites = _productService.GetFavouritesByEmail(email);
        model.favourites = favourites;
        return PartialView("_productsCardPartial",model);
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> GetFavouriteProducts()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
    
        ProductsViewModel model = await _productService.GetFavouriteProducts(email);
        List<int> favourites = _productService.GetFavouritesByEmail(email);
        model.favourites = favourites;
        return PartialView("_productsCardPartial",model);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductsByproductId(int productId)
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

        productDetailsByproductIdViewModel? model  = await _productService.GetProductById(productId,email ?? "");
        model.BaseEmail = email;
        model.UserEmail = email;
        return View(model);
    
    }

    [HttpPost]
    public IActionResult UpdateFavourite(int productId)
    {
        try{
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

            ResponsesViewModel res = _productService.UpdateFavourite(productId, email);
            if(res.IsSuccess)
            {
                return Json(new {success=true,message=res.Message});
            }
            return Json(new {success=false,message="Error occured while updating in favourite"});
        }
        catch(Exception e)
        {
            return Json(new {success=false,message=e.Message});
        }
    }

    [Authorize(Roles = "Buyer")]
    public IActionResult Favourite()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }
}
