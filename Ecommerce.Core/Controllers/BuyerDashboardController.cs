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

    /// <summary>
    /// index method for all type of users (not logged in, seller, buyer)
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// index method for all type of users (not logged in, seller, buyer)
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns>Partial</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts(string? search = null, int? category = null)
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
    
        ProductsViewModel model = await _productService.GetProducts(search, category);
        List<int> favourites = _productService.GetFavouritesByEmail(email ?? "");
        model.favourites = favourites;
        return PartialView("_productsCardPartial",model);
    }

    /// <summary>
    /// for buyers, method to get favourite product list by email
    /// </summary>
    /// <returns>Partial view</returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> GetFavouriteProducts()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
    
        ProductsViewModel model = await _productService.GetFavouriteProducts(email ?? "");
        List<int> favourites = _productService.GetFavouritesByEmail(email ?? "");
        model.favourites = favourites;
        return PartialView("_productsCardPartial",model);
    }

    /// <summary>
    /// for all users , method for getting per product details
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>View</returns>
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

    /// <summary>
    /// method for updating favourite list
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>Json</returns>
    [Authorize(Roles = "Buyer")]
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

    /// <summary>
    /// mthod for favourite products view
    /// </summary>
    /// <returns>View</returns>
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


    
    /// <summary>
    /// cart's view method
    /// </summary>
    /// <returns></returns>
    public IActionResult Cart()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// buyer's method for getting cart data
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [Authorize(Roles ="Buyer")]
    [HttpPost]
    public IActionResult AddToCart(int productId)
    {   
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

            ResponsesViewModel res = _productService.AddToCart(email ?? "", productId);
            if(res.IsSuccess)
            {
                return Json(new {success = true, message = res.Message});    
            }
            return Json(new {success = false, message = res.Message});

        }
        catch(Exception e)
        {
            return Json(new {success = false, message = e.Message});
        }
    }

    /// <summary>
    /// Buyer's method for get cart details
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles ="Buyer")]
    [HttpGet]
    public IActionResult GetCart()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
            ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        CartViewModel model = _productService.GetCartDetails(email??"");
        return PartialView("_CartListPartial",model);
    }


    [Authorize(Roles ="Buyer")]
    [HttpPut]
    public IActionResult UpdateValuesOfCart(int quantity,int cartId)
    {
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
            ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            CartUpdatesViewModel res = _productService.UpdateQuantityAtCart(quantity, cartId,email ?? "");
            if(res.IsSuccess == true)
            {
                return Json(new {
                    success = true,
                    totalQuantity = res.TotalQuantity,
                    totalDiscount = res.TotalDiscount,
                    totalPrice = res.TotalPrice
                });
            }
            return Json(new {success = false});

        }
        catch(Exception e)
        {
            return Json(new {success = false,message=e.Message});
        }
    }


    [Authorize(Roles ="Buyer")]
    [HttpPut]
    public IActionResult UpdateCartList(int cartId)
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
            ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        
        ResponsesViewModel res = _productService.DeleteCartFromList(cartId);
        if(res.IsSuccess == false)
        {
            TempData["ErrorMessage"] = res.Message;
        }
        CartViewModel model = _productService.GetCartDetails(email??"");
        return PartialView("_CartListPartial",model);
    }
}
