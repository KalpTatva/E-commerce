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
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Drawing.Imaging;
using static Ecommerce.Repository.Helpers.Enums;
using System.IO.Compression;


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
    public async Task<List<Product>?> GetSellerSpecificProductsByEmail(string email, int pageNumber = 1, int pageSize = 5)
    {
        try{

            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user!= null)
            {
                return await _unitOfWork.ProductRepository.FindAllAsync(
                    x => x.SellerId == user.UserId && x.IsDeleted == false,
                    x => x.ProductId,
                    false,
                    pageNumber,
                    pageSize
                ) ?? null;
            }
            return null;

        }catch(Exception e){
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting total products count of seller by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception">count of seller's total products</exception>
    public async Task<int> GetSellersTotalProductsCount(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user!=null)
            {
                return await _unitOfWork.ProductRepository.CountAsync(x => x.SellerId == user.UserId && x.IsDeleted == false);
            }
            return 0;
        }
        catch(Exception e)
        {
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

    /// <summary>
    /// method for uploading products from zip file which contains excel file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<ResponsesViewModel> UploadProducts(IFormFile file, string email)
    {
        try
        {
            // need to check if file is not null
            if (file == null || file.Length == 0) { throw new Exception("File is empty or not provided."); }
            
            // if user not found 
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if (user == null) { throw new Exception("User not found. Please login first."); }
            
            
            // need to check if file is zip file or not
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".zip") { throw new Exception("Invalid file format. Please upload a zip file only."); }

            // decompress the zip file 
            string tempPath = Path.Combine(_webHostEnvironment.WebRootPath, "TempUploads", Guid.NewGuid().ToString());
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true); // delete existing directory if exists
            }
            string zipFilePath = Path.Combine(tempPath, file.FileName);

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            
            // Extract the zip file to the temporary path 
            using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            {
                await file.CopyToAsync(zipFileStream);
            }             
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                archive.ExtractToDirectory(tempPath, true);
            }

            // find excel file into it 
            string? excelFilePath = Directory.GetFiles(tempPath, "*.xlsx").FirstOrDefault();
            if (string.IsNullOrEmpty(excelFilePath))
            {
                throw new Exception("No Excel file found in the zip archive.");
            }

            // method for reading excel file and adding product details 
            ResponsesViewModel response = await ExcelReadAndAddProducts(excelFilePath, tempPath, user.UserId);
            if(response.IsSuccess)
            {
                // if everything handled successfully and products added then delete the temp folder and zip file
                if (Directory.Exists(tempPath)) { Directory.Delete(tempPath, true); }
                return new ResponsesViewModel {
                    IsSuccess = true,
                    Message = response.Message
                };
            }
            else 
            {
                // if there is any error in reading excel file then delete the temp folder and zip file
                if (Directory.Exists(tempPath)) { Directory.Delete(tempPath, true); }
                return new ResponsesViewModel {
                    IsSuccess = false,
                    Message = response.Message
                }; 
            }
        }
        catch (Exception e)
        {
            return new ResponsesViewModel {
                IsSuccess = false,
                Message = $"An error occurred while uploading products, {e.Message}"
            };
        }
    }

    /// <summary>
    /// method for reading excel file and adding products into database
    /// </summary>
    /// <param name="excelFilePath"></param>
    /// <param name="tempPath"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<ResponsesViewModel> ExcelReadAndAddProducts(string excelFilePath,string tempPath, int userId)
    {
        try
        {
            // adding transaction for adding products
            // if any error occurs then rollback the transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            // non commercial licence 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Read the Excel file and add products to the database
            using (ExcelPackage package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null) { throw new Exception("No worksheet found in the Excel file."); }

                // 1. check for valid headers
                string[] expectedHeaders = new[] { "Index","Product Name", "Description", "Category", "Discount type", "Discount Rate", "Price", "Stock", "Features", "Image names" };
                for (int col = 1; col <= expectedHeaders.Length; col++)
                {
                    string currentName = worksheet.Cells[1, col].Text.Trim().ToLower();
                    string expecteName = expectedHeaders[col - 1].Trim().ToLower();
                    if (currentName != expecteName)
                    {
                        return new ResponsesViewModel{IsSuccess = false, Message = $"Invalid header at column {col}. Expected '{expectedHeaders[col - 1]}'." };
                    }
                }

                // 2. Add products into product list while validating each product
                List<Product> products = new List<Product>();
                List<Dictionary<int,string>> errors = new List<Dictionary<int,string>>();
                List<ImageHelper> images = new List<ImageHelper>();
                
                string[] validCategories = new[] { "laptops", "computers", "accessories" };
                string[] validDiscountTypes = new[] { "percentage", "fixed amount"};
                string[] validImageExtensions = new[] { "avif", "png", "svg", "bmp", "gif", "webp", "tiff", "heic", "ico", "raw", "jfif", "jpg", "jpeg", "jpe" };

                int rowCount = worksheet.Dimension.Rows;
                int countRowsSuccess = 0;
                int totalRow = 0;
                // validate and add products into list
                // if error occure then add it to error list 
                for(int row = 2; row <= rowCount; row++)
                {
                    // continue if data is empty in all column 
                    if(
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 4].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 6].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 7].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 8].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 9].Text) &&
                        string.IsNullOrWhiteSpace(worksheet.Cells[row, 10].Text)
                    )
                    {
                        continue;
                    }
                    Product product = new Product();
                    int col = 1;
                    if (!int.TryParse(worksheet.Cells[row, col].Text, out int count))
                    {
                        errors.Add(new Dictionary<int, string> { { row, $"Row {row}: Invalid or missing ID." } });
                    }
                    col++;
                    
                    // validate product name 
                    string productName = worksheet.Cells[row,col].Text;
                    // if product name is empty 
                    if(string.IsNullOrWhiteSpace(productName))
                    {
                        errors.Add(new Dictionary<int, string> { { row, $"Row {row}: Invalid Product Name." } }); 
                    }
                    // if product name already exists 
                    if(!string.IsNullOrWhiteSpace(productName))
                    {
                        string productNameLower = productName.Trim();
                        Product? p = await _unitOfWork.ProductRepository.FindAsync(p => p.ProductName.Equals(productNameLower) && p.IsDeleted == false);
                        if (p != null)
                        {
                            errors.Add(new Dictionary<int, string> { { row, $"Row {row}: Product with name '{productName}' already exists." } });
                        }
                    }
                    // for image store it in variable
                    string imageProductName = productName.Trim();
                    col++;
                    
                    // validate Description
                    string Description = worksheet.Cells[row, col].Text;
                    if (string.IsNullOrWhiteSpace(Description))
                    {
                        errors.Add(new Dictionary<int, string> { { row, $"Row {row}: Description is required." } });
                    }
                    col++;

                    // Category (must be one of the valid categories)
                    string category = worksheet.Cells[row, col].Text;
                    int CategoryId = 0;
                    if (!validCategories.Contains(category.ToLower()))
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Category. Must be one of: {string.Join(", ", validCategories)}."} });
                    }
                    switch (category.ToLower())
                    {
                        case "accessories":
                            CategoryId = (int)CategoriesEnum.Accessories;
                            break;
                        case "computers":
                            CategoryId = (int)CategoriesEnum.Computers;
                            break;
                        case "laptops":
                            CategoryId = (int)CategoriesEnum.Laptops;
                            break;
                        default:
                            errors.Add(new Dictionary<int, string> { {row,$"Row {row}: Invalid Category. Must be one of: {string.Join(", ", validCategories)}."} });
                            break;
                    }
                    col++;

                    // discount type validation (must be one of the discount type)
                    string DiscountType = worksheet.Cells[row, col].Text;
                    int discountType = 0;
                    if (!validDiscountTypes.Contains(DiscountType.ToLower()))
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Discount Type. Must be one of: {string.Join(", ", validDiscountTypes)}."} });
                    }
                    switch (DiscountType.ToLower())
                    {
                        case "percentage":
                            discountType = (int)DiscountEnum.Percentage;
                            break;
                        case "fixed amount":
                            discountType = (int)DiscountEnum.FixedAmount;
                            break;
                        default:
                            errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Discount Type. Must be one of: {string.Join(", ", validDiscountTypes)}."} });
                            break;
                    }
                    col++;

                    // discount rate need to validate according to discount type
                    if (!decimal.TryParse(worksheet.Cells[row, col].Text, out decimal discountRate) || discountRate < 0 || Math.Round(discountRate, 2) != discountRate)
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Discount Rate. Must be a non-negative number with up to two decimal places."} });
                    }
                    col++;

                    // validate price
                    if (!decimal.TryParse(worksheet.Cells[row, col].Text, out decimal price) || price <= 0 || Math.Round(price, 2) != price)
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Price. Must be a positive number with up to two decimal places."} });
                    }
                    col++; 
                    
                    // validate Stock 
                    if (!int.TryParse(worksheet.Cells[row, col].Text, out int stock) || stock <= 0)
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Stock. Must be a positive integer."} });
                    }
                    col++;

                    // features validation
                    // need to check input string for the feature part
                    // it should be like in format: (FeatureName1, FeatureDescription1);(FeatureName2, FeatureDescription2);...
                    
                    string FeaturesString = worksheet.Cells[row, col].Text;
                    if(string.IsNullOrWhiteSpace(FeaturesString))
                    {
                        errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Features cannot be empty."} });
                    }
                    if (!string.IsNullOrWhiteSpace(FeaturesString))
                    {
                        List<string> featurePairs = FeaturesString.Split(';').Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                        foreach (string feature in featurePairs)
                        {
                            if (!System.Text.RegularExpressions.Regex.IsMatch(feature, @"^\([^,]+,\s*[^)]+\)$"))
                            {
                                errors.Add(new Dictionary<int, string> { {row, $"Row {row}: Invalid Features format. Expected: (FeatureName, FeatureDescription);..."} });
                                break;
                            }   
                        }
                    }
                    col++;
                        
                    if(worksheet.Cells[row,col].Text!=null)
                    {
                        // images validation
                        string imagesString = worksheet.Cells[row, col].Text;
                        if (!string.IsNullOrWhiteSpace(imagesString))
                        {
                            // split images by comma and trim each image name
                            List<string> imageNames = imagesString.Split(',').Select(i => i.Trim()).ToList();
                            
                            // validate each image name
                            foreach (string imageName in imageNames)
                            {
                                if (string.IsNullOrWhiteSpace(imageName) || !validImageExtensions.Any(ext => imageName.EndsWith($".{ext}", StringComparison.OrdinalIgnoreCase)))
                                {
                                    // errors.Add(new Dictionary<int, string> { {row ,$"Row {row}: Invalid Image Name '{imageName}'. Must be a valid image file with extensions: {string.Join(", ", validImageExtensions)}."} });
                                    continue;
                                }
                            }
                            
                            // if all images are valid, add them to the images list
                            foreach (string imageName in imageNames)
                            {
                                // check if image file exists in the temp folder
                                string imagePath = Path.Combine(tempPath, imageName);
                                if (File.Exists(imagePath))
                                {
                                    // add image details to the images list
                                    images.Add(new ImageHelper
                                    {
                                        count = count,
                                        productName = imageProductName,
                                        ImageName = imageName,
                                        ImagePath = imagePath
                                    });
                                }
                            }   
                        }
                    }

                    // check if row has errors in error object
                    if (!errors.Any(e => e.ContainsKey(row)))
                    {
                        product.ProductName = productName.Trim();
                        product.Description = Description.Trim();
                        product.CategoryId = CategoryId;
                        product.DiscountType = discountType;
                        product.Discount = discountRate;
                        product.Price = price;
                        product.Stocks = stock;
                        product.CreatedAt = DateTime.Now;
                        product.SellerId = userId; 
                        products.Add(product);
                        countRowsSuccess++;
                    }
                    
                    totalRow++;
                }

                // Save product to database
                if(products.Any())
                {
                    await _unitOfWork.ProductRepository.AddRangeAsync(products);
                    
                    // add features, images the products
                    string imagesFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");
                    if (!Directory.Exists(imagesFolderPath))
                    {
                        Directory.CreateDirectory(imagesFolderPath);
                    }
                    List<Feature> features = new ();
                    List<Image> SavedImages = new ();
                    for(int row = 2; row <= rowCount; row++)
                    {
                        // stop on empty rows
                        if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                            break;
                        
                        string FeaturesString = worksheet.Cells[row, 9].Text;
                        int productId = products.Where(
                            p => p.ProductName == worksheet.Cells[row, 2].Text && 
                            p.Discount.ToString() == worksheet.Cells[row, 6].Text &&
                            p.Price.ToString() == worksheet.Cells[row, 7].Text &&
                            p.Stocks.ToString() == worksheet.Cells[row, 8].Text
                        ).Select(p => p.ProductId).FirstOrDefault();
                        
                        // adding features
                        if(productId > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(FeaturesString))
                            {
                                List<string> featurePairs = FeaturesString.Split(';').Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                                foreach (string feature in featurePairs)
                                {
                                    string FeaturName = feature.Split(',')[0].Trim();
                                    FeaturName = FeaturName.Substring(1,FeaturName.Length-1).Trim();

                                    string Description = feature.Split(',')[1].Trim();
                                    Description = Description.Substring(0,Description.Length-1).Trim();

                                    features.Add(new Feature {
                                        FeatureName = FeaturName,
                                        Description = Description,
                                        ProductId = productId,
                                        CreatedAt = DateTime.Now
                                    }); 
                                }
                            }

                            // adding images 
                            List<ImageHelper> productImages = images.Where(i => i.count == int.Parse(worksheet.Cells[row, 1].Text.Trim())).ToList();
                            foreach(ImageHelper image in productImages)
                            {
                                if(image.productName == worksheet.Cells[row, 2].Text.Trim())
                                {
                                    // create unique file name
                                    string uniqueFileName = $"{Guid.NewGuid()}_{image.ImageName}";
                                    string filePath = Path.Combine(imagesFolderPath, uniqueFileName);

                                    // copy the image file to the product images folder
                                    File.Copy(image.ImagePath ?? "", filePath, true);

                                    // add new object of image into saveImages 
                                    SavedImages.Add(new Image
                                    {
                                        ProductId = productId,
                                        ImageUrl = $"/ProductImages/{uniqueFileName}"
                                    }); 
                                }   
                            }
                        }
                    }

                    // adding image and features in db
                    if(features.Any()) {await _unitOfWork.FeatureRepository.AddRangeAsync(features);}
                    if(SavedImages.Any()) {await _unitOfWork.ImageRepository.AddRangeAsync(SavedImages);}
                    // commit the transaction
                    await _unitOfWork.CommitAsync();
                }

                // return success response
                if(errors.Any() || totalRow - countRowsSuccess > 0)
                {
                    string errorMessage = "";

                    if(countRowsSuccess == 0) { errorMessage = $"<strong>No products were added due to errors. Total {totalRow - countRowsSuccess} rows had errors.</strong>"; }
                    else { errorMessage = $"<strong>Total of {countRowsSuccess} products added successfully, but {totalRow - countRowsSuccess} rows had errors.</strong>"; }
                    
                    if (errors.Any())
                    {
                        errorMessage += "<ul>";
                        foreach (Dictionary<int, string> error in errors)
                        {
                            errorMessage += $"<li>{error.First().Value}</li>";
                        }
                        errorMessage += "</ul>";
                    }
                    return new ResponsesViewModel { IsSuccess = false, Message = errorMessage };
                }
                return new ResponsesViewModel { IsSuccess = true , Message = $"Total of {countRowsSuccess} added successfully!" };
            }
        }
        catch (Exception e)
        {
            return new ResponsesViewModel { IsSuccess = false, Message = e.Message };
        }
    }



    /// <summary>
    /// method for getting dashboard data of seller
    /// </summary>
    /// <param name="email"></param>
    /// <param name="selector"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns>DashBoardViewModel</returns>
    public async Task<DashBoardViewModel> GetDashboardData(
        string email,
        int? selector = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                throw new Exception("User not found.");
            }

            // Determine the time range based on selector
            DateTime endDate = DateTime.Now;
            DateTime startDate;
            bool isMonthly = false;

            switch (selector)
            {
                case 1: // Last 30 days
                    startDate = endDate.AddDays(-30);
                    break;
                
                case 2: // last 12 month 
                    startDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-11);
                    isMonthly = true;
                    break;
                    
                case 3: // Custom date 
                    if(fromDate != null && toDate != null)
                    {
                        if(fromDate.Value == toDate.Value)
                        {
                            startDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
                            endDate = startDate.AddDays(1).AddTicks(-1);
                        }
                        else if (fromDate.Value.Year < toDate.Value.Year && 
                                 (fromDate.Value.Month < toDate.Value.Month ||
                                 (fromDate.Value.Month > toDate.Value.Month && fromDate.Value.Month - toDate.Value.Month >= 9))) {
                            isMonthly = true;
                            startDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1);
                            endDate = new DateTime(toDate.Value.Year, toDate.Value.Month, 1).AddMonths(1).AddTicks(-1);
                        } else {
                            startDate = fromDate.Value;
                            endDate = toDate.Value.AddDays(1).AddTicks(-1);
                        }
                        break;
                    }
                    else{
                        startDate = new DateTime(endDate.Year, endDate.Month, 1);
                        isMonthly = true;
                        break;
                    }
                    
                default: // Default to current month
                    startDate = new DateTime(endDate.Year, endDate.Month, 1);
                    isMonthly = true;
                    break;
            }

            // get revenew 
            List<PriceAndDateViewModel> salesData = await _unitOfWork.ProductRepository.GetSalesData(user.UserId, startDate, endDate, isMonthly);
            // get monst selling product of seller 
            List<CountAndProductwithImageViewModel> TopSellingProduct = await _unitOfWork.ProductRepository.CountTop(user.UserId, startDate, endDate);
            // get least selling product of seller
            List<CountAndProductwithImageViewModel> LeastSellingProduct = await _unitOfWork.ProductRepository.CountLeast(user.UserId, startDate, endDate);

            List<CountAndDatewithImageViewModel> NewCustomerRegistered = await _unitOfWork.UserRepository.GetCustomersData(startDate, endDate, isMonthly);

            List<PriceAndDateViewModel> ResultSalesDate = new ();
            List<CountAndDatewithImageViewModel> ResultCustomersDate = new ();

            // setup data as per selectors

            // sales chart helper
            if(salesData != null && salesData.Any())
            {
                if(isMonthly)
                {
                    // for 12 months 
                    for(DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
                    {
                        // get revenue of that perticular month
                        decimal revenue = salesData
                            .Where(s => s.Date.Year == date.Year && s.Date.Month == date.Month)
                            .Sum(s => s.Price);
                        ResultSalesDate.Add(new PriceAndDateViewModel
                        {
                            Date = new DateTime(date.Year, date.Month, 1),
                            Price = revenue,
                            dateNumber = date.ToString("yyy -MM")
                        });       
                    }
                }
                else
                {
                    // for 30 days 
                    for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                       // get revenue of that perticular day
                        decimal revenue = salesData
                            .Where(s => s.Date.Date == date.Date)
                            .Sum(s => s.Price);
                        ResultSalesDate.Add(new PriceAndDateViewModel
                        {
                            Date = date,
                            Price = revenue,
                            dateNumber = date.ToString("MMM dd")
                        });       
                    }
                }
            }

            // registered users chart
            if(NewCustomerRegistered != null && NewCustomerRegistered.Any())
            {
                if(isMonthly)
                {
                    // for 12 months 
                    for(DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
                    {
                        // get count of new customers of that perticular month
                        decimal count = NewCustomerRegistered
                            .Where(s => s.Date.Year == date.Year && s.Date.Month == date.Month)
                            .Sum(s => s.CustomerCount);
                        ResultCustomersDate.Add(new CountAndDatewithImageViewModel
                        {
                            Date = new DateTime(date.Year, date.Month, 1),
                            CustomerCount = count,
                            dateNumber = date.ToString("yyy -MM")
                        });       
                    }
                }
                else
                {
                    // for 30 days 
                    for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                       // get count of new customers of that perticular day
                        decimal count = NewCustomerRegistered
                            .Where(s => s.Date.Date == date.Date)
                            .Sum(s => s.CustomerCount);
                        ResultCustomersDate.Add(new CountAndDatewithImageViewModel
                        {
                            Date = date,
                            CustomerCount = count,
                            dateNumber = date.ToString("MMM dd")
                        });       
                    }
                }
            }
            return new DashBoardViewModel{
                priceAndDate = ResultSalesDate,
                TopSellingProduct = TopSellingProduct,
                LeastSellingProduct = LeastSellingProduct,
                CustomersData = ResultCustomersDate,
            };
        }
        catch (Exception e)
        {
            return new DashBoardViewModel();
        }
    } 




    #endregion
    #region Buyer's service
    
    /// <summary>
    /// method for getting total products count in the database
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<int> GetTotalProductsCount()
    {
        try{
            return await _unitOfWork.ProductRepository.CountAsync(x => x.IsDeleted == false);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting all products details
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns>ProductsViewModel</returns>
    public async Task<ProductsViewModel> GetProducts(string? search = null, int? category = null, int? page = 1, int pageSize = 25)
    {
        try
        {
            ProductsViewModel productsViewModel = new ();
            if(search!=null)
            {
                search = search.ToLower().Trim();
            }

            List<ProductsDeatailsViewModel>? products = await _unitOfWork.ProductRepository.GetAllProducts(search, category, page ?? 1, pageSize);
            
            
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
