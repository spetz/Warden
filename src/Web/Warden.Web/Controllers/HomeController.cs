using Microsoft.AspNet.Mvc;

namespace Warden.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpGet]
        [Route("about")]
        public IActionResult About()
        {
            return View();
        }
    }
}