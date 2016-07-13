using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Warden.Web.Core.Domain;

namespace Warden.Web.Extensions
{
    public static class TaskExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Execute(this Task task, 
            Action onSuccess, 
            Action<Exception> onFailure = null)
        {
            try
            {
                await task;
                onSuccess();
            }
            catch (DomainException exception)
            {
                Logger.Error(exception);
                onFailure?.Invoke(exception);
            }
            catch (ServiceException exception)
            {
                Logger.Error(exception);
                onFailure?.Invoke(exception);
            }
        }

        public static async Task<IActionResult> Execute(this Task task, 
            Func<IActionResult> onSuccess, 
            Func<Exception, IActionResult> onFailure = null)
        {
            try
            {
                await task;
                return onSuccess();
            }
            catch (DomainException exception)
            {
                Logger.Error(exception);
                return onFailure?.Invoke(exception);
            }
            catch (ServiceException exception)
            {
                Logger.Error(exception);
                return onFailure?.Invoke(exception);
            }
        }

        public static async Task<IActionResult> ExecuteAsync(this Task task,
            Func<Task<IActionResult>> onSuccess,
            Func<Exception, IActionResult> onFailure = null)
        {
            try
            {
                await task;
                return await onSuccess();
            }
            catch (DomainException exception)
            {
                Logger.Error(exception);
                return onFailure?.Invoke(exception);
            }
            catch (ServiceException exception)
            {
                Logger.Error(exception);
                return onFailure?.Invoke(exception);
            }
        }
    }
}