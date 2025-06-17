using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.implementation;

public class ProductRepository : IProductRepository
{
    private readonly EcommerceContext _context;

    public ProductRepository(EcommerceContext context)
    {
        _context = context;
    }


    /// <summary>
    /// method for adding product into db
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    public void AddProduct(Product product)
    {
        try
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }

    }

    /// <summary>
    /// method for adding multiple images 
    /// </summary>
    /// <param name="images"></param>
    /// <exception cref="Exception"></exception>
    public void AddProductImages(List<Image> images)
    {
        try
        {
            _context.Images.AddRange(images);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// get method for seller specific products
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<Product>? GetSellerSpecificProducts(int userId){
        try
        {
            return _context.Products.Where(x => x.SellerId == userId && x.IsDeleted == false ).OrderBy(x => x.ProductId).ToList();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for adding range of features into feature table
    /// </summary>
    /// <param name="features"></param>
    public void AddFeaturesRange(List<Feature> features)
    {
        try
        {
            _context.Features.AddRange(features);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// get product by id
    /// </summary>
    /// <param name="product"></param>
    /// <returns>Product</returns>
    public Product? GetProductById(int product)
    {
        try
        {
            return _context.Products.FirstOrDefault(x => x.ProductId == product && x.IsDeleted == false);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for soft delete the product 
    /// which updates isdelete, edit and delete time by self
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void DeleteProduct(Product product)
    {
        try
        {
            product.IsDeleted = true;
            product.EditedAt = DateTime.Now;
            product.DeletedAt = DateTime.Now;
            _context.Products.Update(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for getting features of perticular product by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public List<Feature>? GetFeaturesByProductId(int productId)
    {
        try
        {
            return _context.Features.Where(x => x.ProductId == productId).ToList();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
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
            EditProductViewModel model = new ();
            Product? product = _context.Products.FirstOrDefault(x => x.ProductId == productId && x.IsDeleted==false);
            List<Feature>? features = _context.Features.Where(x => x.ProductId == productId).ToList();
            List<Image>? images = _context.Images.Where(x => x.ProductId == productId).ToList();
            if(product!=null)
            {
                model.ProductId = product.ProductId;
                model.ProductName = product.ProductName;
                model.Description = product.Description;
                model.CategoryId = product.CategoryId;
                model.Price = product.Price;
                model.Stocks = product.Stocks;
                model.SellerId = product.SellerId;
                model.DiscountType = product.DiscountType;
                model.Discount = product.Discount;
                model.Features = features ?? new List<Feature>();
                model.Images = images ?? new List<Image>();

                return model;
            }

            return null;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
        

    } 

    /// <summary>
    /// method which delete images which are no longer selected
    /// </summary>
    /// <param name="DeletedImageIdList"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteProductImagesByIds(List<int> DeletedImageIdList)
    {
        try
        {
            List<Image>? imagesToDelete = _context.Images.Where(image => DeletedImageIdList.Contains(image.ImageId)).ToList();
            _context.Images.RemoveRange(imagesToDelete);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    /// <summary>
    /// hard delete on features
    /// </summary>
    /// <param name="feature"></param>
    public void DeleteFeature(Feature feature)
    {
        try
        {
            _context.Features.Remove(feature);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for update product details
    /// </summary>
    /// <param name="product"></param>
    public void updateProducts(Product product)
    {
        try
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for add new feature
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    public void AddFeature(Product product)
    {
        try
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for updating range of features 
    /// </summary>
    /// <param name="features"></param>
    /// <exception cref="Exception"></exception>
    public void updateFeaturesRange(List<Feature> features)
    {
        try
        {
            _context.Features.UpdateRange(features);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
