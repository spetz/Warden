using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    public abstract class ApiController : Controller
    {
        private readonly IApiKeyService _apiKeyService;
        protected Guid OrganizationId { get; private set;  }

        protected ApiController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        //Improve performance e.g. by storing API keys in cache
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var apiKeyHeader = context.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "X-Api-Key");
            if (apiKeyHeader.Key.Empty())
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
            var apiKey = await _apiKeyService.GetAsync(apiKeyHeader.Value);
            if (apiKey == null)
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
            OrganizationId = apiKey.UserId;
            await next();
        }
    }
}