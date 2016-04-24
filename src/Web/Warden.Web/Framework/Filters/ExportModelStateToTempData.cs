using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Framework.Filters
{
    public class ExportModelStateToTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var tempData = filterContext.HttpContext.RequestServices.GetService<ITempDataDictionary>();
            if (filterContext.ModelState.IsValid)
                return;
            if (IsInvalidResult(filterContext))
                return;

            var values = filterContext.ModelState.ToList()
                .Select(item => new KeyValuePair<string, Entry>(item.Key, new Entry(item.Value)))
                .ToDictionary(x => x.Key, x => x.Value);
            tempData[ModelEntriesKey] = values.ToJson();

            base.OnActionExecuted(filterContext);
        }

        private static bool IsInvalidResult(ActionExecutedContext context)
            => !(context.Result is RedirectToActionResult) &&
               !(context.Result is RedirectResult) &&
               !(context.Result is RedirectToRouteResult);
    }
}