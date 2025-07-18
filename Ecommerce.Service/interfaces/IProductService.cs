using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Service.interfaces;

public interface IProductService
{
    /// <summary>
    /// method service for adding product
    /// </summary>
    /// <param name="model"></param>
    /// <param name="email"></param>
    /// <param name="List<Features>"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> AddProduct(AddProductViewModel model, string email, List<Feature> features);

    /// <summary>
    /// get seller specific data from db
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<List<Product>?> GetSellerSpecificProductsByEmail(string email);

    /// <summary>
    /// soft delete of product
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> DeleteProductById(int id);

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    EditProductViewModel? GetProductDetailsById(int productId);

    /// <summary>
    /// method for
    ///     1. delete images which are no longer in model (have list of images for delete)
    ///     2. udpate features (delete all previouse and add all new one)
    ///     3. update product details
    /// </summary>
    /// <param name="model"></param>
    /// <param name="features"></param>
    /// <param name="DeletedImageIdList"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> UpdateProductDetails(EditProductViewModel model, List<Feature>? features, List<int>? DeletedImageIdList);

    /// <summary>
    /// method for getting all products details
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    Task<ProductsViewModel> GetProducts(string? search = null, int? category = null);
    
    /// <summary>
    /// method for getting product by product id and email 
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<productDetailsByproductIdViewModel?> GetProductById(int productId, string email);
    
    /// <summary>
    /// method for getting user wise favourite products details
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<ProductsViewModel> GetFavouriteProducts(string email);
    
    /// <summary>
    /// method for updating state of favourite button
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<ResponsesViewModel> UpdateFavourite(int productId,string? email = null);
    
    /// <summary>
    /// mehtod for getting details of favourite products list by user emails
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    List<int> GetFavouritesByEmail(string email);


    /// <summary>
    /// method for adding product into cart
    /// </summary>
    /// <param name="email"></param>
    /// <param name="productId"></param>
    /// <returns>responsesviewmodel</returns>
    Task<ResponsesViewModel> AddToCart(string email, int productId);

    /// <summary>
    /// method which calculate major properties of cart and returns cart items of user
    /// </summary>
    /// <param name="email"></param>
    /// <returns>CartViewModel</returns>
    Task<CartViewModel> GetCartDetails(string email);


    /// <summary>
    /// method for updating cart product's quantity values and displaying updated totals 
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="cartId"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<CartUpdatesViewModel> UpdateQuantityAtCart(int quantity, int cartId, string email);

    /// <summary>
    /// method for delete product from cart (soft delete)
    /// </summary>
    /// <param name="cartId"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> DeleteCartFromList(int cartId);

    /// <summary>
    /// method for adding review to the product
    /// </summary>
    /// <param name="orderProductId"></param>
    /// <param name="rating"></param>
    /// <param name="productId"></param>
    /// <param name="reviewText"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> AddReview(int orderProductId,decimal rating, int productId, string reviewText,string email);



    /// <summary>
    /// method to check product stock by cart ids
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<ResponsesViewModel> CheckProductStockByCartId(string email);


    /// <summary>
    /// method for getting all products for offer by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns>List<ProductNameViewModel></returns>
    /// <exception cref="Exception"></exception>
    List<ProductNameViewModel> GetProductsForOffer(string email);
}
