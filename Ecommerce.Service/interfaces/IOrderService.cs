using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Service.interfaces;

public interface IOrderService
{
    /// <summary>
    /// Method to get order details for a given session object and user email
    /// </summary>
    /// <param name="obj">ObjectSessionViewModel containing order details</param>
    /// <param name="email">User's email address</param>
    /// <returns>OrderViewModel containing order details</returns>
    Task<OrderViewModel> GetDetailsForOrder(ObjectSessionViewModel obj, string email);

    /// <summary>
    /// Method to get order details for a single order based on the session object and user email
    /// </summary>
    /// <param name="obj">ObjectSessionViewModel containing order details</param>
    /// <param name="email">User's email address</param>
    /// <returns>OrderViewModel containing order details</returns>
    Task<OrderViewModel> GetDetailsForSingleOrder(ObjectSessionViewModel obj, string email);

    /// <summary>
    /// Method to place an order based on the session object and user ID.
    /// This method retrieves order details, creates a new order, adds order products,
    /// updates product quantities, and marks cart items as deleted.
    /// </summary>
    /// <param name="objSession">ObjectSessionViewModel containing order details</param>
    /// <param name="UserId">ID of the user placing the order</param>
    /// <param name="isByProductId">Flag to indicate if the order is by product ID</param>
    Task<ResponsesViewModel?> PlaceOrder(
        ObjectSessionViewModel objSession, 
        int UserId, 
        string rzp_paymentid,
        string rzp_orderid,
        bool isByProductId = false);

    /// <summary>
    /// Method to retrieve the order history for a user based on their email address.
    /// This method fetches the user by email, retrieves their order details,
    /// and returns a list of MyOrderViewModel containing the order history.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>List of MyOrderViewModel containing order history</returns>
    Task<List<MyOrderViewModel>> GetMyOrderHistoryByEmail(string email);


    /// <summary>
    /// Method to retrieve the seller's orders based on their email address.
    /// This method fetches the user by email, retrieves their seller orders,
    /// and returns a list of SellerOrderViewModel containing the seller's order details.
    /// </summary>
    /// <param name="email">Seller's email address</param>
    /// <returns>List of SellerOrderViewModel containing seller's order details</returns>
    Task<List<SellerOrderViewModel>?> GetSellerOrders(string email, int pageNumber = 1, int pageSize = 5);


    /// <summary>
    /// Method to update the status of an order based on the provided order ID and status.
    /// This method retrieves the order by ID, updates its status,
    /// and if the status is "cancelled", it restores the product stock.
    /// </summary>
    /// <param name="orderId">ID of the order to be updated</param>
    /// <param name="status">New status for the order (e.g., "pending", "shipped", "delivered", "cancelled")</param>
    /// <returns>ResponsesViewModel indicating success or failure of the operation</returns>
    Task<ResponsesViewModel> UpdateOrderStatus(int orderId, string status);

    /// <summary>
    /// Method to get the total count of orders for a seller based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>int: count of orders</returns>
    /// <exception cref="Exception"></exception>
    int GetSellersOrderTotalCount(string email);

    /// <summary>
    /// method for adding new offer to the product
    /// </summary>
    /// <param name="model"></param>
    /// <returns>responsesviewmodel</returns>
    Task<ResponsesViewModel> AddOffer(OfferViewModel model);


    /// <summary>
    /// Method to get the count of notifications for a user based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>int</returns>
    /// <exception cref="Exception"></exception>
    int GetNotificationCount(string email);

    /// <summary>
    /// Method to retrieve notifications for a user based on their email address.
    /// This method fetches the user by email and retrieves their notifications.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>List of Notification containing user's notifications</returns>
    List<Notification>? GetNotificationsByEmail(string email);

    /// <summary>
    /// Method to mark all notifications as read for a user based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <exception cref="Exception"></exception>
    Task MarkNotificationAsRead(string email);


    Task<PaymentViewModel> CreatePayment(int UserId,ObjectSessionViewModel objRes);

}
