using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface IProductRepository : IGenericRepository<Product>
{   

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    EditProductViewModel? GetProductDetailsById(int productId);

    /// <summary>
    /// method for get all product with search filters
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<ProductsDeatailsViewModel>?> GetAllProducts(string? search = null, int? category = null);
    
    /// <summary>
    /// method for getting product details by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<productDetailsByproductIdViewModel?> GetProductDetailsByProductId(int productId);

    /// <summary>
    /// method for getting products which are user's favourite
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<ProductsDeatailsViewModel>?> GetFavouriteProductsByUserId(int userId);

    /// <summary>
    /// method for getting products for offer by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    List<ProductNameViewModel> GetProductsForOffer(int userId);

    /// <summary>
    /// method for getting all products for offer
    /// </summary>
    public List<ProductNameViewModel> GetAllProductsForOffer();



}
