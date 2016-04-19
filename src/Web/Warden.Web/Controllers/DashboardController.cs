using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Warden.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Index()
        {
            return View();
        }
    }
}