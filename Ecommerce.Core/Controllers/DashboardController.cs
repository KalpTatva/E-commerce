using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Ecommerce.Core.Hub;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Core.Controllers;

public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IHubContext<NotificationHub> _notificationHub;
    public DashboardController( 
        IUserService userService, 
        IOrderService orderService, 
        IProductService productService,
        IHubContext<NotificationHub> notificationHub
        
    )
    {
        _userService = userService;
        _orderService = orderService;
        _productService = productService;
        _notificationHub = notificationHub;
    }

    /// <summary>
    /// Index method for all type of users (not logged in, seller, buyer)
    /// </summary>
    /// <returns>View with base view model</returns>
    public IActionResult Index()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);

        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }

    /// <summary>
    /// Index method for user dashboard
    /// </summary>
    /// <returns>View with base view model</returns>
    public IActionResult UserDashboard()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }
   
    /// <summary>
    /// Method to edit user profile
    /// </summary>
    /// <returns>View with user details</returns>   
    [Authorize]
    public IActionResult EditProfile()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        EditRegisteredUserViewModel? model = _userService.GetUserDetailsByEmail(email ?? ""); 
        if(model!=null)
        {
            model.BaseEmail = email;
            model.BaseRole = role;
            model.BaseUserName = name;
        }   
        return View(model);
    }

    /// <summary>
    /// Method to edit user profile details
    /// </summary>
    /// <param name="model">Model containing user details</param>
    [Authorize]
    public async Task<IActionResult> EditUser(EditRegisteredUserViewModel model)
    {
        try
        {
            ResponsesViewModel responses = await _userService.EditUserDetails(model);
            if(responses.IsSuccess)
            {
                return Json(new {success= true,message=responses.Message});
            }
            return Json(new {success= false,message=responses.Message});

        }
        catch(Exception e)
        {
            return Json(new{success = false, message=e.Message});
        }
    }


    /// <summary>
    /// Method to view user's favourite products
    /// </summary>
    /// <returns>View with base view model</returns>
    [Authorize(Roles ="Buyer")]
    public IActionResult MyOrders()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);

    
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }

    /// <summary>
    /// Method to get user's order history
    /// </summary>
    /// <returns>Partial view with user's order history</returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);

        List<MyOrderViewModel>? model = await _orderService.GetMyOrderHistoryByEmail(email ?? ""); 
        OrderAtMyOrderViewModel result = new ();
        result.BaseEmail = email;
        result.BaseRole = role;
        result.BaseUserName = name;
        if(model!=null)
        {
            result.myOrderViewModels = model;
        }
        return PartialView("_MyOrdersPartial", result);
    }

    /// <summary>
    /// Method to view seller's order list
    /// </summary>
    [Authorize(Roles = "Seller")]
    public IActionResult SellerOrderList()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        BaseViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }

    /// <summary>
    /// Method to get seller's orders
    /// </summary>
    [Authorize(Roles = "Seller")]
    [HttpGet]
    public async Task<IActionResult> GetSellerOrders(
        int pageNumber = 1, 
        int pageSize = 5
    )
    {   
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);

        List<SellerOrderViewModel>? model = await _orderService.GetSellerOrders(email ?? "", pageNumber, pageSize);
        int TotalOrders = _orderService.GetSellersOrderTotalCount(email ?? "");
        SellerOrderListViewModel sellerOrderListViewModel = new SellerOrderListViewModel
        {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name,
            TotalCount = TotalOrders,
            SellerOrders = model ?? new List<SellerOrderViewModel>()
        };
        return PartialView("_SellerOrderList", sellerOrderListViewModel);   
    }


    /// <summary>
    /// Method to add an offer for a product
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "Seller")]
    public IActionResult AddOffer()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);
    
        OfferViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }


    /// <summary>
    /// Method to add an offer for a product
    /// </summary>
    /// <param name="model"></param>
    /// <returns>View</returns>
    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> AddOffer(OfferViewModel model)
    {
        try
        {
            if(ModelState.IsValid)
            {
                ResponsesViewModel responses = await _orderService.AddOffer(model);
                if(responses.IsSuccess)
                {
                    TempData["SuccessMessage"] = responses.Message;
                    string? email = BaseValues.GetEmail(HttpContext);
                    string? role = BaseValues.GetRole(HttpContext);
                    string? name = BaseValues.GetUserName(HttpContext);

                    OfferViewModel baseViewModel = new () {
                        BaseEmail = email,
                        BaseRole = role,
                        BaseUserName = name
                    };   

                    // call hub for notification add
                    await _notificationHub.Clients.All.SendAsync("ReceiveNotification", $"New offer added by {email} for product {model.ProductId}.");

                    return RedirectToAction("EditProfile", "Dashboard");
                }
                TempData["ErrorMessage"] = responses.Message;
            }
            else
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = string.Join(", ", errorMessages);
            
                string? email = BaseValues.GetEmail(HttpContext);
                string? role = BaseValues.GetRole(HttpContext);
                string? name = BaseValues.GetUserName(HttpContext);

                OfferViewModel baseViewModel = new () {
                    BaseEmail = email,
                    BaseRole = role,
                    BaseUserName = name
                }; 
                return View(baseViewModel);
            }
            return View(model);
        }
        catch (Exception e)
        {   
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);

            OfferViewModel baseViewModel = new () {
                BaseEmail = email,
                BaseRole = role,
                BaseUserName = name
            }; 
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }

    /// <summary>
    /// Method to get products list for offer
    /// </summary>
    /// <returns>Json</returns>
    [HttpGet]
    public IActionResult GetProducts()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);    
                
            List<ProductNameViewModel> products = _productService.GetProductsForOffer(email ?? "");
            if(products == null || products.Count == 0)
            {
                return Json(new {success = false, message = "No products found for offer."});
            }
            return Json(new {success = true, products = products});
        }
        catch(Exception e)
        {
            return Json(new {success = false, message = e.Message});
        }
    }


    [HttpGet]
    public IActionResult GetNotificationCount()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            int count = _orderService.GetNotificationCount(email ?? "");
            return Json(new { success = true, count = count });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }


    [HttpGet]
    public IActionResult GetNotifications()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            List<Notification>? notifications = _orderService.GetNotificationsByEmail(email ?? "");
            if (notifications == null || notifications.Count == 0)
            {
                return Json(new { success = false, message = "No notifications found." });
            }
            return Json(new { success = true, notifications = notifications });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            await _orderService.MarkNotificationAsRead( email ?? "");
            return Json(new { success = true, message = "Notification marked as read." });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }


    /// <summary>
    /// Method to display the contact us page
    /// </summary>
    /// <returns></returns>
    public IActionResult ContactUs()
    {
        string? email = BaseValues.GetEmail(HttpContext);
        string? role = BaseValues.GetRole(HttpContext);
        string? name = BaseValues.GetUserName(HttpContext);

    
        ContactUsViewModel baseViewModel = new () {
            BaseEmail = email,
            BaseRole = role,
            BaseUserName = name
        };        
        return View(baseViewModel);
    }

    /// <summary>
    /// Method to handle contact us form submission
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> ContactUs(ContactUsViewModel model)
    {
        try
        {
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);

            if (ModelState.IsValid)
            {
                
                ResponsesViewModel responses = await _userService.AddContactMessage(model);

                if (!responses.IsSuccess)
                {
                    model.BaseEmail = email;
                    model.BaseRole = role;
                    model.BaseUserName = name;
                    TempData["ErrorMessage"] = responses.Message;
                    return View(model);
                }

                // Notify all connected clients about the new contact message
                await _notificationHub.Clients.All.SendAsync("ReceiveNotification", $"New contact message from {model.BaseEmail}: {model.Message}");

                TempData["SuccessMessage"] = "Your message has been sent successfully.";
                
                ContactUsViewModel baseViewModel = new () {
                    BaseEmail = email,
                    BaseRole = role,
                    BaseUserName = name
                };
               
                return RedirectToAction("EditProfile", "Dashboard");
            }
            model.BaseEmail = email;
            model.BaseRole = role;
            model.BaseUserName = name;
            TempData["ErrorMessage"] = "Invalid input. Please check your details.";
            return View(model);
        }
        catch(Exception e)
        {   
            string? email = BaseValues.GetEmail(HttpContext);
            string? role = BaseValues.GetRole(HttpContext);
            string? name = BaseValues.GetUserName(HttpContext);
            model.BaseEmail = email;
            model.BaseRole = role;
            model.BaseUserName = name;
            TempData["ErrorMessage"] = e.Message;
            return View(model);
        }
    }

}
