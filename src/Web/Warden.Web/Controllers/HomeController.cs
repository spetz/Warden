using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Services;

namespace Warden.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IApiKeyService _apiKeyService;

        public HomeController(IUserService userService, IOrganizationService organizationService, 
            IApiKeyService apiKeyService)
        {
            _userService = userService;
            _organizationService = organizationService;
            _apiKeyService = apiKeyService;
        }

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

        //TODO: remove temp method
        [HttpGet]
        [Route("seed-data")]
        public async Task RunSeedData()
        {
            var email = "my@email.com";
            var password = "test";
            var organizationName = "My organization";
            await _userService.RegisterAsync(email, password);
            var user = await _userService.GetAsync(email);
            await _organizationService.CreateAsync(organizationName, user.Id);
            var organization = await _organizationService.GetAsync(organizationName);
            await _apiKeyService.CreateAsync(organization.Id);
            var apiKeys = await _apiKeyService.GetAllForOrganizationAsync(organization.Id);
            await Response.WriteAsync($"Organization id: {organization.Id}\n" +
                                      $"API key: {apiKeys.First()}");
        }
    }
}