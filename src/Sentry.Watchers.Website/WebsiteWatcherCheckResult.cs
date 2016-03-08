using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sentry.Watchers.Website
{
    public class WebsiteWatcherCheckResult : WatcherCheckResult
    {
        public Uri Uri { get; }
        public HttpRequestHeaders RequestHeaders { get; }
        public HttpResponseMessage Response { get; }

        protected WebsiteWatcherCheckResult(IWatcher watcher, bool isValid, string description, Uri uri,
            HttpRequestHeaders requestHeaders, HttpResponseMessage response) : base(watcher, isValid, description)
        {
            Uri = uri;
            RequestHeaders = requestHeaders;
            Response = response;
        }

        public static WebsiteWatcherCheckResult Create(IWatcher watcher, bool isValid, Uri uri,
            HttpRequestHeaders requestHeaders, HttpResponseMessage response, string description = "")
            => new WebsiteWatcherCheckResult(watcher, isValid, description, uri, requestHeaders, response);
    }
}