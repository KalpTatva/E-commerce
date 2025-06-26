using System.Threading.Tasks;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Service.implementation;

public class OrderService : IOrderService
{

    private readonly IProductRepository _productRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IUserRepository _userRepository;
    private readonly ICartRepository _cartRepository;
    private readonly EcommerceContext _context;
    private readonly IOrderRepository _orderRepository;

    public OrderService(
    IProductRepository productRepository, 
    IWebHostEnvironment webHostEnvironment,
    IUserRepository userRepository,
    ICartRepository cartRepository,
    EcommerceContext context,
    IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _webHostEnvironment = webHostEnvironment;
        _userRepository = userRepository; 
        _orderRepository = orderRepository;
        _context = context;
        _cartRepository = cartRepository;
    }

    /// <summary>
    /// Method to get order details for a given session object and user email
    /// </summary>
    /// <param name="obj">ObjectSessionViewModel containing order details</param>
    /// <param name="email">User's email address</param>
    /// <returns>OrderViewModel containing order details</returns>
    public async Task<OrderViewModel> GetDetailsForOrder(ObjectSessionViewModel obj, string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            Profile? profile = user!=null ? _userRepository.GetProfileById(user.ProfileId) : null;
            
            if(user==null && obj.orders == null && profile==null)
            {
                throw new Exception("User not found");
            }

            OrderViewModel result = new ();

            // method for getting order details based on cart ids 
            List<productAtOrderViewModel>? orderList = await _orderRepository.GetDetailsForOrders(obj.orders ?? new List<int>()); 
            if(orderList != null)
            {
            
                result.ordersList = orderList;
                result.UserId = user?.UserId ?? 0;
                result.Address = profile?.Address;
                result.Phone = profile?.PhoneNumber;
                result.FirstName = profile?.FirstName;
                result.LastName = profile?.LastName;
                result.objSession = obj;
                result.PinCode = profile?.Pincode ?? 0;
                result.CountryName = _userRepository.GetCountryNameById(profile?.CountryId ?? 0);
                result.CityName = _userRepository.GetCityNameById(profile?.CityId ?? 0);
                result.StateName = _userRepository.GetStateNameById(profile?.StateId ?? 0);

                return result;

            }
            throw new Exception("Order details not found!");
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to get order details for a single order based on the session object and user email
    /// </summary>
    /// <param name="obj">ObjectSessionViewModel containing order details</param>
    /// <param name="email">User's email address</param>
    /// <returns>OrderViewModel containing order details</returns>
    public async Task<OrderViewModel> GetDetailsForSingleOrder(ObjectSessionViewModel obj, string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            Profile? profile = user!=null ? _userRepository.GetProfileById(user.ProfileId) : null;
            
            if(user==null && obj.orders == null && profile==null)
            {
                throw new Exception("User not found");
            }

            OrderViewModel result = new ();

            // method for getting order details based on cart ids 
            List<productAtOrderViewModel>? orderList = await _orderRepository.GetDetailsForOrdersByProductId(obj.orders ?? new List<int>()); 
            if(orderList != null)
            {
            
                result.ordersList = orderList;
                result.UserId = user?.UserId ?? 0;
                result.Address = profile?.Address;
                result.Phone = profile?.PhoneNumber;
                result.FirstName = profile?.FirstName;
                result.LastName = profile?.LastName;
                result.objSession = obj;
                result.PinCode = profile?.Pincode ?? 0;
                result.CountryName = _userRepository.GetCountryNameById(profile?.CountryId ?? 0);
                result.CityName = _userRepository.GetCityNameById(profile?.CityId ?? 0);
                result.StateName = _userRepository.GetStateNameById(profile?.StateId ?? 0);
                return result;

            }
            throw new Exception("Order details not found!");
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to place an order based on the session object and user ID.
    /// This method retrieves order details, creates a new order, adds order products,
    /// updates product quantities, and marks cart items as deleted.
    /// </summary>
    /// <param name="objSession">ObjectSessionViewModel containing order details</param>
    /// <param name="UserId">ID of the user placing the order</param>
    /// <param name="isByProductId">Flag to indicate if the order is by product ID</param>
    public async Task<ResponsesViewModel?> PlaceOrder(
    ObjectSessionViewModel objSession, 
    int UserId, 
    bool isByProductId = false)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            List<productAtOrderViewModel>? orderList;

            // Choose which method to fetch order details
            if (isByProductId){
                orderList = await _orderRepository.GetDetailsForOrdersByProductId(objSession.orders ?? new List<int>());
            }else{
                orderList = await _orderRepository.GetDetailsForOrders(objSession.orders ?? new List<int>());
            }

            if (orderList != null)
            {
                // Place order
                Order order = new()
                {
                    BuyerId = UserId,
                    Amount = objSession.totalPrice,
                    Status = (int)OrderStatusEnum.Pending,
                    TotalQuantity = (int)objSession.totalQuantity,
                    TotalDiscount = objSession.totalDiscount,
                };
                _orderRepository.AddOrder(order);

                
                // Add order details in orderproduct
                List<OrderProduct> orderProducts = new();
                foreach (productAtOrderViewModel model in orderList)
                {
                    decimal price = model.Price;
                    decimal discount = 0;
                    int quantity = model.Quantity;
                    
                    if (model.DiscountType == (int)DiscountEnum.FixedAmount)
                    {
                        discount = (model.Discount ?? 0) * model.Quantity;
                        price = (model.Price * model.Quantity) - (model.Discount ?? 0);
                    }
                    else if (model.DiscountType == (int)DiscountEnum.Percentage)
                    {
                        discount = (model.Price * (model.Discount ?? 0) * model.Quantity) / 100;
                        price = (model.Price * model.Quantity) - discount;
                    }


                    // If there's an offer, apply it
                    if (model.Offer != null && model.Offer.DiscountRate != null)
                    {

                        switch (model.Offer.OfferType)
                        {
                            case (int)OfferTypeEnum.Percentage:
                                price -= (price * (model.Offer.DiscountRate??0) * model.Quantity) / 100;
                                discount += (model.Price * (model.Offer.DiscountRate??0 ) * model.Quantity) / 100;
                            break;
                            case (int)OfferTypeEnum.FixedPrice:
                                price -= (model.Offer.DiscountRate ?? 0) * model.Quantity;
                                discount += (model.Offer.DiscountRate ?? 0) * model.Quantity;
                            break; 
                            case (int)OfferTypeEnum.BOGO:
                                quantity = model.Quantity * 2;
                                break;
                            default:
                                break;
                        }
                    }

                    orderProducts.Add(new OrderProduct()
                    {
                        OrderId = order.OrderId,
                        ProductId = model.ProductId,
                        Quantity = quantity,
                        PriceWithDiscount = price
                    });
                }
                _orderRepository.AddOrderProductRange(orderProducts);

                // update product's quantity in product table
                foreach (productAtOrderViewModel model in orderList)
                {
                    Product? product = _productRepository.GetProductById(model.ProductId);
                    if (product != null)
                    {
                        product.Stocks -= model.Quantity;
                        _productRepository.UpdateProduct(product);
                    }
                }

                // Mark cart items as deleted
                _cartRepository.DeleteCartByIdsRange(objSession.orders ?? new List<int>());

                await transaction.CommitAsync();

                return new ResponsesViewModel()
                {
                    IsSuccess = true,
                    Message = "order placed successfully"
                };
            }

            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = "Error occurred while placing order!"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = e.Message
            };
        }
    }


    /// <summary>
    /// Method to retrieve the order history for a user based on their email address.
    /// This method fetches the user by email, retrieves their order details,
    /// and returns a list of MyOrderViewModel containing the order history.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>List of MyOrderViewModel containing order history</returns>
    public async Task<List<MyOrderViewModel>> GetMyOrderHistoryByEmail(string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            List<MyOrderViewModel>? myOrderViewModel = new ();
            if(user != null)
            {
                myOrderViewModel = await _orderRepository.GetMyOrderDetails(user.UserId); 
            }

            return myOrderViewModel ?? new List<MyOrderViewModel>();
        }
        catch
        {
            return new List<MyOrderViewModel>();
        }
    }

    /// <summary>
    /// Method to retrieve the seller's orders based on their email address.
    /// This method fetches the user by email, retrieves their seller orders,
    /// and returns a list of SellerOrderViewModel containing the seller's order details.
    /// </summary>
    /// <param name="email">Seller's email address</param>
    /// <returns>List of SellerOrderViewModel containing seller's order details</returns>
    public async Task<List<SellerOrderViewModel>?> GetSellerOrders(string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            if(user == null)
            {
                throw new Exception("User not found");
            }
            List<SellerOrderViewModel>? sellerOrders = await _orderRepository.GetSellerOrders(user.UserId);
            return sellerOrders ?? new List<SellerOrderViewModel>();
        }
        catch
        {
            return new List<SellerOrderViewModel>();
        }
    }

    /// <summary>
    /// Method to update the status of an order based on the provided order ID and status.
    /// This method retrieves the order by ID, updates its status,
    /// and if the status is "cancelled", it restores the product stock.
    /// </summary>
    /// <param name="orderId">ID of the order to be updated</param>
    /// <param name="status">New status for the order (e.g., "pending", "shipped", "delivered", "cancelled")</param>
    /// <returns>ResponsesViewModel indicating success or failure of the operation</returns>
    public ResponsesViewModel UpdateOrderStatus(int orderId, string status)
    {
        try
        {
            OrderProduct? order = _orderRepository.GetOrderById(orderId);
            if (order == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Order not found"
                };
            }
            
            string Message = "";

            switch (status.ToLower())
            {
                case "pending":
                    order.Status = (int)OrderStatusEnum.Pending;
                    Message = "Order status updated to pending";
                    break;
                case "shipped":
                    order.Status = (int)OrderStatusEnum.Shipped;
                    Message = "Order status updated to shipped";
                    break;
                case "delivered":
                    order.Status = (int)OrderStatusEnum.Delivered;
                    Message = "Order status updated to delivered";
                    break;
                case "cancelled":
                    order.Status = (int)OrderStatusEnum.Cancelled;
                    Message = "Order status updated to cancelled";
                    // If order is cancelled, we can also update the product stock
                    Product? product = _productRepository.GetProductById(order.ProductId);
                    if (product != null)
                    {
                        product.Stocks += order.Quantity; // Restoring stock
                        _productRepository.UpdateProduct(product);
                    }
                    break;
                default:
                    throw new Exception("Invalid status");
            }
            
            _orderRepository.UpdateOrderProducts(order);
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = Message
            };
        }
        catch (Exception e)
        {
            return  new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = e.Message
            };
        }
    }

    /// <summary>
    /// method for adding new offer to the product
    /// </summary>
    /// <param name="model"></param>
    /// <returns>responsesviewmodel</returns>
    public ResponsesViewModel AddOffer(OfferViewModel model)
    {
        try
        {
            if (model == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Offer model cannot be null"
                };
            }

            Offer? offer1 = _productRepository.GetOfferByProductId(model.ProductId);
            if (offer1 != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Offer already exists for this product"
                };
            }

            Offer offer = new Offer()
            {
                ProductId = model.ProductId,
                OfferType = model.OfferType,
                DiscountRate = model.DiscountRate ?? 0,
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate.Date.AddTicks(-1), // Start of the day
                EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1), // End of the day
            };
            
            // Add the offer to the repository
            _orderRepository.AddOffer(offer);
           


            // add notification to the users who are interested in this product
            // that means who have produts in their favorite list
        
            // first need to make notification message 
            Product? product = _productRepository.GetProductById(model.ProductId);
            string notificationMessage = $"New offer on product {product?.ProductName}: {model.Title} - {model.Description}";
            
            // next is to add this message into notification table
            Notification notification = new Notification()
            {
                Notification1 = notificationMessage,
                ProductId = model.ProductId,
                CreatedAt = DateTime.Now
            };
            _orderRepository.AddNotification(notification);

            // next is to get users who have this product in their favourite list
            List<User>? users = _userRepository.GetUsersByProductIdFromFavourite(model.ProductId);

            // and add this notification to their usernotificationmapping table
            List<UserNotificationMapping> userNotificationMapping = new List<UserNotificationMapping>();
            if (users != null && users.Count > 0)
            {
                foreach (User user in users)
                {
                    userNotificationMapping.Add(
                        new UserNotificationMapping()
                        {
                            UserId = user.UserId,
                            NotificationId = notification.NotificationId,
                        }
                    );
                    
                }
            }
            _orderRepository.AddUserNotificationMappingRange(userNotificationMapping);        

            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "Offer added successfully"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = e.Message
            };
        }
    }

    /// <summary>
    /// Method to get the count of notifications for a user based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>int</returns>
    /// <exception cref="Exception"></exception>
    public int GetNotificationCount(string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return _orderRepository.GetNotificationCount(user.UserId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to retrieve notifications for a user based on their email address.
    /// This method fetches the user by email and retrieves their notifications.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>List of Notification containing user's notifications</returns>
    public List<Notification>? GetNotificationsByEmail(string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return _orderRepository.GetNotificationsByUserId(user.UserId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Method to mark all notifications as read for a user based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <exception cref="Exception"></exception>
    public void MarkNotificationAsRead(string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            List<UserNotificationMapping>? notifications = _orderRepository.GetUserNotificationMapping(user.UserId);
            if (notifications != null && notifications.Count > 0)
            {
                foreach (UserNotificationMapping notification in notifications)
                {
                    notification.ReadAll = true;
                    notification.EditedAt = DateTime.Now;
                }
                _orderRepository.UpdateNotificationRange(notifications);
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


}
