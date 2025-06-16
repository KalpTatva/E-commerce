using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Core.Controllers;

public class ProductController : Controller
{

    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }


    [Authorize(Roles="Seller")]
    public IActionResult MyProducts()
    {
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        BaseViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }

    [Authorize(Roles ="Seller")]
    public IActionResult AddProduct(){
        string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
        AddProductViewModel baseViewModel = new () {
            BaseEmail = email
        }; 
        return View(baseViewModel);
    }

    [Authorize(Roles ="Seller")]
    [HttpPost]
    public IActionResult AddProduct(AddProductViewModel model)
    {
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

            if (ModelState.IsValid)
            {
                ResponsesViewModel responses = _productService.AddProduct(model, email);
                if (responses.IsSuccess)
                {
                    TempData["SuccessMessage"] = responses.Message;
                    return Json(new { success = true, message = responses.Message });
                }
                return Json(new { success = false, message = responses.Message });
            }
            return Json(new { success = false, message = "Invalid model state!" });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = $"Error: {e.Message}" });
        }
    }

    [Authorize(Roles ="Seller")]
    [HttpGet]
    public IActionResult GetSellerSpecificProducts()
    {
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

            List<Product>? products = _productService.GetSellerSpecificProductsByEmail(email);
            if(products != null)
            {
                return PartialView("_ListOfProductPartial", products);
            }

            return Json(new { message = "No data found!" });
        }
        catch(Exception e)
        {
            return Json(new {message = e.Message});
        }

    }

}
