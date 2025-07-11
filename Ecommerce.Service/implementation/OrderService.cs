using System.Threading.Tasks;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Service.implementation;

public class OrderService : IOrderService
{
    private readonly EcommerceContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
    EcommerceContext context,
    IUnitOfWork unitOfWork,
    IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
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
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            Profile? profile = user!=null ? await _unitOfWork.ProfileRepository.GetByIdAsync(user.ProfileId) : null;
            
            if(user==null && obj.orders == null && profile==null)
            {
                throw new Exception("User not found");
            }

            OrderViewModel result = new ();

            // method for getting order details based on cart ids 
            List<productAtOrderViewModel>? orderList = await  _unitOfWork.OrderRepository.GetDetailsForOrders(obj.orders ?? new List<int>()); 
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
                result.CountryName = _unitOfWork.CountryRepository.GetCountryNameById(profile?.CountryId ?? 0);
                result.CityName = _unitOfWork.CityRepository.GetCityNameById(profile?.CityId ?? 0);
                result.StateName = _unitOfWork.StateRepository.GetStateNameById(profile?.StateId ?? 0);

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
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            Profile? profile = user!=null ? await _unitOfWork.ProfileRepository.GetByIdAsync(user.ProfileId) : null;

            
            if(user==null && obj.orders == null && profile==null)
            {
                throw new Exception("User not found");
            }

            OrderViewModel result = new ();

