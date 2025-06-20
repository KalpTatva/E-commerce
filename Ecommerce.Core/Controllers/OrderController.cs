using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Ecommerce.Core.Controllers;

public class OrderController : Controller
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    public OrderController(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
    }


    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public IActionResult SetSessionForOrder(string objectCart)
    {
        try
        {   
            // check if objectCart get dserialized or not
            ObjectSessionViewModel? res = string.IsNullOrEmpty(objectCart) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(objectCart);
            // if not then show error
            if(res==null)
            {
                return Json(new {
                    success = false,
                    message = "error while setting up session while creating order"
                }); 
            }
            // if json data is deserialized then add that data to session
            // here deserialized only for checking perpose
            string sessionId = Guid.NewGuid().ToString();
            // set it into session for 30 minutes
            SessionUtils.SetSession(HttpContext,sessionId, objectCart);
            // return session id which will redirect to the page
            return Json( new {
                success=true,
                message=sessionId
            });
        }
        catch{
            return Json(new {
                success = false,
                message = "error while setting up session while creating order"
            });
        }
    }

    
    /// <summary>
    /// method for make changes on session
    /// </summary>
    /// <param name="SessionId"></param>
    /// <returns></returns>
    [Authorize(Roles = "Buyer")]
    [HttpDelete]
    public IActionResult CancelOrderBefore(string SessionId)
    {
        try
        {
            // util for delete session data 
            SessionUtils.RemoveSessionById(HttpContext, SessionId);
            return Json(new{success=true,message="order canceled"});
        }
        catch(Exception e)
        {
            return Json(new {success=false,message=e.Message});
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> Index(string sessionId)
    {
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // check if session is persist or not
            string? res = SessionUtils.GetSession(HttpContext, sessionId);
            
            // if not then redirect to the index of dashboard
            if(res == null)
            {
                TempData["ErrorMessage"] = "your order's session is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            } 

            // check if objectCart get dserialized or not
            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            // if not then show error and redirect to index of dashboard
            if(objRes==null)
            {
                TempData["ErrorMessage"] = "your order's session is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            }

            OrderViewModel result = await _orderService.GetDetailsForOrder(objRes, email ?? "");
            result.BaseEmail = email;
            result.BaseRole = role;
            result.SessionId = sessionId;
            return View(result);

        }
        catch(Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return RedirectToAction("Index","BuyerDashboard");
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(int UserId, string SessionId)
    {
        try{

            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                    ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            
            // check if session is persist or not
            string? res = SessionUtils.GetSession(HttpContext, SessionId);
            
            // if not then redirect to the index of dashboard
            if(res == null)
            {
                return Json(new{success=false,message="your order's session is expired! please reset your order"});
            } 

            // check if objectCart get dserialized or not
            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            // if not then show error and redirect to index of dashboard
            if(objRes==null)
            {
                return Json(new{success=false,message="your order's session is expired! please reset your order"});
            }

            ResponsesViewModel? response = await _orderService.PlaceOrder(objRes, UserId);
            if(response!=null && response.IsSuccess)
            {
                // util for delete session data 
                SessionUtils.RemoveSessionById(HttpContext, SessionId);
                return Json(new {success=true,message=response.Message});
            }else{
                return Json(new {success=false,message=response?.Message}); 
            }

        }
        catch(Exception e){
           return Json(new {success=false,message=e.Message}); 
        }
    }

    
    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrderFoSingleProduct(int UserId, string SessionId)
    {
        try{
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                    ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            
            // check if session is persist or not
            string? res = SessionUtils.GetSession(HttpContext, SessionId);
            
            // if not then redirect to the index of dashboard
            if(res == null)
            {
                return Json(new{success=false,message="your order's session is expired! please reset your order"});
            } 

            // check if objectCart get dserialized or not
            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            // if not then show error and redirect to index of dashboard
            if(objRes==null)
            {
                return Json(new{success=false,message="your order's session is expired! please reset your order"});
            }

            ResponsesViewModel? response = await _orderService.PlaceOrder(objRes, UserId,true);
            if(response!=null && response.IsSuccess)
            {
                // util for delete session data 
                SessionUtils.RemoveSessionById(HttpContext, SessionId);
                return Json(new {success=true,message=response.Message});
            }else{
                return Json(new {success=false,message=response?.Message}); 
            }

        }
        catch(Exception e){
           return Json(new {success=false,message=e.Message}); 
        }
    }


    [Authorize(Roles = "Buyer")]
    [HttpGet]
    public async Task<IActionResult> BuyProduct(string sessionId)
    {
        try
        {
            string? email = HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value 
                ?? HttpContext.User.FindFirst(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            string? role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // check if session is persist or not
            string? res = SessionUtils.GetSession(HttpContext, sessionId);
            
            // if not then redirect to the index of dashboard
            if(res == null)
            {
                TempData["ErrorMessage"] = "your order's session is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            } 

            // check if objectCart get dserialized or not
            ObjectSessionViewModel? objRes = string.IsNullOrEmpty(res) 
                ? new ObjectSessionViewModel() 
                : JsonConvert.DeserializeObject<ObjectSessionViewModel>(res);

            // if not then show error and redirect to index of dashboard
            if(objRes==null)
            {
                TempData["ErrorMessage"] = "your order's session is expired! please reset your order";
                return RedirectToAction("Index","BuyerDashboard");
            }

            OrderViewModel result = await _orderService.GetDetailsForSingleOrder(objRes, email ?? "");
            result.BaseEmail = email;
            result.BaseRole = role;
            result.SessionId = sessionId;
            return View(result);

        }
        catch(Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            return RedirectToAction("Index","BuyerDashboard");
        }
    }


}