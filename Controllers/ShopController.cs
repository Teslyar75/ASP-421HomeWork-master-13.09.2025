using Microsoft.AspNetCore.Mvc;

namespace ASP_421.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Admin()
        {
            return View();
        }
    }
}
