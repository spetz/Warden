using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sentry.Watchers.Website
{
    public interface IHttpClient
    {
        Uri BaseAddress { get; set; }
        TimeSpan Timeout { get; set; }
        HttpRequestHeaders RequestHeaders { get; }
        Task<HttpResponseMessage> GetAsync(string endpoint);
    }

    public class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient _client;

        public Uri BaseAddress
        {
            get { return _client.BaseAddress; }
            set { _client.BaseAddress = value; }
        }

        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        public HttpRequestHeaders RequestHeaders => _client.DefaultRequestHeaders;

        public HttpClientWrapper(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
            => await _client.GetAsync(endpoint);
    }
}