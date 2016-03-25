using System.Collections.Generic;

namespace Sentry.Watchers.Web
{
    public enum HttpMethod
    {
        Get = 1,
        Put = 2,
        Post = 3,
        Delete = 4
    }

    public interface IHttpRequest
    {
        HttpMethod Method { get; }
        string Endpoint { get; }
        object Data { get; }
        IDictionary<string, string> Headers { get; }
    }

    public class HttpRequest : IHttpRequest
    {
        public HttpMethod Method { get; }
        public string Endpoint { get; }
        public object Data { get; }
        public IDictionary<string, string> Headers { get; }

        protected HttpRequest(HttpMethod method, string endpoint,
            IDictionary<string, string> headers = null, dynamic data = null)
        {

            Method = method;
            Endpoint = endpoint;
            Headers = headers ?? new Dictionary<string, string>();
            Data = data;
        }

        public static IHttpRequest Get(IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Get, string.Empty, headers);

        public static IHttpRequest Get(string endpoint, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Get, endpoint, headers);

        public static IHttpRequest Put(object data, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Put, string.Empty, headers, data);

        public static IHttpRequest Put(string endpoint, object data = null, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Put, endpoint, headers, data);

        public static IHttpRequest Post(object data, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Post, string.Empty, headers, data);

        public static IHttpRequest Post(string endpoint, object data = null, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Post, endpoint, headers, data);

        public static IHttpRequest Delete(IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Delete, string.Empty, headers);

        public static IHttpRequest Delete(string endpoint, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Delete, endpoint, headers);
    }
}