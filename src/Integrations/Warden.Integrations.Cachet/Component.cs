using System;
using Newtonsoft.Json;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Component object used by the Cachet.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Name of the component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Status of the component (1-4).
        /// </summary>
        public int Status { get; }

        /// <summary>
        /// Description of the component.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// A hyperlink to the component.
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// Order of the component (0 by default).
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The group id that the component is within (0 by default).
        /// </summary>
        [JsonProperty("group_id")]
        public int GroupId { get; }

        /// <summary>
        /// Whether the component is enabled (true by default).
        /// </summary>
        public bool Enabled { get; }

        protected Component(string name, int status, string description,
            string link, int order, int groupId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name of the component can not be empty.", nameof(name));
            if (status < 1 || status > 4)
                throw new ArgumentException("Status of the component is invalid.", nameof(name));

            Name = name;
            Description = description;
            Status = status;
            Link = link;
            Order = order;
            GroupId = groupId;
            Enabled = enabled;
        }

        /// <summary>
        /// Factory method for creating component details.
        /// </summary>
        /// <param name="name">Name of the incident.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="description">Description of the component.</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Instance of Component.</returns>
        public static Component Create(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
            => new Component(name, status, description, link, order, groupId, enabled);
    }
}

