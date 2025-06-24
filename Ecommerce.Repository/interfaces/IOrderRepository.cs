using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Repository.interfaces;

public interface IOrderRepository
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
    /// Method to add an order to the database.
    /// This method adds a new order to the Orders table and 
    /// saves changes to the database.
    /// </summary>
    /// <param name="order"></param>
    /// <exception cref="Exception"></exception>
    void AddOrder(Order order);
    
    /// <summary>
    /// Method to add a range of order products to the database.
    /// This method adds multiple order products to the OrderProducts table and
    /// saves changes to the database.
    /// </summary>
    /// <param name="orderProducts"></param>
    /// <exception cref="Exception"></exception>    
    void AddOrderProductRange(List<OrderProduct> orderProducts);
    
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
    Task<List<SellerOrderViewModel>?> GetSellerOrders(int userId);

    /// <summary>
    /// Method to get an order by its ID.
    /// This method retrieves a specific order from the OrderProducts table based on the provided order ID.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns>It returns an OrderProduct object if found, otherwise null.</returns>
    OrderProduct? GetOrderById(int orderId);

    /// <summary>
    /// Method to update an existing order product.
    /// This method updates the details of an order product in the OrderProducts table
    /// and saves the changes to the database.
    /// </summary>
    /// <param name="order"></param>
    void UpdateOrderProducts(OrderProduct order);


    /// <summary>
    /// Method to add an offer to the database.
    /// This method adds a new offer to the Offers table and saves changes to the database.
    /// </summary>
    void AddOffer(Offer offer);

    /// <summary>
    /// Method to add a notification to the database.
    /// </summary>
    /// <param name="notification"></param>
    /// <exception cref="Exception"></exception>
    void AddNotification(Notification notification);

    /// <summary>
    /// Method to add a range of user notification mappings to the database.
    /// </summary>
    /// <param name="userNotificationMappings"></param>
    /// <exception cref="Exception"></exception>
    void AddUserNotificationMappingRange(List<UserNotificationMapping> userNotificationMappings);

    /// <summary>
    /// Method to get the count of notifications for a user based on their user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    int GetNotificationCount(int userId);
}
