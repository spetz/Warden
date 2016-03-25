using System;

namespace Sentry.Watchers.Web
{
    public class WebWatcherCheckResult : WatcherCheckResult
    {
        public Uri Uri { get; }
        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }

        protected WebWatcherCheckResult(IWatcher watcher, bool isValid, string description,
             Uri uri, IHttpRequest request, IHttpResponse response) 
            : base(watcher, isValid, description)
        {
            Uri = uri;
            Request = request;
            Response = response;
            Response = response;
        }

        public static WebWatcherCheckResult Create(IWatcher watcher, bool isValid, Uri uri,
            IHttpRequest request, IHttpResponse response, string description = "")
            => new WebWatcherCheckResult(watcher, isValid, description, uri, request, response);
    }
}