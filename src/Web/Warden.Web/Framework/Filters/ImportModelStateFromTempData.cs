using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
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
            if (!tempData.ContainsKey(ModelEntriesKey))
                return;

            var modelEntries = tempData[ModelEntriesKey].ToString().FromJson<Dictionary<string, Entry>>();
            if (modelEntries == null)
                return;

            foreach (var modelEntry in modelEntries)
            {
                filterContext.ModelState.Add(modelEntry.Key, new ModelStateEntry
                {
                    AttemptedValue = modelEntry.Value.AttemptedValue,
                    RawValue = modelEntry.Value.RawValue,
                    ValidationState = modelEntry.Value.State
                });

                foreach (var error in modelEntry.Value.Errors)
                {
                    filterContext.ModelState.AddModelError(modelEntry.Key, error);
                }
            }

            tempData.Remove(ModelEntriesKey);
            base.OnActionExecuted(filterContext);
        }
    }
}