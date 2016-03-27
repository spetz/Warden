using System.Collections.Generic;
using System.Net;

namespace Sentry.Watchers.Web
{
    /// <summary>
    /// Custom interface for HTTP response. 
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Status code of response.
        /// </summary>
        HttpStatusCode StatusCode { get; }
        /// <summary>
        /// Flag determining whether response is valid.
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// Reason phrase that can be provided in response.
        /// </summary>
        string ReasonPhrase { get; }
        /// <summary>
        /// Response headers.
        /// </summary>
        IDictionary<string, string> Headers { get; }
        /// <summary>
        /// Response content.
        /// </summary>
        string Data { get; }
    }

    /// <summary>
    /// Default implementation of the IHttpResponse.
    /// </summary>
    public class HttpResponse : IHttpResponse
    {
        public HttpStatusCode StatusCode { get; }
        public bool IsValid { get; }
        public string ReasonPhrase { get; }
        public IDictionary<string, string> Headers { get; }
        public string Data { get; }

        protected HttpResponse(HttpStatusCode statusCode, bool isValid, 
            string reasonPhrase, IDictionary<string, string> headers, string data)
        {
            StatusCode = statusCode;
            IsValid = isValid;
            ReasonPhrase = reasonPhrase;
            Headers = headers ?? new Dictionary<string, string>();
            Data = data;
        }

        /// <summary>
        /// Valid HTTP response.
        /// </summary>
        /// <param name="statusCode">Status code of response.</param>
        /// <param name="reasonPhrase">Reason phrase.</param>
        /// <param name="headers">Response headers.</param>
        /// <param name="data">Response content.</param>
        /// <returns>Instance of IHttpResponse.</returns>
        public static IHttpResponse Valid(HttpStatusCode statusCode, string reasonPhrase,
            IDictionary<string, string> headers, string data) => new HttpResponse(statusCode, true,
                reasonPhrase, headers, data);

        /// <summary>
        /// Invalid HTTP response.
        /// </summary>
        /// <param name="statusCode">Status code of response.</param>
        /// <param name="reasonPhrase">Reason phrase.</param>
        /// <param name="headers">Response headers.</param>
        /// <param name="data">Response content.</param>
        /// <returns>Instance of IHttpResponse.</returns>
        public static IHttpResponse Invalid(HttpStatusCode statusCode, string reasonPhrase,
            IDictionary<string, string> headers, string data) => new HttpResponse(statusCode, false,
                reasonPhrase, headers, data);
    }
}