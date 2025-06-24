using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ecommerce.Core.Controllers;

public class ProductController : Controller
{

    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }


    /// <summary>
    /// seller's products view
    /// </summary>
    /// <returns>View(baseViewModel)</returns>
    [Authorize(Roles="Seller")]
    public IActionResult MyProducts()
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
    /// seller's view for add product (get method)
    /// </summary>
    /// <returns>View(baseViewModel)</returns>
    [Authorize(Roles ="Seller")]
    public IActionResult AddProduct(){
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
    
        AddProductViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role
        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// post method dedicated for seller to add new porduct
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Json</returns>
    [Authorize(Roles ="Seller")]
    [HttpPost]
    public IActionResult AddProduct(AddProductViewModel model)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
      
            if (ModelState.IsValid)
            {
                List<Feature>? features = string.IsNullOrEmpty(model.FeaturesInput) 
                    ? new List<Feature>() 
                    : JsonConvert.DeserializeObject<List<Feature>>(model.FeaturesInput);

                ResponsesViewModel responses = _productService.AddProduct(model, email ?? "", features ?? new List<Feature>());
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


    /// <summary>
    /// get method for sellers which shows the list of products which seller added
    /// </summary>
    /// <returns>Json</returns>
    [Authorize(Roles ="Seller")]
    [HttpGet]
    public IActionResult GetSellerSpecificProducts()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
        
            List<Product>? products = _productService.GetSellerSpecificProductsByEmail(email ?? "");
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


    /// <summary>
    /// controller for handling put request of soft deleting product by id
    /// </summary>
    /// <param name="ProductId"></param>
    /// <returns>Json</returns>
    [Authorize(Roles ="Seller")]
    [HttpPut]
    public IActionResult DeleteProduct(int ProductId)
    {
        try
        {
            ResponsesViewModel response = _productService.DeleteProductById(ProductId);
            if(response.IsSuccess)
            {
                return Json(new { success = true, message = response.Message });
            }
            return Json(new { success = false, message = "error occured while delete product!" });
        }
        catch(Exception e)
        {
            return Json(new { success = false, message = $"Error: {e.Message}" });

        }
    }

    /// <summary>
    /// edit product view for seller
    /// </summary>
    /// <returns>View(model)</returns>
    [Authorize(Roles ="Seller")]
    [HttpGet]
    public IActionResult EditProduct(int productId)
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
    
        EditProductViewModel? model = _productService.GetProductDetailsById(productId);
        if(model == null)
        {
            TempData["ErrorMessage"] = "501 Error occured while fatching product data";
            return View();
        }
        model.BaseEmail = email;
        model.BaseRole = role;
        return View(model);
    }

    /// <summary>
    /// method for edit product
    /// </summary>
    /// <param name="model"></param>
    /// <returns>View(model)</returns>
    [Authorize(Roles ="Seller")]
    [HttpPost]
    public IActionResult EditProduct(EditProductViewModel model)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
        
            if(ModelState.IsValid)
            {
                // list of features with previous ones so need to check the db
                List<Feature>? features = string.IsNullOrEmpty(model.FeaturesInput) 
                        ? new List<Feature>() 
                        : JsonConvert.DeserializeObject<List<Feature>>(model.FeaturesInput);
                
                // list which is formed for deleting existing image from db 
                List<int>? DeletedImageIdList = string.IsNullOrEmpty(model.ImageDeleteInput) 
                        ? new List<int>() 
                        : JsonConvert.DeserializeObject<List<int>>(model.ImageDeleteInput);

                ResponsesViewModel responses = _productService.UpdateProductDetails(model, features, DeletedImageIdList);

                if(responses.IsSuccess)
                {
                    TempData["SuccessMessage"] = responses.Message;
                    return RedirectToAction("MyProducts");
                }
                TempData["ErrorMessage"] = responses.Message;
                return View(model);

            }
            TempData["ErrorMessage"] = "Error occured while editing product";
            return View(model);
        }
        catch(Exception e)
        {   
            TempData["ErrorMessage"] = $"501 : Error occured while editing porduct : {e.Message}";
            return View(model);
        }
    }
}
