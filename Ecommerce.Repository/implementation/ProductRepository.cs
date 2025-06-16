using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

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


    public List<Product>? GetSellerSpecificProducts(int userId){
        try
        {
            return _context.Products.Where(x => x.SellerId == userId).ToList();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
