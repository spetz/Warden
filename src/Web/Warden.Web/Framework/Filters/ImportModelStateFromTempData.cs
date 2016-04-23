using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Framework.Filters
{
    public class ImportModelStateFromTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var tempData = filterContext.HttpContext.RequestServices.GetService<ITempDataDictionary>();
            if(!tempData.ContainsKey(ErrorsKey))
            {
                base.OnActionExecuted(filterContext);

                return;
            }

            var errors = tempData[ErrorsKey].ToString().FromJson<List<KeyValuePair<string, string>>>();
            if (errors != null)
            {
                if (filterContext.Result is ViewResult)
                {
                    foreach (var error in errors)
                    {
                        filterContext.ModelState.AddModelError(error.Key, error.Value);
                    }
                }
                else
                {
                    tempData.Remove(ErrorsKey);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}