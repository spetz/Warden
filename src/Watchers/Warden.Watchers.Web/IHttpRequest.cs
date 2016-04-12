using System.Collections.Generic;

namespace Warden.Watchers.Web
{
    /// <summary>
    /// Representation of the HTTP method type.
    /// </summary>
    public enum HttpMethod
    {
        Get = 1,
        Put = 2,
        Post = 3,
        Delete = 4
    }

    /// <summary>
    /// Custom interface for HTTP request. 
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Type of HTTP method.
        /// </summary>
        HttpMethod Method { get; }

        /// <summary>
        /// Endpoint of the request (part of the base URL).
        /// </summary>
        string Endpoint { get; }

        /// <summary>
        /// Request data that may be required for either POST or PUT request.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Request headers.
        /// </summary>
        IDictionary<string, string> Headers { get; }
    }


    /// <summary>
    /// Default implementation of the IHttpRequest.
    /// </summary>
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

        /// <summary>
        /// GET request with optional headers.
        /// </summary>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Get(IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Get, string.Empty, headers);

        /// <summary>
        /// GET request with endpoint and optional headers.
        /// </summary>
        /// <param name="endpoint">Endpoint of the request</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Get(string endpoint, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Get, endpoint, headers);

        /// <summary>
        /// POST request with data and optional headers.
        /// </summary>
        /// <param name="data">Request body</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Post(object data, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Post, string.Empty, headers, data);

        /// <summary>
        /// POST request with endpoint and optional data and headers.
        /// </summary>
        /// <param name="endpoint">Endpoint of the request</param>
        /// <param name="data">Request body</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Post(string endpoint, object data = null, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Post, endpoint, headers, data);

        /// <summary>
        /// PUT request with data and optional headers.
        /// </summary>
        /// <param name="data">Request body</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Put(object data, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Put, string.Empty, headers, data);

        /// <summary>
        /// PUT request with endpoint and optional data and headers.
        /// </summary>
        /// <param name="endpoint">Endpoint of the request</param>
        /// <param name="data">Request body</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Put(string endpoint, object data = null, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Put, endpoint, headers, data);

        /// <summary>
        /// DELETE request with optional headers.
        /// </summary>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Delete(IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Delete, string.Empty, headers);

        /// <summary>
        /// DELETE request with endpoint and optional headers.
        /// </summary>
        /// <param name="endpoint">Endpoint of the request</param>
        /// <param name="headers">Request headers</param>
        /// <returns>Instance of IHttpRequest.</returns>
        public static IHttpRequest Delete(string endpoint, IDictionary<string, string> headers = null)
            => new HttpRequest(HttpMethod.Delete, endpoint, headers);
    }
}