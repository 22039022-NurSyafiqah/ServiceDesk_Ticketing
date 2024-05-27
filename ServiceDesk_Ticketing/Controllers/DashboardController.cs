using Microsoft.AspNetCore.Mvc;

namespace ServiceDesk_Ticketing.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}