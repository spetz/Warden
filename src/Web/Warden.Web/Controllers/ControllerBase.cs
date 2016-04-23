using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Warden.Web.Core.Extensions;
using Warden.Web.Framework;

namespace Warden.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        private const string NotificationKey = "Notifications";

        protected Guid UserId
            => HttpContext.User.Identity.IsAuthenticated ? Guid.Parse(HttpContext.User.Identity.Name) : Guid.Empty;

        protected void Notify(FlashNotificationType type, string message, params object[] args)
        {
            var notifications = FetchTempData<List<KeyValuePair<FlashNotificationType, string>>>(NotificationKey) ??
                                new List<KeyValuePair<FlashNotificationType, string>>();
            notifications.Add(new KeyValuePair<FlashNotificationType, string>(type, string.Format(message, args)));
            TempData[NotificationKey] = notifications.ToJson();
        }

        protected T FetchTempData<T>(string key)
        {
            var result = default(T);
            var data = TempData[key];
            if (data == null)
                return result;

            try
            {
                result = (T)Convert.ChangeType(data, typeof(T));
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }
    }
}