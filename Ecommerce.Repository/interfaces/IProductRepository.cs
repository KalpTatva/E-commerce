using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IProductRepository
{
    /// <summary>
    /// method for adding product into db
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    void AddProduct(Product product);
    
    /// <summary>
    /// method for adding multiple images 
    /// </summary>
    /// <param name="images"></param>
    /// <exception cref="Exception"></exception>
    void AddProductImages(List<Image> images);


    List<Product>? GetSellerSpecificProducts(int userId);
}
