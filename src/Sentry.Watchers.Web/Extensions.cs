namespace Sentry.Watchers.Web
{
    /// <summary>
    /// Custom extension methods for the Web watcher.
    /// </summary>
    public static class Extensions
    {
        public static string GetFullUrl(this IHttpRequest request, string baseUrl)
        {
            var endpoint = request.Endpoint;
            if (string.IsNullOrWhiteSpace(endpoint))
                return baseUrl;

            if(baseUrl.EndsWith("/"))
                return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint.Substring(1) : $"{endpoint}")}";

            return $"{baseUrl}{(endpoint.StartsWith("/") ? endpoint : $"/{endpoint}")}";
        }
    }
}