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
    private readonly IOrderRepository _orderRepository;

    public OrderService(
    IProductRepository productRepository, 
    IWebHostEnvironment webHostEnvironment,
    IUserRepository userRepository,
    IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _webHostEnvironment = webHostEnvironment;
        _userRepository = userRepository; 
        _orderRepository = orderRepository;
    }


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

    public async Task<ResponsesViewModel?> PlaceOrder(
    ObjectSessionViewModel objSession, 
    int UserId, 
    bool isByProductId = false)
    {
        try{
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
                    if (model.DiscountType == (int)DiscountEnum.FixedAmount)
                    {
                        price = model.Price - (model.Discount ?? 0);
                        discount = model.Discount ?? 0;
                    }
                    else if (model.DiscountType == (int)DiscountEnum.Percentage)
                    {
                        price = model.Price - ((model.Price * (model.Discount ?? 0)) / 100);
                        discount = (model.Price * (model.Discount ?? 0)) / 100;
                    }

                    orderProducts.Add(new OrderProduct()
                    {
                        OrderId = order.OrderId,
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        PriceWithDiscount = price
                    });
                }
                _orderRepository.AddOrderProductRange(orderProducts);

                // Mark cart items as deleted
                _productRepository.DeleteCartByIdsRange(objSession.orders ?? new List<int>());

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



}
