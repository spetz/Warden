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
            if (!filterContext.ModelState.IsValid)
            {
                if (filterContext.Result is RedirectToActionResult ||
                    filterContext.Result is RedirectResult ||
                    filterContext.Result is RedirectToRouteResult)
                {
                    var errors = filterContext.ModelState.ToList()
                        .Where(item => item.Value.Errors.Any())
                        .Select(item => new KeyValuePair<string, string>(item.Key, item.Value.Errors[0].ErrorMessage))
                        .ToList();

                    tempData[ErrorsKey] = errors.ToJson();
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}