using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sentry.Watchers.Api
{
    public class ApiWatcherCheckResult : WatcherCheckResult
    {
        public Uri Uri { get; }
        public HttpRequest Request { get; }
        public HttpRequestHeaders RequestHeaders { get; }
        public HttpResponseMessage Response { get; }

        protected ApiWatcherCheckResult(IWatcher watcher, bool isValid, string description, 
            Uri uri, HttpRequest request, HttpRequestHeaders requestHeaders, HttpResponseMessage response) 
            : base(watcher, isValid, description)
        {
            Uri = uri;
            Request = request;
            RequestHeaders = requestHeaders;
            Response = response;
        }

        public static ApiWatcherCheckResult Create(IWatcher watcher, bool isValid, Uri uri,
            HttpRequest request, HttpRequestHeaders requestHeaders, HttpResponseMessage response, string description = "")
            => new ApiWatcherCheckResult(watcher, isValid, description, uri, request, requestHeaders, response);
    }
}