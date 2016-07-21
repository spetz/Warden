using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Custom Cachet client for executing HTTP requests to the API.
    /// </summary>
    public interface ICachetService
    {
        /// <summary>
        /// Creates a component using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the incident.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="description">Description of the component.</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Details of created component if operation has succeeded.</returns>
        Task<ComponentDetails> CreateComponentAsync(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true);

        Task UpdateComponentAsync(Component component);
        Task DeleteComponentAsync(int id);

        /// <param name="name">Name of the incident.</param>
        /// <param name="message">A message (supporting Markdown) to explain more.</param>
        /// <param name="status">Status of the incident (1-4).</param>
        /// <param name="visible">Whether the incident is publicly visible (1 = true by default).</param>
        /// <param name="componentId">Component to update. (Required with component_status).</param>
        /// <param name="componentStatus">The status to update the given component with (1-4).</param>
        /// <param name="notify">Whether to notify subscribers (false by default).</param>
        /// <param name="createdAt">When the incident was created (actual UTC date by default).</param>
        /// <param name="template">The template slug to use.</param>
        /// <param name="vars">The variables to pass to the template.</param>
        /// <returns>Details of created incident if operation has succeeded.</returns>
        Task<IncidentDetails> CreateIncidentAsync(string name, string message, int status, int visible = 1,
            string componentId = null, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars);

        Task UpdateIncidentAsync(Incident incident);
        Task DeleteIncidentAsync(int id);
    }

    /// <summary>
    /// Default implementation of the ICachetService based on HttpClient.
    /// </summary>
    public class CachetService : ICachetService
    {
        private readonly HttpClient _client = new HttpClient();

        public CachetService(Uri apiUrl)
        {
            _client.BaseAddress = apiUrl;
        }

        public async Task<ComponentDetails> CreateComponentAsync(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
        {
            var component = Component.Create(name, status, description, link, order, groupId, enabled);
            var response = await PostAsync("/components", component);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ComponentDetails>(content);
        }

        public async Task UpdateComponentAsync(Component component)
        {
            throw new System.NotImplementedException();
        }

        public async Task DeleteComponentAsync(int id)
        {
        }

        public async Task<IncidentDetails> CreateIncidentAsync(string name, string message, int status, int visible = 1,
            string componentId = null, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars)
        {
            var incident = Incident.Create(name, message, status, visible, componentId, componentStatus, notify,
                createdAt, template, vars);
            var response = await PostAsync("/incidents", incident);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IncidentDetails>(content);
        }

        public async Task UpdateIncidentAsync(Incident incident)
        {
        }

        public async Task DeleteIncidentAsync(int id)
        {
        }

        private async Task<HttpResponseMessage> PutAsync(string endpoint, object data, 
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
            => await ExecuteAsync(() => _client.PutAsync(GetFullUrl(endpoint),
                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")),
                headers, timeout, failFast);

        private async Task<HttpResponseMessage> PostAsync(string endpoint, object data, 
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
            => await ExecuteAsync(() => _client.PostAsync(GetFullUrl(endpoint),
                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")),
                headers, timeout, failFast);

        private async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> request, 
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
        {
            SetRequestHeaders(headers);
            SetTimeout(timeout);
            try
            {
                var response = await request();
                if (response.IsSuccessStatusCode)
                    return response;
                if (!failFast)
                    return response;

                throw new Exception("Received invalid HTTP response from Cachet API" +
                                    $" with status code: {response.StatusCode}. " +
                                    $"Reason phrase: {response.ReasonPhrase}");
            }
            catch (Exception exception)
            {
                if (!failFast)
                    return null;

                throw new Exception("There was an error while executing the HTTP request to the Cachet API: " +
                                    $"{exception}", exception);
            }
        }

        private string GetFullUrl(string endpoint) => $"{_client.BaseAddress}/{endpoint}";

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