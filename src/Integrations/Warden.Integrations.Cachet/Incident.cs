using System;
using Newtonsoft.Json;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Incident object used by the Cachet.
    /// </summary>
    public class Incident
    {
        /// <summary>
        /// Name of the incident.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A message (supporting Markdown) to explain more.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Status of the incident (1-4).
        /// </summary>
        public int Status { get; }

        /// <summary>
        /// Whether the incident is publicly visible (1 = true by default).
        /// </summary>
        public int Visible { get; }

        /// <summary>
        /// Component to update. (Required with component_status).
        /// </summary>
        [JsonProperty("component_id")]
        public string ComponentId { get; }

        /// <summary>
        /// The status to update the given component (1-4).
        /// </summary>
        [JsonProperty("component_status")]
        public int ComponentStatus { get; }

        /// <summary>
        /// Whether to notify subscribers (false by default).
        /// </summary>
        public bool Notify { get; }

        /// <summary>
        /// When the incident was created (actual UTC date by default).
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        /// <summary>
        /// The template slug to use.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// The variables to pass to the template.
        /// </summary>
        public string[] Vars { get; }

        protected Incident(string name, string message, int status, int visible,
            string componentId, int componentStatus, bool notify,
            DateTime createdAt, string template, params string[] vars)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name of the incident can not be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message of the incident can not be empty.", nameof(name));
            if (status < 1 || status > 4)
                throw new ArgumentException("Status of the incident is invalid.", nameof(name));
            if (visible < 0 || visible > 1)
                throw new ArgumentException("Visible flag of the incident is invalid.", nameof(name));
            if (componentStatus < 1 || componentStatus > 4)
                throw new ArgumentException("Status of the component is invalid.", nameof(name));

            Name = name;
            Message = message;
            Status = status;
            Visible = visible;
            ComponentId = componentId;
            ComponentStatus = componentStatus;
            Notify = notify;
            CreatedAt = createdAt;
            Template = template;
            Vars = vars;
        }

        /// <summary>
        /// Factory method for creating incident details.
        /// </summary>
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
        /// <returns>Instance of Incident.</returns>
        public static Incident Create(string name, string message, int status, int visible = 1,
            string componentId = null, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars)
            => new Incident(name, message, status, visible, componentId, componentStatus,
                notify, createdAt.GetValueOrDefault(DateTime.UtcNow), template, vars);
    }
}