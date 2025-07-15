using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ecommerce.Repository.implementation;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Service.implementation;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;


    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductService( 
    IWebHostEnvironment webHostEnvironment,
    IUnitOfWork unitOfWork)
    {
        _webHostEnvironment = webHostEnvironment; 
        _unitOfWork = unitOfWork;
        
    }
    

    #region Seller's service
    /// <summary>
    /// method service for adding product
    /// </summary>
    /// <param name="model"></param>
    /// <param name="email"></param>
    /// <param name="features"></param>
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> AddProduct(AddProductViewModel model, string email, List<Feature> features)
    {
        try
        {
            // get user details by email
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user==null)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = $"Error occurred while adding product"
                };
            }

            if(model.ProductName.Trim() == string.Empty || model.Description.Trim() == string.Empty)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Product name and description cannot be empty"
                };
            }
            if(model.Price <= 0 || model.Stocks <= 0)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Price and stocks must be greater than zero"
                };
            }
            if((int)DiscountEnum.Percentage == model.DiscountType && (model.Discount <= 0 || model.Discount > 100))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be between 0 and 100 for percentage discount type"
                };
            }
            if((int)DiscountEnum.FixedAmount == model.DiscountType && (model.Discount <= 0 || model.Discount > model.Price))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be greater than zero and less than or equal to price for fixed amount discount type"
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
            await _unitOfWork.ProductRepository.AddAsync(product);

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

            await _unitOfWork.ImageRepository.AddRangeAsync(productImages);

            // Save features
            foreach (Feature feature in features)
            {
                feature.ProductId = product.ProductId;
            }

            // _featureRepository.AddFeaturesRange(features);
            await _unitOfWork.FeatureRepository.AddRangeAsync(features);

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
    /// <returns>List<Product></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<Product>?> GetSellerSpecificProductsByEmail(string email)
    {
        try{

            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user!= null)
            {
                return await _unitOfWork.ProductRepository.FindAllAsync(
                    x => x.SellerId == user.UserId && x.IsDeleted == false,
                    x => x.ProductId,
                    false
                ) ?? null;
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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> DeleteProductById(int id)
    {
        try
        {
            
            Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(id);

            // check weather it into the cart of user or not if into the cart then return

            Cart? existingInCart = await _unitOfWork.CartRepository.FindAsync(x => x.ProductId == product.ProductId && x.IsDeleted == false);
            if(existingInCart!=null)
            {
                return new ResponsesViewModel 
                {
                    IsSuccess = false,
                    Message = "This product cannot be deleted while it's in a user's shopping cart."
                };
            }

            if(product!=null)
            {   
                // soft delete of product
                product.IsDeleted = true;
                product.EditedAt = DateTime.Now;
                product.DeletedAt = DateTime.Now;
                await _unitOfWork.ProductRepository.UpdateAsync(product);
        
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
    /// <returns>EditProductViewModel</returns>
    public EditProductViewModel? GetProductDetailsById(int productId)
    {
        try
        {
            EditProductViewModel? product = _unitOfWork.ProductRepository.GetProductDetailsById(productId);
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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> UpdateProductDetails(EditProductViewModel model, List<Feature>? features, List<int>? DeletedImageIdList)
    {
        try
        {  
            // validaions 
            if(model.ProductName.Trim() == string.Empty || model.Description.Trim() == string.Empty)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Product name and description cannot be empty"
                };
            }
            if(model.Price <= 0 || model.Stocks <= 0)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Price and stocks must be greater than zero"
                };
            }
            if((int)DiscountEnum.Percentage == model.DiscountType && (model.Discount <= 0 || model.Discount > 100))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be between 0 and 100 for percentage discount type"
                };
            }
            if((int)DiscountEnum.FixedAmount == model.DiscountType && (model.Discount <= 0 || model.Discount > model.Price))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be greater than zero and less than or equal to price for fixed amount discount type"
                };
            }



            // image management
            if(DeletedImageIdList != null && DeletedImageIdList.Any())
            {
                // method which delete images which are no longer selected
                _unitOfWork.ImageRepository.DeleteProductImagesByIds(DeletedImageIdList);
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
                await _unitOfWork.ImageRepository.AddRangeAsync(productImages);
            }
            

            // fetures management
            if(features!=null)
            {
                // udpate features (update old one and add all new one)
                List<Feature>? features1 = await _unitOfWork.FeatureRepository.FindAllAsync(
                                                x => x.ProductId == model.ProductId,
                                                x => x.FeatureId, false);

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
                            await _unitOfWork.FeatureRepository.DeleteAsync(existingFeature);
                        }
                    }
                    
                    // _featureRepository.updateFeaturesRange(FeaturesToUpdate);
                    await _unitOfWork.FeatureRepository.UpdateRangeAsync(FeaturesToUpdate);


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

                    // _featureRepository.AddFeaturesRange(NewFeaturesToAdd);
                    await _unitOfWork.FeatureRepository.AddRangeAsync(NewFeaturesToAdd);
                }
            }

            if(model != null)
            {
                Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(model.ProductId);

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

                    await _unitOfWork.ProductRepository.UpdateAsync(product);

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

    /// <summary>
    /// method for getting all products for offer by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns>List<ProductNameViewModel></returns>
    /// <exception cref="Exception"></exception>
    public List<ProductNameViewModel> GetProductsForOffer(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                return new List<ProductNameViewModel>();
            }
            if(user.RoleId != (int)RoleEnum.Seller)
            {
                return _unitOfWork.ProductRepository.GetAllProductsForOffer();
            }
            else
            {
                return _unitOfWork.ProductRepository.GetProductsForOffer(user.UserId);
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    #endregion
    #region Buyer's service
    

    /// <summary>
    /// method for getting all products details
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns>ProductsViewModel</returns>
    public async Task<ProductsViewModel> GetProducts(string? search = null, int? category = null)
    {
        try
        {
            ProductsViewModel productsViewModel = new ();
            if(search!=null)
            {
                search = search.ToLower().Trim();
            }

            List<ProductsDeatailsViewModel>? products = await _unitOfWork.ProductRepository.GetAllProducts(search,category);
            
            
            if(products != null && products.Any() )
            {
                productsViewModel.productsDetails = products;
            }

            return productsViewModel;
        }
        catch
        {
            // will showcase no items found on page
            return new ProductsViewModel();
        }
    }

    /// <summary>
    /// method for getting user wise favourite products details
    /// </summary>
    /// <param name="email"></param>
    /// <returns>ProductsViewModel</returns>
    public async Task<ProductsViewModel> GetFavouriteProducts(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user==null)
            {
                // will showcase no items found on page
                return new ProductsViewModel();
            }
            ProductsViewModel productsViewModel = new ();
            List<ProductsDeatailsViewModel>? products = await _unitOfWork.ProductRepository.GetFavouriteProductsByUserId(user.UserId);
            if(products != null && products.Any() )
            {

                productsViewModel.productsDetails = products;
            }

            return productsViewModel;
        }
        catch
        {
            // will showcase no items found on page
            return new ProductsViewModel();
        }
    }
    
    /// <summary>
    /// method for getting product by product id and email 
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="email"></param>
    /// <returns>productDetailsByproductIdViewModel</returns>
    public async Task<productDetailsByproductIdViewModel?> GetProductById(int productId, string email)
    {
        try
        {
            productDetailsByproductIdViewModel? result = await _unitOfWork.ProductRepository.GetProductDetailsByProductId(productId);
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(result!=null && user!=null)
            {
                // Favourite? favourite = _favouriteRepository.GetFavouriteByIds(user.UserId,result.ProductId);
                Favourite? favourite = await _unitOfWork.FavouriteRepository.FindAsync(f => f.UserId == user.UserId && f.ProductId == productId);
                if(favourite!= null)
                {
                    result.IsFavourite = true;
                }
                else{
                    result.IsFavourite = false;
                }

                decimal AverageRatings = result.Reviews?.Count > 0 ? result.Reviews.Average(r => r.Ratings) : 0;

                result.AverageRatings = Math.Round(AverageRatings, 2);

            }
            
            return result;
        }
        catch
        {
            // will showcase no items found on page
            return new productDetailsByproductIdViewModel();
        }
    }
    
    /// <summary>
    /// method for updating state of favourite button
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="email"></param>
    /// <returns>ResponsesViewModel</returns>
    /// <exception cref="Exception"></exception>
    public async Task<ResponsesViewModel> UpdateFavourite(int productId,string? email = null)
    {
        try
        {
            if(email!=null)
            {
                User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
                if(user == null)
                {
                    return new ResponsesViewModel{
                        IsSuccess=false,
                        Message="Invalid user details for updating favourites"
                    };
                }

                Favourite? favourite = await _unitOfWork.FavouriteRepository.FindAsync(f => f.UserId == user.UserId && f.ProductId == productId);
                if(favourite != null && favourite.ProductId > 0)
                {
                    await _unitOfWork.FavouriteRepository.DeleteAsync(favourite);
                }else
                {
                    Favourite favourite1 = new(){
                        ProductId = productId,
                        UserId = user.UserId
                    };
                    await _unitOfWork.FavouriteRepository.AddAsync(favourite1);
                }
                return new ResponsesViewModel{
                    IsSuccess=true,
                    Message="Change made successfully"
                };
            }
            return new ResponsesViewModel{
                IsSuccess=false,
                Message="Invalid user details for updating favourites"
            };
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// mehtod for getting details of favourite products list by user emails
    /// </summary>
    /// <param name="email"></param>
    /// <returns>List<int></returns>
    public List<int> GetFavouritesByEmail(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user==null)
            {
                return new List<int>();
            }
            return _unitOfWork.FavouriteRepository.GetFavouriteByUserId(user.UserId);
        }
        catch
        {
            return new List<int>();
        }
    }
    

    /// <summary>
    /// method for adding product into cart
    /// </summary>
    /// <param name="email"></param>
    /// <param name="productId"></param>
    /// <returns>responsesviewmodel</returns>
    public async Task<ResponsesViewModel> AddToCart(string email, int productId)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user==null)
            {
                return new ResponsesViewModel{
                    IsSuccess = false,
                    Message = "user not found! please login first"
                };
            }

            // check if product is already in cart
            Cart? existingCart = await _unitOfWork.CartRepository.FindAsync(c => c.UserId == user.UserId && c.ProductId == productId && c.IsDeleted == false);
            if(existingCart != null)
            {
                return new ResponsesViewModel{
                    IsSuccess = false,
                    Message = "product already exists in your cart!"
                };
            }

            Cart cart = new () {
                UserId = user.UserId,
                ProductId = productId
            };

            // _cartRepository.AddToCart(cart);
            await _unitOfWork.CartRepository.AddAsync(cart);

            return new ResponsesViewModel{
                IsSuccess = true,
                Message = "product added into your cart!"
            };
        }
        catch(Exception e)
        {
            return new ResponsesViewModel{
                IsSuccess = false,
                Message = e.Message
            };
        }
    }
    
    
    
    /// <summary>
    /// method which calculate major properties of cart and returns cart items of user
    /// </summary>
    /// <param name="email"></param>
    /// <returns>CartViewModel</returns>
    public async Task<CartViewModel> GetCartDetails(string email)
    {
        try
        {
            CartViewModel model = new ();
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                return new CartViewModel();
            }

            List<productAtCartViewModel>? result = await _unitOfWork.CartRepository.GetproductAtCart(user.UserId);
            
            if(result!=null && result.Any())
            {
                decimal TotalPrice = 0;
                decimal TotalDiscount = 0;
                int TotalQuantity = 0;
                decimal currentDiscount = 0;
                int currentQuantity = 0;
                decimal currentPrice = 0;

                foreach(productAtCartViewModel product in result)
                {

                    // default discount
                    currentDiscount = product.DiscountType == (int)DiscountEnum.FixedAmount ? 
                                    ((product.Discount ?? 0) * product.Quantity) : 
                                    ((product.Price * (product.Discount ?? 0) * product.Quantity) / 100);
                    
                    // price with given discount 
                    currentPrice = (product.Price * product.Quantity) - currentDiscount;
                    
                    // quantity with given product
                    currentQuantity = product.Quantity;
                    
                    // checking for available offers
                    if(product.Offer != null && product.Offer.OfferId > 0)
                    {
                        // add offer discount to total discount
                        switch (product.Offer.OfferType)
                        {
                            case (int)OfferTypeEnum.Percentage:
                                currentPrice = currentPrice - ((currentPrice * (product.Offer.DiscountRate ?? 0) * currentQuantity) / 100);
                                currentDiscount = currentDiscount + ((currentPrice * (product.Offer.DiscountRate ?? 0) * currentQuantity) / 100);
                                break;
                            case (int)OfferTypeEnum.FixedPrice:
                                currentPrice = currentPrice - ((product.Offer.DiscountRate ?? 0) * currentQuantity); 
                                currentDiscount = currentDiscount + ((product.Offer.DiscountRate ?? 0) * currentQuantity);
                                break;
                            case (int)OfferTypeEnum.BOGO:
                                currentQuantity = 2 * currentQuantity;
                                break;
                            default:
                                break;
                        }
                    }

                    TotalDiscount += currentDiscount;
                    TotalQuantity += currentQuantity;
                    TotalPrice += currentPrice;
                }

                model.ProductsAtCart = result;
                model.TotalPrice = TotalPrice;
                model.TotalDiscount = TotalDiscount;
                model.TotalQuantity = TotalQuantity;
            }


            return model;
        }
        catch
        {
            return new CartViewModel();
        }
    }


    /// <summary>
    /// method for updating cart product's quantity values and displaying updated totals 
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="cartId"></param>
    /// <param name="email"></param>
    /// <returns>CartUpdatesViewModel</returns>
    /// <exception cref="Exception"></exception>
    public async Task<CartUpdatesViewModel> UpdateQuantityAtCart(int quantity, int cartId, string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                return new CartUpdatesViewModel();
            }
            //need to check weather the quantity exceeds the available stocks
            Product? product1 =  await _unitOfWork.CartRepository.GetProductByCartId(cartId, user.UserId);
            if(product1 != null)
            {
                if(quantity > product1.Stocks || quantity <= 0)
                {
                    throw new Exception($"Product {product1.ProductName} has only {product1.Stocks} stocks available for now! you can add once stocks are updated");
                }
            }
            else
            {
                throw new Exception("Product not found in cart!");
            }

            // update cart quantity by cartId
            // await _unitOfWork.CartRepository.UpdateCartByIdAsync(cartId, quantity);
            Cart cart = await _unitOfWork.CartRepository.GetByIdAsync(cartId);
            cart.Quantity = quantity;
            cart.EditedAt = DateTime.Now;
            await _unitOfWork.CartRepository.UpdateAsync(cart);


            // update values on frontend 
            List<productAtCartViewModel>? result = await _unitOfWork.CartRepository.GetproductAtCart(user.UserId);
            
            CartUpdatesViewModel model = new ();
            
            if(result!=null && result.Any())
            {
                decimal TotalPrice = 0;
                decimal TotalDiscount = 0;
                int TotalQuantity = 0;
                decimal currentDiscount = 0;
                int currentQuantity = 0;
                decimal currentPrice = 0;
                foreach(productAtCartViewModel product in result)
                {
                    // default discount
                    currentDiscount = product.DiscountType == (int)DiscountEnum.FixedAmount ? 
                                    ((product.Discount ?? 0) * product.Quantity) : 
                                    ((product.Price * (product.Discount ?? 0) * product.Quantity) / 100);
                    
                    // price with given discount 
                    currentPrice = (product.Price * product.Quantity) - currentDiscount;
                    
                    // quantity with given product
                    currentQuantity = product.Quantity;
                    
                    // checking for available offers
                    if(product.Offer != null && product.Offer.OfferId > 0)
                    {
                        // add offer discount to total discount
                        switch (product.Offer.OfferType)
                        {
                            case (int)OfferTypeEnum.Percentage:
                                currentPrice = currentPrice - ((currentPrice * (product.Offer.DiscountRate ?? 0) * currentQuantity) / 100);
                                currentDiscount = currentDiscount + ((currentPrice * (product.Offer.DiscountRate ?? 0) * currentQuantity) / 100);
                                break;
                            case (int)OfferTypeEnum.FixedPrice:
                                currentPrice = currentPrice - ((product.Offer.DiscountRate ?? 0) * currentQuantity); 
                                currentDiscount = currentDiscount + ((product.Offer.DiscountRate ?? 0) * currentQuantity);
                                break;
                            case (int)OfferTypeEnum.BOGO:
                                currentQuantity = 2 * currentQuantity;
                                break;
                            default:
                                break;
                        }
                    }

                    TotalDiscount += currentDiscount;
                    TotalQuantity += currentQuantity;
                    TotalPrice += currentPrice;
                }

                model.TotalPrice = TotalPrice;
                model.TotalDiscount = TotalDiscount;
                model.TotalQuantity = TotalQuantity;
            }
            model.IsSuccess = true;
            return model;

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for delete product from cart (soft delete)
    /// </summary>
    /// <param name="cartId"></param>
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> DeleteCartFromList(int cartId)
    {
        try
        {
            Cart cart = await _unitOfWork.CartRepository.GetByIdAsync(cartId);
            cart.IsDeleted = true;
            cart.DeletedAt = DateTime.Now;
            await _unitOfWork.CartRepository.UpdateAsync(cart);
            return new ResponsesViewModel(){
                IsSuccess = true,
                Message = "cart updated successfully"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel(){
                IsSuccess = false,
                Message = e.Message
            };
        }
    }
    
    /// <summary>
    /// method for adding review to product by orderProductId
    /// </summary>
    /// <param name="orderProductId"></param>
    /// <param name="rating"></param>
    /// <param name="productId"></param>
    /// <param name="reviewText"></param>
    /// <param name="email"></param>
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> AddReview(int orderProductId,decimal rating, int productId, string reviewText,string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                return new ResponsesViewModel{
                    IsSuccess = false,
                    Message = "user not found! please login first"
                };
            }

            Review review = new Review
            {
                OrderProductId = orderProductId,
                Ratings = rating,
                ProductId = productId,
                Comments = reviewText,
                BuyerId = user.UserId,
            };

            await _unitOfWork.ReviewRepository.AddAsync(review);

            return new ResponsesViewModel{
                IsSuccess = true,
                Message = "review added successfully!"
            };
        }
        catch(Exception e)
        {
            return new ResponsesViewModel{
                IsSuccess = false,
                Message = e.Message
            };
        }
    }
    

    /// <summary>
    /// method to check product stock by cart ids
    /// </summary>
    /// <param name="email"></param>
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> CheckProductStockByCartId(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                return new ResponsesViewModel{
                    IsSuccess = false,
                    Message = "user not found! please login first"
                };
            }
            List<productAtCartViewModel>? products = await _unitOfWork.CartRepository.GetproductAtCart(user.UserId);
            if(products != null && products.Any())
            {
                foreach (productAtCartViewModel product in products)
                {
                    if(product.Quantity > product.Stocks || product.Stocks <= 0)
                    {
                        return new ResponsesViewModel{
                            IsSuccess = false,
                            Message = $"Product {product.ProductName} has only {product.Stocks} stocks available!"
                        };
                    }
                }
            }
            return new ResponsesViewModel{
                IsSuccess = true,
                Message = "All products are available in stock!"
            };
        }
        catch(Exception e)
        {
            return new ResponsesViewModel{
                IsSuccess = false,
                Message = e.Message
            };
        }
    }

    #endregion









}
