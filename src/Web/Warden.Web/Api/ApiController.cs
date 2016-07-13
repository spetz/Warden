using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Services;

namespace Warden.Web.Api
{
    public abstract class ApiController : Controller
    {
        protected readonly IApiKeyService ApiKeyService;
        protected readonly IUserService UserService;
        protected Guid UserId { get; private set;  }

        protected ApiController(IApiKeyService apiKeyService, IUserService userService)
        {
            ApiKeyService = apiKeyService;
            UserService = userService;
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
            var apiKey = await ApiKeyService.GetAsync(apiKeyHeader.Value);
            if (apiKey == null)
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
            var isActive = await UserService.IsActiveAsync(apiKey.UserId);
            if (!isActive)
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }

            UserId = apiKey.UserId;
            await next();
        }
    }
}