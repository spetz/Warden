using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warden.Watchers;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Integration with the Cachet (https://cachethq.io) - an open source status page system.
    /// </summary>
    public class CachetIntegration : IIntegration
    {
        private readonly CachetIntegrationConfiguration _configuration;
        private readonly ICachetService _cachetService;

        public CachetIntegration(CachetIntegrationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Cachet Integration configuration has not been provided.");
            }

            _configuration = configuration;
            _cachetService = _configuration.CachetServiceProvider();
        }

        /// <summary>
        /// Gets a component by id using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the component.</param>
        /// <returns>Details of component if exists.</returns>
        public async Task<Component> GetComponentAsync(int id)
            => await _cachetService.GetComponentAsync(id);

        /// <summary>
        /// Gets a component by name using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <returns>Details of component if exists.</returns>
        public async Task<Component> GetComponentAsync(string name)
            => await _cachetService.GetComponentAsync(name);

        /// <summary>
        /// Gets a components by name using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <returns>Details of components if exist.</returns>
        public async Task<IEnumerable<Component>> GetComponentsAsync(string name)
            => await _cachetService.GetComponentsAsync(name);

        /// <summary>
        /// Creates a component using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="description">Description of the component.</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Details of created component if operation has succeeded.</returns>
        public async Task<Component> CreateComponentAsync(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
            => await _cachetService.CreateComponentAsync(name, status, description, link, order, groupId, enabled);

        /// <summary>
        /// Updates a component using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the component.</param>
        /// <param name="name">Name of the component.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Details of created component if operation has succeeded.</returns>
        public async Task<Component> UpdateComponentAsync(int id, string name = null, int status = 1,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
            => await _cachetService.UpdateComponentAsync(id, name, status, link, order, groupId, enabled);

        /// <summary>
        /// Deletes a component using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the component.</param>
        /// <returns>True if operation has succeeded, otherwise false.</returns>
        public async Task<bool> DeleteComponentAsync(int id) 
            => await _cachetService.DeleteComponentAsync(id);

        /// <summary>
        /// Gets incidents by component id using the Cachet API.
        /// </summary>
        /// <param name="componentId">Id of the component.</param>
        /// <returns>Details of incidents if exist.</returns>
        public async Task<IEnumerable<Incident>> GetIncidentsAsync(int componentId)
            => await _cachetService.GetIncidentsAsync(componentId);

        /// <summary>
        /// Creates an incident using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the incident.</param>
        /// <param name="message">A message (supporting Markdown) to explain more.</param>
        /// <param name="status">Status of the incident (1-4).</param>
        /// <param name="visible">Whether the incident is publicly visible (1 = true by default).</param>
        /// <param name="componentId">Id of the component (0 by default).</param>
        /// <param name="componentStatus">The status to update the given component with (1-4).</param>
        /// <param name="notify">Whether to notify subscribers (false by default).</param>
        /// <param name="createdAt">When the incident was created.</param>
        /// <param name="template">The template slug to use.</param>
        /// <param name="vars">The variables to pass to the template.</param>
        /// <returns>Details of created incident if operation has succeeded.</returns>
        public async Task<Incident> CreateIncidentAsync(string name, string message, int status, int visible = 1,
            int componentId = 0, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars)
            => await _cachetService.CreateIncidentAsync(name, message, status, visible, componentId,
                componentStatus, notify, createdAt, template, vars);

        /// <summary>
        /// Updates an incident using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the incident.</param>
        /// <param name="name">Name of the incident.</param>
        /// <param name="message">A message (supporting Markdown) to explain more.</param>
        /// <param name="status">Status of the incident (1-4).</param>
        /// <param name="visible">Whether the incident is publicly visible (1 = true by default).</param>
        /// <param name="componentId">Id of the component (0 by default).</param>
        /// <param name="componentStatus">The status to update the given component with (1-4).</param>
        /// <param name="notify">Whether to notify subscribers (false by default).</param>
        /// <returns>Details of updated incident if operation has succeeded.</returns>
        public async Task<Incident> UpdateIncidentAsync(int id, string name = null, string message = null,
            int status = 1, int visible = 1, int componentId = 0, int componentStatus = 1, bool notify = false)
            => await _cachetService.UpdateIncidentAsync(id, name, message, status, visible, componentId,
                componentStatus, notify);

        /// <summary>
        /// Deletes an incident using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the incident.</param>
        /// <returns>True if operation has succeeded, otherwise false.</returns>
        public async Task<bool> DeleteIncidentAsync(int id)
            => await _cachetService.DeleteIncidentAsync(id);

        /// <summary>
        /// Saves the IWardenIteration using Cachet API.
        /// </summary>
        /// <param name="iteration">Iteration object that will be saved using Cachet API.</param>
        /// <param name="notify">Flag determining whether to notify the system administrator(s).</param>
        /// <param name="saveValidIncidents">Flag determining whether to save the valid incidents even if there were no errors previously reported.</param>
        /// <returns></returns>
        public async Task SaveIterationAsync(IWardenIteration iteration, bool notify = false, 
            bool saveValidIncidents = false)
        {
            var tasks = iteration.Results.Select(x => SaveCheckResultAsync(x, notify, saveValidIncidents));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Saves the IWardenCheckResult using Cachet API.
        /// </summary>
        /// <param name="result">Result object that will be saved using Cachet API.</param>
        /// <param name="notify">Flag determining whether to notify the system administrator(s).</param>
        /// <param name="saveValidIncidents">Flag determining whether to save the valid incidents even if there were no errors previously reported.</param>
        /// <returns></returns>
        public async Task SaveCheckResultAsync(IWardenCheckResult result, bool notify = false,
             bool saveValidIncidents = false)
        {
            var groupKey = result.WatcherCheckResult.WatcherGroup ?? string.Empty;
            var groupId = _configuration.WatcherGroups.ContainsKey(groupKey)
                ? _configuration.WatcherGroups[groupKey]
                : _configuration.GroupId;
            var checkResult = result.WatcherCheckResult;
            var name = checkResult.WatcherName;
            var status = checkResult.IsValid ? ComponentStatus.Operational : ComponentStatus.MajorOutage;
            var component = await _cachetService.GetComponentAsync(checkResult.WatcherName);
            if (component == null)
            {
                component = await _cachetService.CreateComponentAsync(name,
                    status, groupId: groupId);
            }
            else
            {
                component = await _cachetService.UpdateComponentAsync(component.Id,
                    name, status, groupId: groupId);
            }
            await SaveIncidentAsync(component.Id, checkResult, notify, saveValidIncidents);
        }

        private async Task SaveIncidentAsync(int componentId, IWatcherCheckResult result, bool notify = false,
            bool saveValidIncident = false)
        {
            var date = _configuration.DateTimeProvider().Date;
            var componentStatus = result.IsValid ? ComponentStatus.Operational : ComponentStatus.MajorOutage;
            var incidentStatus = result.IsValid ? IncidentStatus.Fixed : IncidentStatus.Identified;
            var incidents = await _cachetService.GetIncidentsAsync(componentId);
            var existingIncidentStatus = incidents.FirstOrDefault(x => x.ComponentId == componentId)?.Status;

            //If there's neither failure nor previous incident reported, do not report a valid service check.
            if (!saveValidIncident && result.IsValid &&
                (existingIncidentStatus == null || existingIncidentStatus == incidentStatus))
                return;

            var name = $"{result.WatcherName} check is {(result.IsValid ? "valid" : "invalid")}";
            var incident = incidents.FirstOrDefault(x => x.ComponentId == componentId &&
                                                         x.CreatedAt?.Date == date);
            var message = result.Description;
            if (incident == null)
            {
                incident = await _cachetService.CreateIncidentAsync(name, message,
                    incidentStatus, notify: notify, componentId: componentId,
                    componentStatus: componentStatus);
            }
            else
            {
                incident = await _cachetService.UpdateIncidentAsync(incident.Id, name,
                    message, incidentStatus, notify: notify, componentId: componentId,
                    componentStatus: componentStatus);
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="accessToken">Access token of the Cachet account.</param>
        /// <param name="configurator">Lambda expression for configuring the Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(string apiUrl, string accessToken,
            Action<CachetIntegrationConfiguration.Builder> configurator = null)
        {
            var config = new CachetIntegrationConfiguration.Builder(apiUrl, accessToken);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="apiUrl">URL of the Cachet API.</param>
        /// <param name="username">Username of the Cachet account.</param>
        /// <param name="password">Password of the Cachet account.</param>
        /// <param name="configurator">Lambda expression for configuring the Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(string apiUrl, string username, string password, 
            Action<CachetIntegrationConfiguration.Builder> configurator)
        {
            var config = new CachetIntegrationConfiguration.Builder(apiUrl, username, password);
            configurator?.Invoke(config);

            return Create(config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of CachetIntegration.
        /// </summary>
        /// <param name="configuration">Configuration of Cachet integration.</param>
        /// <returns>Instance of CachetIntegration.</returns>
        public static CachetIntegration Create(CachetIntegrationConfiguration configuration)
            => new CachetIntegration(configuration);
    }
}