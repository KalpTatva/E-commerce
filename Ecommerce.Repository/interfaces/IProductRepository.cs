using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

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

    /// <summary>
    /// get method for seller specific products
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    List<Product>? GetSellerSpecificProducts(int userId); 

    /// <summary>
    /// get product by id
    /// </summary>
    /// <param name="product"></param>
    /// <returns>Product</returns>
    public Product? GetProductById(int product);

    /// <summary>
    /// method for soft delete the product 
    /// which updates isdelete, edit and delete time by self
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    void DeleteProduct(Product product);  

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    EditProductViewModel? GetProductDetailsById(int productId);

    /// <summary>
    /// method which delete images which are no longer selected
    /// </summary>
    /// <param name="DeletedImageIdList"></param>
    void DeleteProductImagesByIds(List<int> DeletedImageIdList);

    /// <summary>
    /// method for update product details
    /// </summary>
    /// <param name="product"></param>
    void updateProducts(Product product);


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
    /// method for updating product details
    /// </summary>
    /// <param name="product"></param>
    void UpdateProduct(Product product);


    /// <summary>
    /// method for adding review in db
    /// </summary>
    /// <param name="review"></param>
    void AddReview(Review review);

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


    /// <summary>
    /// method for getting offer by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Offer? GetOfferByProductId(int productId);
}
