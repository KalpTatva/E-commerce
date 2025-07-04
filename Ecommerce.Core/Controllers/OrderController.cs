using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Core.Hub;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Razorpay.Api;

namespace Ecommerce.Core.Controllers;

public class OrderController : Controller
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IHubContext<NotificationHub> _notificationHub;
    public OrderController(
        IProductService productService, 
        IOrderService orderService,
        IHubContext<NotificationHub> notificationHub)
    {
        _productService = productService;
        _notificationHub = notificationHub;
        _orderService = orderService;
    }

    /// <summary>
    /// method for set cookie for order
    /// </summary>
    /// <param name="objectCart"></param>
    /// <returns>Json with success or error message</returns>
    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> SetSessionForOrder(string objectCart)
    {
        try
        {   
            string? email = BaseValues.GetEmail(HttpContext);
    
            ObjectSessionViewModel? res = string.IsNullOrEmpty(objectCart) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(objectCart);
            if (res == null)
            {
                return Json(new {
                    success = false,
                    message = "error while setting up cookie while creating order"
                }); 
            }

            ResponsesViewModel response = await _productService.CheckProductStockByCartId(email ?? "");

            if (!response.IsSuccess)
            {
                return Json(new {
                    success = false,
                    message = response.Message
                });
            }

            string sessionId = Guid.NewGuid().ToString();
            CookieUtils.SetCookie(HttpContext, sessionId, objectCart);
            return Json(new {
                success = true,
                message = sessionId
            });
        }
        catch
        {
            return Json(new {
                success = false,
                message = "error while setting up cookie while creating order"
            });
        }
    }

    
    /// <summary>
    /// method for make changes on cookie
    /// </summary>
    /// <param name="SessionId"></param>
    /// <returns></returns>
    [Authorize(Roles = "Buyer")]
    [HttpDelete]
    public IActionResult CancelOrderBefore(string SessionId)
    {
        try
        {
            CookieUtils.RemoveCookie(HttpContext, SessionId);
            return Json(new { success = true, message = "order canceled" });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }   


    /// <summary>
    /// Method to view order details
    /// </summary>
    /// <returns>View with order details</returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> Index(string sessionId)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);

            string? res = CookieUtils.GetCookie(HttpContext, sessionId);
            

            if (res == null)
            {
                TempData["ErrorMessage"] = "your order's cookie is expired! please reset your order";
                return RedirectToAction("Index", "BuyerDashboard");
            } 

            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            if (objRes == null)
            {
                TempData["ErrorMessage"] = "your order's cookie is expired! please reset your order";
                return RedirectToAction("Index", "BuyerDashboard");
            }

            OrderViewModel result = await _orderService.GetDetailsForOrder(objRes, email ?? "");
            result.BaseEmail = email;
            result.BaseRole = role;
            result.SessionId = sessionId;
            result.BaseUserName = name;
            return View(result);

        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return RedirectToAction("Index", "BuyerDashboard");
        }
    }



    /// <summary>
    /// Method to place an order
    /// </summary>
    /// <param name="UserId"></param>
    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        string rzp_paymentid,
        string rzp_orderid,
        int UserId, 
        string SessionId
    )
    {
        try{

            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);
            
            string? res = CookieUtils.GetCookie(HttpContext, SessionId);
            
            if (res == null)
            {
                return Json(new{success=false,message="your order's cookie is expired! please reset your order"});
            } 


            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            if (objRes == null)
            {
                return Json(new{success=false,message="your order's cookie is expired! please reset your order"});
            }

            ResponsesViewModel? response = await _orderService.PlaceOrder(objRes, UserId, rzp_paymentid, rzp_orderid, objRes.isByProductId);
            if(response!=null && response.IsSuccess)
            {
                // clearing cookie 
                CookieUtils.RemoveCookie(HttpContext, SessionId);
                // calling signalR to update order status
                await _notificationHub.Clients.All.SendAsync("ReceiveNotification", "Order placed successfully");
                return Json(new {success=true,message=response.Message});
            }else{
                return Json(new {success=false,message=response?.Message}); 
            }

        }
        catch(Exception e){
           return Json(new {success=false,message=e.Message}); 
        }
    }


    /// <summary>
    /// Method to view order details for a single product
    /// </summary>
    /// <returns>View with order details</returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> BuyProduct(string sessionId)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);

            string? res = CookieUtils.GetCookie(HttpContext, sessionId);
            

            if (res == null)
            {
                TempData["ErrorMessage"] = "your order's cookie is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            } 

            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            if (objRes == null)
            {
                TempData["ErrorMessage"] = "your order's cookie is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            }

            OrderViewModel result = await _orderService.GetDetailsForSingleOrder(objRes, email ?? "");
            result.BaseEmail = email;
            result.BaseRole = role;
            result.SessionId = sessionId;
            result.BaseUserName = name;
            return View(result);
        }
        catch(Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return RedirectToAction("Index","BuyerDashboard");
        }
    }


    /// <summary>
    /// Method to get order history for a buyer
    /// </summary>
    /// <returns>Partial view with order history</returns>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
    {
        try
        {
            if (string.IsNullOrEmpty(status) || orderId <= 0)
            {
                return Json(new { success = false, message = "Invalid order ID or status." });
            }

            ResponsesViewModel? isUpdated = await _orderService.UpdateOrderStatus(orderId, status);
            if (isUpdated.IsSuccess)
            {
                return Json(new { success = true, message = isUpdated.Message });
            }
            else
            {
                return Json(new { success = false, message = isUpdated.Message });
            }
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }


    /// <summary>
    /// Method to add a review for an order product
    /// </summary>
    /// <param name="orderProductId"></param>
    /// <param name="rating"></param>
    /// <param name="reviewText"></param>
    [HttpPost]
    public async Task<IActionResult> AddReview(int orderProductId,decimal rating,int productId, string reviewText)
    {
        if(orderProductId > 0)
        {
            try
            {
                string? email = BaseValues.GetEmail(HttpContext);
    
                
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "User email not found." });
                }

                ResponsesViewModel response = await _productService.AddReview(orderProductId, rating, productId, reviewText, email);
                if (response.IsSuccess)
                {
                    return Json(new { success = true, message = response.Message });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
        else
        {
            return Json(new { success = false, message = "Invalid order product ID." });
        }
    }





    public async Task<IActionResult> CreatePayment(int UserId, string SessionId)
    {
        
        // get details from cookie 
        string? res = CookieUtils.GetCookie(HttpContext, SessionId);
        if (res == null)
        {
            return Json(new{success=false,message="your order's cookie is expired! please reset your order"});
        } 

        ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
            ? new ObjectSessionViewModel() 
            : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

        if (objRes == null)
        {
            return Json(new{success=false,message="your order's cookie is expired! please reset your order"});
        }
        
        // create payment
        PaymentViewModel paymentViewModel = await _orderService.CreatePayment(UserId, objRes);
        paymentViewModel.sessionId = SessionId;
        return View(paymentViewModel);
    
    }


}