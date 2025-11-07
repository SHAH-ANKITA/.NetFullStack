using Microsoft.AspNetCore.Mvc;

namespace GenerationPlanMLM.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