            // method for getting order details based on cart ids 
            List<productAtOrderViewModel>? orderList = await  _unitOfWork.OrderRepository.GetDetailsForOrdersByProductId(obj.orders ?? new List<int>()); 
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
                result.CountryName = _unitOfWork.CountryRepository.GetCountryNameById(profile?.CountryId ?? 0);
                result.CityName = _unitOfWork.CityRepository.GetCityNameById(profile?.CityId ?? 0);
                result.StateName = _unitOfWork.StateRepository.GetStateNameById(profile?.StateId ?? 0);

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
    string rzp_paymentid,
    string rzp_orderid,
    bool isByProductId = false)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            List<productAtOrderViewModel>? orderList;

            // Choose which method to fetch order details
            if (isByProductId){
                orderList = await  _unitOfWork.OrderRepository.GetDetailsForOrdersByProductId(objSession.orders ?? new List<int>());
            }else{
                orderList = await  _unitOfWork.OrderRepository.GetDetailsForOrders(objSession.orders ?? new List<int>());
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
                    RzpPaymentId = rzp_paymentid,
                    RzpOrderId = rzp_orderid,
                };
                await _unitOfWork.OrderRepository.AddAsync(order);

                
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
                
                await _unitOfWork.OrderProductRepository.AddRangeAsync(orderProducts);

                // update product's quantity in product table
                foreach (productAtOrderViewModel model in orderList)
                {
                    Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(model.ProductId);
                    if (product != null)
                    {
                        product.Stocks -= model.Quantity;
                        await _unitOfWork.ProductRepository.UpdateAsync(product);
                    }
                }

                // Mark cart items as deleted
                // await _unitOfWork.CartRepository.DeleteCartByIdsRangeAsync(objSession.orders ?? new List<int>());
                List<Cart>? cartsToDelete = await _unitOfWork.CartRepository.FindAllAsync(c => objSession.orders != null && objSession.orders.Contains(c.CartId));
                if(cartsToDelete != null)
                {

                    foreach (var cart in cartsToDelete)
                    {
                        cart.IsDeleted = true;
                        cart.DeletedAt = DateTime.Now;
                    }
                    await _unitOfWork.CartRepository.UpdateRangeAsync(cartsToDelete);
                }

                // now adding notification for seller after placing order
                // User? user = _userRepository.GetUserById(UserId);
                User? user = await _unitOfWork.UserRepository.GetByIdAsync(UserId);

                string notification = $"New order placed pleace check your dashboard for more details. Order ID: {order.OrderId}"; 

                Notification notificationModel = new Notification()
                {
                    Notification1 = notification,
                    ProductId = orderProducts.Count > 0 ? orderProducts[0].ProductId : 0,
                    CreatedAt = DateTime.Now
                };
                // _notificationRepository.AddNotification(notificationModel);
                await _unitOfWork.NotificationRepository.AddAsync(notificationModel);

                // adding notification to seller at mapping table 
                List<UserNotificationMapping> userNotificationMappings = new List<UserNotificationMapping>();
                foreach (productAtOrderViewModel model in orderList)
                {
                    Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(model.ProductId);
                    userNotificationMappings.Add(new UserNotificationMapping()
                    {
                        UserId = product?.SellerId ?? 0,
                        NotificationId = notificationModel.NotificationId,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
                
                await _unitOfWork.UserNotificationMappingRepository.AddRangeAsync(userNotificationMappings);
            

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
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            List<MyOrderViewModel>? myOrderViewModel = new ();
            if(user != null)
            {
                myOrderViewModel = await _unitOfWork.OrderRepository.GetMyOrderDetails(user.UserId); 
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
    public async Task<List<SellerOrderViewModel>?> GetSellerOrders(string email, int pageNumber = 1, int pageSize = 5)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user == null)
            {
                throw new Exception("User not found");
            }
            List<SellerOrderViewModel>? sellerOrders = await  _unitOfWork.OrderRepository.GetSellerOrders(user.UserId, pageNumber, pageSize);
            return sellerOrders ?? new List<SellerOrderViewModel>();
        }
        catch
        {
            return new List<SellerOrderViewModel>();
        }
    }

    /// <summary>
    /// Method to get the total count of orders for a seller based on their email address.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>int: count of orders</returns>
    /// <exception cref="Exception"></exception>
    public int GetSellersOrderTotalCount(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return  _unitOfWork.OrderRepository.GetSellersOrderTotalCount(user.UserId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
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
    public async Task<ResponsesViewModel> UpdateOrderStatus(int orderId, string status)
    {
        try
        {
            OrderProduct? order = await _unitOfWork.OrderProductRepository.FindAsync(op => op.OrderProductId == orderId && op.IsDeleted == false);
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
                    Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(order.ProductId);

                    if (product != null)
                    {
                        product.Stocks += order.Quantity; // Restoring stock
                        await _unitOfWork.ProductRepository.UpdateAsync(product);
                    }
                    break;
                default:
                    throw new Exception("Invalid status");
            }
            
            await _unitOfWork.OrderProductRepository.UpdateAsync(order);
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
    public async Task<ResponsesViewModel> AddOffer(OfferViewModel model)
    {
        try
        {
            // validations for the offer 
            if (model == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Offer model cannot be null!"
                };
            }

            // check if the values for discount are valid or not
            Product? product = await _unitOfWork.ProductRepository.FindAsync(x => x.ProductId == model.ProductId && x.IsDeleted == false);

            if (product == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Product does not exists!"
                };
            }

            if((int)OfferTypeEnum.Percentage == model.OfferType && (model.DiscountRate <= 0 || model.DiscountRate > 100))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be between 0 and 100 for percentage discount type"
                };
            }

            int prices = (int)DiscountEnum.Percentage == product.DiscountType ?
                         (int)(product.Price * (product.Discount ?? 0) / 100) :
                         (int)product.Price - (int)(product.Discount ?? 0) ;

            if((int)OfferTypeEnum.FixedPrice == model.OfferType && (model.DiscountRate <= 0 || model.DiscountRate > prices))
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Discount must be greater than zero and less than or equal to price for fixed amount discount type"
                };
            }

            Offer? offer1 = await _unitOfWork.OfferRepository.FindAsync(
                o => o.ProductId == model.ProductId 
                && o.StartDate.Date <= DateTime.Now.Date 
                && o.EndDate.Date > DateTime.Now.Date);

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
            await _unitOfWork.OfferRepository.AddAsync(offer);
           


            // add notification to the users who are interested in this product
            // that means who have produts in their favorite list
        
            // first need to make notification message 
            string notificationMessage = $"New offer on product {product?.ProductName}: {model.Title} - {model.Description}";
            
            // next is to add this message into notification table
            Notification notification = new Notification()
            {
                Notification1 = notificationMessage,
                ProductId = model.ProductId,
                CreatedAt = DateTime.Now
            };
            // _notificationRepository.AddNotification(notification);
            await _unitOfWork.NotificationRepository.AddAsync(notification);

            // next is to get users who have this product in their favourite list
            List<User>? users =  _unitOfWork.UserRepository.GetUsersByProductIdFromFavourite(model.ProductId);

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
            await _unitOfWork.UserNotificationMappingRepository.AddRangeAsync(userNotificationMapping);


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
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return  _unitOfWork.NotificationRepository.GetNotificationCount(user.UserId);
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
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return  _unitOfWork.NotificationRepository.GetNotificationsByUserId(user.UserId);
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
    public async Task MarkNotificationAsRead(string email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            // List<UserNotificationMapping>? notifications = _notificationRepository.GetUserNotificationMapping(user.UserId);
            List<UserNotificationMapping>? notifications = await _unitOfWork.UserNotificationMappingRepository.FindAllAsync(unm => unm.UserId == user.UserId && unm.IsRead == false);
            if (notifications != null && notifications.Count > 0)
            {
                foreach (UserNotificationMapping notification in notifications)
                {
                    notification.IsRead = true;
                    notification.EditedAt = DateTime.Now;
                }
                // _notificationRepository.UpdateNotificationRange(notifications);
                await _unitOfWork.UserNotificationMappingRepository.UpdateRangeAsync(notifications);
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// razorpay payment gateway helper service
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="objRes"></param>
    /// <returns></returns>
    public async Task<PaymentViewModel> CreatePayment(int UserId,ObjectSessionViewModel objRes)
    {
        try
        {
            // User? user = _userRepository.GetUserById(UserId);
            User? user = await _unitOfWork.UserRepository.GetByIdAsync(UserId);

            Profile? profile = user!=null ? await _unitOfWork.ProfileRepository.GetByIdAsync(user.ProfileId) : null;

            if (user == null)
            {
                throw new Exception("User not found");
            }

            

            // generating transaction id
            string transactionId = Guid.NewGuid().ToString();

            string razorpaySecret = _configuration["Razorpay:Secret"] ?? "";
            string razorpayKey = _configuration["Razorpay:Key"] ?? "";

            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(razorpayKey, razorpaySecret);
            
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", (int)((objRes.totalPrice > 0 ? objRes.totalPrice : 0) * 100)); // Amount in paise
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 0 for authorization only, 1 for automatic capture
            
            Razorpay.Api.Order orderResponse = client.Order.Create(options);
            string orderId2 = orderResponse["id"].ToString();

            PaymentViewModel payment = new PaymentViewModel()
            {
                orderId = orderResponse.Attributes["id"],
                UserId = user.UserId,
                Amount = ((objRes.totalPrice > 0 ? objRes.totalPrice : 0) * 100), // Amount in paise
                TotalQuantity = objRes.totalQuantity,
                TotalDiscount = objRes.totalDiscount,
                RazorpayKey = _configuration["Razorpay:Key"] ?? "",
                Currency = _configuration["Razorpay:Currency"] ?? "INR",
                Name = profile?.FirstName + " " + profile?.LastName,
                Email = user.Email,
                PhoneNumber = profile?.PhoneNumber,
                Address = profile?.Address,
                Description = "Order Payment for " + (objRes.orders?.Count > 0 ? objRes.orders.Count + " items" : "1 item")
            };

            return payment;

        }
        catch{
            return new PaymentViewModel()
            {
                orderId = null,
                UserId = 0,
                Amount = 0,
                TotalQuantity = 0,
                TotalDiscount = 0,
                RazorpayKey = string.Empty,
                Currency = "INR",
                Name = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty,
                Address = string.Empty,
                Description = "An error occurred while fetching payment details."

            };
        }
    }
}
