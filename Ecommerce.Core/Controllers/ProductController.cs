using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
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
    private readonly IHostEnvironment _env;
    public ProductController(IProductService productService, IHostEnvironment env)
    {
        _productService = productService;
        _env = env;
    }


    /// <summary>
    /// seller's products view
    /// </summary>
    /// <returns>View(baseViewModel)</returns>
    [Authorize(Roles = "Seller, Admin")]
    public IActionResult MyProducts()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
        
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// seller's view for add product (get method)
    /// </summary>
    /// <returns>View(baseViewModel)</returns>
    [Authorize(Roles = "Seller, Admin")]
    public IActionResult AddProduct(){
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        AddProductViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        }; 
        return View(baseViewModel);
    }


    /// <summary>
    /// post method dedicated for seller to add new porduct
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Json</returns>
    [Authorize(Roles = "Seller, Admin")]
    [HttpPost]
    public async Task<IActionResult> AddProduct(AddProductViewModel model)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
      
            if (ModelState.IsValid)
            {
                List<Feature>? features = string.IsNullOrEmpty(model.FeaturesInput) 
                    ? new List<Feature>() 
                    : JsonConvert.DeserializeObject<List<Feature>>(model.FeaturesInput);

                ResponsesViewModel responses = await _productService.AddProduct(model, email ?? "", features ?? new List<Feature>());
                if (responses.IsSuccess)
                {
                    TempData["SuccessMessage"] = responses.Message;
                    return Json(new { success = true, message = responses.Message });
                }
                return Json(new { success = false, message = responses.Message });
            }
            return Json(new { success = false, message = "Invalid model state! please fill out the inputs properly according to Guidlines" });
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
    [Authorize(Roles = "Seller, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetSellerSpecificProducts()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
        
            List<Product>? products = await _productService.GetSellerSpecificProductsByEmail(email ?? "");
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
    [Authorize(Roles = "Seller, Admin")]
    [HttpPut]
    public async Task<IActionResult> DeleteProduct(int ProductId)
    {
        try
        {
            ResponsesViewModel response = await _productService.DeleteProductById(ProductId);
            if(response.IsSuccess)
            {
                return Json(new { success = true, message = response.Message });
            }
            return Json(new { success = false, message = response.Message });
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
    [Authorize(Roles = "Seller, Admin")]
    [HttpGet]
    public IActionResult EditProduct(int productId)
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        EditProductViewModel? model = _productService.GetProductDetailsById(productId);
        if(model == null)
        {
            TempData["ErrorMessage"] = "501 Error occured while fatching product data";
            return View();
        }
        model.BaseEmail = email;
        model.BaseRole = role;
        model.BaseUserName = name;
        return View(model);
    }

    /// <summary>
    /// method for edit product
    /// </summary>
    /// <param name="model"></param>
    /// <returns>View(model)</returns>
    [Authorize(Roles = "Seller, Admin")]
    [HttpPost]
    public async Task<IActionResult> EditProduct(EditProductViewModel model)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);
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

                ResponsesViewModel responses = await _productService.UpdateProductDetails(model, features, DeletedImageIdList);

                if(responses.IsSuccess)
                {
                    TempData["SuccessMessage"] = responses.Message;
                    model.BaseEmail = email;
                    model.BaseRole = role;
                    model.BaseUserName = name;
                    return RedirectToAction("MyProducts");
                }
                TempData["ErrorMessage"] = responses.Message;
                model.BaseEmail = email;
                model.BaseRole = role;
                model.BaseUserName = name;
                return View(model);

            }
            TempData["ErrorMessage"] = "Error occured while editing product";
            model.BaseEmail = email;
            model.BaseRole = role;
            model.BaseUserName = name;
            return View(model);
        }
        catch(Exception e)
        {   
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);  
            TempData["ErrorMessage"] = $"501 : Error occured while editing porduct : {e.Message}";
            model.BaseEmail = email;
            model.BaseRole = role;
            model.BaseUserName = name;
            return View(model);
        }
    }

    [Authorize(Roles ="Seller,Admin")]
    public IActionResult DownloadTemplate()
    {
        string filePath = Path.Combine(_env.ContentRootPath,"wwwroot", "File\\ProductTemplate.xlsx");
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        string fileName = "ProductTemplate.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [Authorize(Roles ="Seller,Admin")]
    public IActionResult BulkUpload()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext); 
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        }; 
        return View(baseViewModel);
    }

    [Authorize(Roles = "Seller, Admin")]
    [HttpPost]
    public async Task<IActionResult> UploadProducts(IFormFile file)
    {
        if(file == null || file.Length == 0)
        {
            return  Json(new {success = false, message = "No file uploaded."});
        } 

        string extension = Path.GetExtension(file.FileName).ToLower();
        if(extension != ".xlsx")
        {
            return Json(new { success = false, message = "Invalid file format. Please upload an .xlsx file." });
        }

        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            ResponsesViewModel response = await _productService.UploadProducts(file, email ?? "");
            if(response.IsSuccess)
            {
                return Json(new {success = true, message = response.Message});
            }
            return Json(new { success = false, message = response.Message });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    } 
}
