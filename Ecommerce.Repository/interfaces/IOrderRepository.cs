using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{

    /// <summary>
    /// Get details for orders based on cart IDs.
    /// This method retrieves product details for the specified cart IDs, 
    /// including product name, price, stocks, discount type, discount amount, quantity, and associated images.
    /// </summary>
    /// <param name="cartIds"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<productAtOrderViewModel>?> GetDetailsForOrders(List<int> cartIds);
    
    /// <summary>
    /// Get details for orders based on product IDs.
    /// This method retrieves product details for the specified product IDs,
    /// including product name, price, stocks, discount type, discount amount, 
    /// quantity, and associated images.
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<productAtOrderViewModel>?> GetDetailsForOrdersByProductId(List<int> productId);
    
    /// <summary>
    /// Method to get the order details for a specific user.
    /// This method retrieves a list of orders placed by the user, including order items and their details.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>It returns a list of MyOrderViewModel objects containing order and item details.</returns>
    /// <exception cref="Exception"></exception>
    Task<List<MyOrderViewModel>?> GetMyOrderDetails(int userId);

    /// <summary>
    /// Method to get the seller's orders.
    /// This method retrieves a list of orders placed for products sold by the seller, including order details and buyer information.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>It returns a list of SellerOrderViewModel objects containing order and buyer details.</returns>
    /// <exception cref="Exception"></exception>
    Task<List<SellerOrderViewModel>?> GetSellerOrders(int userId, int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Method to get the count of orders for a seller based on their user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>int</returns>
    /// <exception cref="Exception"></exception>
    int GetSellersOrderTotalCount(int userId);


}
