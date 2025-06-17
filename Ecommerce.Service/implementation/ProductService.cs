using System.IO;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
    /// <param name="features"></param>
    /// <returns></returns>
    public ResponsesViewModel AddProduct(AddProductViewModel model, string email, List<Feature> features)
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
                Discount = model.Discount,
                DiscountType = model.DiscountType,
                CreatedAt = DateTime.Now
            };
            _productRepository.AddProduct(product);

            // Save images and store their paths in the database
            List<Image> productImages = new List<Image>();
            if (model.imageFile != null)
            {
                foreach (IFormFile file in model.imageFile)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(imagesFolderPath, uniqueFileName);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
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

            // Save features
            foreach (Feature feature in features)
            {
                feature.ProductId = product.ProductId;
            }

            _productRepository.AddFeaturesRange(features);

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

    /// <summary>
    /// get seller specific data from db
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// soft delete of product
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ResponsesViewModel DeleteProductById(int id)
    {
        try
        {
            Product? product = _productRepository.GetProductById(id);
            if(product!=null)
            {   
                // soft delete of product
                _productRepository.DeleteProduct(product);
        
                return new ResponsesViewModel 
                {
                    IsSuccess = true,
                    Message = "product deleted successfully!"
                };
            }
        
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error occurred while deleting product"
            };

        }
        catch(Exception e)
        {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error occurred while deleting product: {e.Message}"
            };
        }
    }

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public EditProductViewModel? GetProductDetailsById(int productId)
    {
        try
        {
            EditProductViewModel? product = _productRepository.GetProductDetailsById(productId);
            return product;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for
    ///     1. delete images which are no longer in model (have list of images for delete)
    ///     2. add new images 
    ///     3. udpate features (update old one and add all new one)
    ///     4. update product details
    /// </summary>
    /// <param name="model"></param>
    /// <param name="features"></param>
    /// <param name="DeletedImageIdList"></param>
    /// <returns></returns>
    public ResponsesViewModel UpdateProductDetails(EditProductViewModel model, List<Feature>? features, List<int>? DeletedImageIdList)
    {
        try
        {   

            // image management
            if(DeletedImageIdList != null && DeletedImageIdList.Any())
            {
                // method which delete images which are no longer selected
                _productRepository.DeleteProductImagesByIds(DeletedImageIdList);   
            }

            // Create a folder for storing product images if it doesn't exist
            string imagesFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }


            // Save images and store their paths in the database
            List<Image> productImages = new List<Image>();
            if (model.imageFile != null)
            {
                foreach (IFormFile file in model.imageFile)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(imagesFolderPath, uniqueFileName);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productImages.Add(new Image
                    {
                        ProductId = model.ProductId,
                        ImageUrl = $"/ProductImages/{uniqueFileName}"
                    });
                }   

                // call for adding images
                _productRepository.AddProductImages(productImages);

            }
            

            // fetures management
            if(features!=null)
            {
                // udpate features (update old one and add all new one)
                List<Feature>? features1 = _productRepository.GetFeaturesByProductId(model.ProductId);
                if(features1 != null)
                {
                    // to update the range of features 
                    List<Feature> FeaturesToUpdate = new ();
                    
                    foreach (Feature existingFeature in features1)
                    {
                        // Check if the existing feature is still in the new list
                        Feature? updatedFeature = features.FirstOrDefault(f => f.FeatureName == existingFeature.FeatureName);
                        if (updatedFeature != null)
                        {
                            // Update the existing feature
                            existingFeature.Description = updatedFeature.Description;
                            existingFeature.EditedAt = DateTime.Now;
                            FeaturesToUpdate.Add(existingFeature);
                        }
                        else
                        {
                            // Delete the feature if it's no longer in the new list
                            _productRepository.DeleteFeature(existingFeature);
                        }
                    }
                    
                    _productRepository.updateFeaturesRange(FeaturesToUpdate);

                    // Add new features that are not already in the database
                    List<Feature> NewFeaturesToAdd = new ();

                    foreach (Feature newFeature in features)
                    {
                        if (features1.All(f => f.FeatureName != newFeature.FeatureName))
                        {
                            newFeature.ProductId = model.ProductId;
                            NewFeaturesToAdd.Add(newFeature);
                        }
                    }

                    _productRepository.AddFeaturesRange(NewFeaturesToAdd);
                }
            }

            if(model != null)
            {

                Product? product = _productRepository.GetProductById(model.ProductId);
                if(product!=null)
                {
                    product.ProductName = model.ProductName;
                    product.Description = model.Description;
                    product.CategoryId = model.CategoryId;
                    product.Price = model.Price;
                    product.Stocks = model.Stocks;
                    product.DiscountType = model.DiscountType;
                    product.Discount = model.Discount;
                    product.EditedAt = DateTime.Now;

                    _productRepository.updateProducts(product);

                    return new ResponsesViewModel
                    {
                        IsSuccess = true,
                        Message = $"product Edited successfully!"
                    };

                };

            }


            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"500 : Error occurred while editing product"
            };
        }
        catch(Exception e)
        {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"500 : Error occurred while editing product: {e.Message}"
            };
        }
    }
}
