using System.IO;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.Service.implementation;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
     private readonly IUserRepository _userRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductService(
    IProductRepository productRepository, 
    IWebHostEnvironment webHostEnvironment,
    IUserRepository userRepository)
    {
        _productRepository = productRepository;
        _webHostEnvironment = webHostEnvironment;
        _userRepository = userRepository; 
    }
    
    /// <summary>
    /// method service for adding product
    /// </summary>
    /// <param name="model"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public ResponsesViewModel AddProduct(AddProductViewModel model, string email)
    {
        try
        {
            // get user details by email
            User? user = _userRepository.GetUserByEmail(email);
            if(user==null)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = $"Error occurred while adding product"
                };
            }
            
            // Create a folder for storing product images if it doesn't exist
            string imagesFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            // Save product details
            Product product = new Product
            {
                ProductName = model.ProductName,
                Description = model.Description,
                CategoryId = model.CategoryId,
                Price = model.Price,
                Stocks = model.Stocks,
                SellerId = user.UserId,
                CreatedAt = DateTime.Now
            };
            _productRepository.AddProduct(product);

            // Save images and store their paths in the database
            List<Image> productImages = new List<Image>();
            if (model.imageFile != null)
            {
                foreach (var file in model.imageFile)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(imagesFolderPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productImages.Add(new Image
                    {
                        ProductId = product.ProductId,
                        ImageUrl = $"/ProductImages/{uniqueFileName}"
                    });
                }
            }

            _productRepository.AddProductImages(productImages);

            return new ResponsesViewModel
            {
                IsSuccess = true,
                Message = "Product added successfully!"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error occurred while adding product: {e.Message}"
            };
        }
    }


    public List<Product>? GetSellerSpecificProductsByEmail(string email)
    {
        try{

            User? user = _userRepository.GetUserByEmail(email);
            if(user!= null)
            {
                return _productRepository.GetSellerSpecificProducts(user.UserId) ?? null;
            }
            return null;

        }catch(Exception e){
            throw new Exception(e.Message);
        }
    }
}
