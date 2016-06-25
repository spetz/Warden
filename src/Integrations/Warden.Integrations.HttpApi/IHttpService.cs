using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Warden.Integrations.HttpApi
{
    /// <summary>
    /// Custom HTTP client for executing the POST request.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Executes the HTTP POST request.
        /// </summary>
        /// <param name="url">Full API URL (base + endpoint) of the request (e.g. http://www.my-api.com)</param>
        /// <param name="data">Request data in JSON format that will be sent.</param>
        /// <param name="headers">Optional request headers.</param>
        /// <param name="timeout">Optional timeout for the request.</param>
        /// <param name="failFast">Flag determining whether an exception should be thrown if received reponse is invalid (false by default).</param>
        /// <returns>Instance of IHttpResponse.</returns>
        Task PostAsync(string url, string data, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool failFast = false);
    }

    /// <summary>
    /// Default implementation of the IHttpService based on HttpService.
    /// </summary>
    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;

        public HttpService(HttpClient client)
        {
            _client = client;
        }

        public async Task PostAsync(string url, string data, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool failFast = false)
        {
            SetRequestHeaders(headers);
            SetTimeout(timeout);
            try
            {
                var response = await _client.PostAsync(url, new StringContent(
                    data, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                    return;
                if (!failFast)
                    return;

                throw new Exception($"Received invalid HTTP response with status code: {response.StatusCode}. " +
                    $"Reason phrase: {response.ReasonPhrase}");
            }
            catch (Exception exception)
            {
                if (!failFast)
                    return;

                throw new Exception($"There was an error while executing the PostAsync(): " +
                                    $"{exception}", exception);
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
    }
}