using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace Warden.Web.Framework.Filters
{
    public class ExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext context)
        {
            Logger.Error(context.Exception);
        }
    }
}