using Microsoft.AspNetCore.Mvc;

namespace ScramblerWeb.Server.Controllers
{
    [ApiController]
    public class HomeController : Controller
    {
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
