using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Core.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
