using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Warden.Web.Framework.Filters
{
    public abstract class ModelStateTempDataTransfer : ActionFilterAttribute
    {
        protected static readonly string ModelEntriesKey = typeof(ModelStateTempDataTransfer).FullName;

        protected internal class Entry
        {
            public object RawValue { get; set; }
            public string AttemptedValue { get; set; }
            public ModelValidationState State { get; set; }
            public IEnumerable<string> Errors { get; set; }

            public Entry()
            {
            }

            public Entry(ModelStateEntry entry)
            {
                RawValue = entry.RawValue;
                AttemptedValue = entry.AttemptedValue;
                State = entry.ValidationState;
                Errors = entry.Errors?.Select(x => x.ErrorMessage) ?? Enumerable.Empty<string>();
            }
        }
    }
}