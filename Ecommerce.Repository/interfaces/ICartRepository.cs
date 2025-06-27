using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface ICartRepository
{
    /// <summary>
    /// method for add in cart
    /// </summary>
    /// <param name="cart"></param>
    /// <exception cref="Exception"></exception>
    public void AddToCart(Cart cart);

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
    /// method for getting cart by user id and product id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns>Cart</returns>
    Cart? GetCartByUserIdAndProductId(int userId, int productId);


    /// <summary>
    /// get product details by cart id
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="userId"></param>
    /// <returns>product</returns>
    /// <exception cref="Exception"></exception>
    Product? GetProductByCartId(int cartId,int userId);
}
