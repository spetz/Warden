using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Warden.Watchers.Web
{
    /// <summary>
    /// Custom HTTP service for executing the requests.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Executes the HTTP request and returns an instance of the IHttpResponse.
        /// </summary>
        /// <param name="baseUrl">Base URL of the request (e.g. http://www.example.com)</param>
        /// <param name="request">Instance of the IHttpRequest that contains request details (method type, headers, etc.).</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <returns>Instance of IHttpResponse.</returns>
        Task<IHttpResponse> ExecuteAsync(string baseUrl, IHttpRequest request, TimeSpan? timeout = null);
    }

    /// <summary>
    /// Default implementation of the IHttpService based on HtpClient.
    /// </summary>
    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;

        public HttpService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IHttpResponse> ExecuteAsync(string baseUrl, IHttpRequest request, TimeSpan? timeout = null)
        {
            SetRequestHeaders(request.Headers);
            SetTimeout(timeout);
            var response = await GetHttpResponseAsync(baseUrl, request);
            var data = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
            var valid = response.IsSuccessStatusCode;
            var responseHeaders = GetResponseHeaders(response.Headers);

            return valid
                ? HttpResponse.Valid(response.StatusCode, response.ReasonPhrase, responseHeaders, data)
                : HttpResponse.Invalid(response.StatusCode, response.ReasonPhrase, responseHeaders, data);
        }

        private async Task<HttpResponseMessage> GetHttpResponseAsync(string baseUrl, IHttpRequest request)
        {
            var fullUrl = request.GetFullUrl(baseUrl);

            return await ExecuteHttpResponseAsync(fullUrl, request);
        }

        private async Task<HttpResponseMessage> ExecuteHttpResponseAsync(string fullUrl, IHttpRequest request)
        {
            var method = request.Method;
            switch (method)
            {
                case HttpMethod.Get:
                    return await _client.GetAsync(fullUrl);
                case HttpMethod.Post:
                    return await _client.PostAsync(fullUrl, new StringContent(
                        JsonConvert.SerializeObject(request.Data ?? new { }), Encoding.UTF8, "application/json"));
                case HttpMethod.Put:
                    return await _client.PutAsync(fullUrl, new StringContent(
                        JsonConvert.SerializeObject(request.Data ?? new { }), Encoding.UTF8, "application/json"));
                case HttpMethod.Delete:
                    return await _client.DeleteAsync(fullUrl);
                default:
                    throw new ArgumentException($"Invalid HTTP method: {method}.", nameof(method));
            }
        }

        private void SetTimeout(TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
                _client.Timeout = timeout.Value;
        }

        private void SetRequestHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
            {
                var existingHeader = _client.DefaultRequestHeaders
                    .FirstOrDefault(x => string.Equals(x.Key, header.Key, StringComparison.CurrentCultureIgnoreCase));
                if (existingHeader.Key != null)
                    _client.DefaultRequestHeaders.Remove(existingHeader.Key);

                _client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private IDictionary<string, string> GetResponseHeaders(HttpResponseHeaders headers)
            => headers?.ToDictionary(header => header.Key, header => header.Value?.FirstOrDefault()) ??
               new Dictionary<string, string>();
    }
}