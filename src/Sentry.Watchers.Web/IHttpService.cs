using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sentry.Watchers.Web
{
    public interface IHttpService
    {
        Task<IHttpResponse> ExecuteAsync(IHttpRequest request, TimeSpan? timeout = null);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;

        public HttpService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IHttpResponse> ExecuteAsync(IHttpRequest request, TimeSpan? timeout = null)
        {
            SetRequestHeaders(request.Headers);
            SetTimeout(timeout);
            var response = await GetHttpResponseAsync(request);
            var data = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
            var valid = response.IsSuccessStatusCode;
            var responseHeaders = GetResponseHeaders(response.Headers);

            return valid
                ? HttpResponse.Valid(response.StatusCode, response.ReasonPhrase, responseHeaders, data)
                : HttpResponse.Invalid(response.StatusCode, response.ReasonPhrase, responseHeaders, data);
        }

        private async Task<HttpResponseMessage> GetHttpResponseAsync(IHttpRequest request)
        {
            var fullUrl = $"{_client.BaseAddress}{request.Endpoint}";
            var method = request.Method;
            HttpResponseMessage response;
            switch (method)
            {
                case HttpMethod.Get:
                    response = await _client.GetAsync(fullUrl);
                    break;
                case HttpMethod.Put:
                    response = await _client.PutAsJsonAsync(fullUrl, request.Data);
                    break;
                case HttpMethod.Post:
                    response = await _client.PostAsJsonAsync(fullUrl, request.Data);
                    break;
                case HttpMethod.Delete:
                    response = await _client.DeleteAsync(fullUrl);
                    break;
                default:
                    throw new ArgumentException($"Invalid HTTP method: {method}.", nameof(method));
            }

            return response;
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
                    .FirstOrDefault(x => string.Equals(x.Key, header.Key, StringComparison.InvariantCultureIgnoreCase));
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