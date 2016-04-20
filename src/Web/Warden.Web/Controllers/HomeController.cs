using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using MongoDB.Driver;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Services;

namespace Warden.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoDatabase _database;
        private readonly IEncrypter _encrypter;
        private readonly IApiKeyService _apiKeyService;

        public HomeController(IMongoDatabase database, IEncrypter encrypter, IApiKeyService apiKeyService)
        {
            _database = database;
            _encrypter = encrypter;
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
            var user = new User("test@email.com", "test", _encrypter);
            var organization = new Organization("Test", user);
            organization.AddWarden("Test Warden");
            await _database.Users().InsertOneAsync(user);
            await _database.Organizations().InsertOneAsync(organization);
            await _apiKeyService.CreateAsync(organization.Id);
            var apiKeys = await _apiKeyService.GetAllForOrganizationAsync(organization.Id);
            await Response.WriteAsync($"Organization id: {organization.Id}\n" +
                                      $"API key: {apiKeys.First()}");
        }
    }
}