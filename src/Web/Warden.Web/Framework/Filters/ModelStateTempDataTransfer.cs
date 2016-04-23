using Microsoft.AspNet.Mvc.Filters;

namespace Warden.Web.Framework.Filters
{
    public abstract class ModelStateTempDataTransfer : ActionFilterAttribute
    {
        protected static readonly string ErrorsKey = typeof(ModelStateTempDataTransfer).FullName;
    }
}