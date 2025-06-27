using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Core.Utils;
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
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
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
        string? email =  BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        ProductsViewModel model = await _productService.GetProducts(search, category);
        List<int> favourites = _productService.GetFavouritesByEmail(email ?? "");
        model.BaseEmail = email;
        model.BaseRole = role;
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
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
    
        ProductsViewModel model = await _productService.GetFavouriteProducts(email ?? "");
        List<int> favourites = _productService.GetFavouritesByEmail(email ?? "");
        model.favourites = favourites;
        model.BaseEmail = email;
        model.BaseRole = role;
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
        string? email = BaseValues.GetEmail(HttpContext);

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
            string? email = BaseValues.GetEmail(HttpContext);

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
    /// 
    [Authorize(Roles = "Buyer")]
    public IActionResult Favourite()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        }; 
        return View(baseViewModel);
    }


    
    /// <summary>
    /// cart's view method
    /// </summary>
    /// <returns>View with base view model</returns>
    [Authorize(Roles ="Buyer")]
    public IActionResult Cart()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role

        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// buyer's method for getting cart data
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>json</returns>
    [Authorize(Roles ="Buyer")]
    [HttpPost]
    public IActionResult AddToCart(int productId)
    {   
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);

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
    /// <returns>partial view with model</returns>
    [Authorize(Roles ="Buyer")]
    [HttpGet]
    public IActionResult GetCart()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        CartViewModel model = _productService.GetCartDetails(email??"");
        return PartialView("_CartListPartial",model);
    }


    /// <summary>
    /// Buyer's method for updating cart values
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="cartId"></param>
    /// returns>Json</returns>
    [Authorize(Roles ="Buyer")]
    [HttpPut]
    public IActionResult UpdateValuesOfCart(int quantity,int cartId)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
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


    /// <summary>
    /// Buyer's method for deleting cart from list
    /// </summary>
    /// <param name="cartId"></param>
    /// returns>PartialView</returns>
    [Authorize(Roles ="Buyer")]
    [HttpPut]
    public IActionResult UpdateCartList(int cartId)
    {
        string? email = BaseValues.GetEmail(HttpContext);
        
        ResponsesViewModel res = _productService.DeleteCartFromList(cartId);
        if(res.IsSuccess == false)
        {
            TempData["ErrorMessage"] = res.Message;
        }
        CartViewModel model = _productService.GetCartDetails(email??"");
        return PartialView("_CartListPartial",model);
    }
}
