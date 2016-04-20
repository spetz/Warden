using System;
using Microsoft.AspNet.Mvc;

namespace Warden.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected Guid UserId
            => HttpContext.User.Identity.IsAuthenticated ? Guid.Parse(HttpContext.User.Identity.Name) : Guid.Empty;
    }
}