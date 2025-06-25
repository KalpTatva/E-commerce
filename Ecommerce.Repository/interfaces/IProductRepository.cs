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
    /// method for adding range of features into feature table
    /// </summary>
    /// <param name="features"></param>
    void AddFeaturesRange(List<Feature> features);  

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
    /// method for getting features of perticular product by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    List<Feature>? GetFeaturesByProductId(int productId);

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
    /// hard delete on features
    /// </summary>
    /// <param name="feature"></param>
    void DeleteFeature(Feature feature);

    /// <summary>
    /// method for updating range of features 
    /// </summary>
    /// <param name="features"></param>
    /// <exception cref="Exception"></exception>
    void updateFeaturesRange(List<Feature> features);

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
    /// method for fetch favourites by user and product id
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="ProductId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Favourite? GetFavouriteByIds(int UserId,int ProductId);

    /// <summary>
    /// method for dropping favourite tupple from db
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    void dropFavourite(Favourite favourite);

    /// <summary>
    /// method for add tupple in favourite
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    void AddFavourite(Favourite favourite);

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>    
    List<int> GetFavouriteByUserId(int userId);

    /// <summary>
    /// method for getting products which are user's favourite
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<ProductsDeatailsViewModel>?> GetFavouriteProductsByUserId(int userId);

    /// <summary>
    /// method for add in cart
    /// </summary>
    /// <param name="cart"></param>
    /// <exception cref="Exception"></exception>
    void AddToCart(Cart cart);

    /// <summary>
    /// method which gets cart data based on user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    List<productAtCartViewModel> GetproductAtCart(int userId);

    /// <summary>
    /// method for updating cart's quantity only
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="quantity"></param>
    /// <exception cref="Exception"></exception>
    void UpdateCartById(int cartId,int quantity);

    /// <summary>
    /// soft delete method for updating delete boolean = true
    /// </summary>
    /// <param name="cartId"></param>
    /// <exception cref="Exception"></exception>
    void DeleteCartById(int cartId);

    /// <summary>
    /// soft delete implementation for cart items 
    /// </summary>
    /// <param name="cartIds"></param>
    /// <exception cref="Exception"></exception>
    void DeleteCartByIdsRange(List<int> cartIds);

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
    /// method for getting cart by user id and product id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns>Cart</returns>
    Cart? GetCartByUserIdAndProductId(int userId, int productId);
    

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
