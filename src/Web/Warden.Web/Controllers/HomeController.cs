using Microsoft.AspNet.Mvc;

namespace Warden.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}