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
        /// Id of the component.
        /// </summary>
        [JsonProperty]
        public int Id { get; protected set; }

        /// <summary>
        /// Name of the component.
        /// </summary>
        [JsonProperty]
        public string Name { get; protected set; }

        /// <summary>
        /// Status of the component (1-4).
        /// </summary>
        [JsonProperty]
        public int Status { get; protected set; }

        /// <summary>
        /// Description of the component.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// A hyperlink to the component.
        /// </summary>
        [JsonProperty]
        public string Link { get; protected set; }

        /// <summary>
        /// Order of the component (0 by default).
        /// </summary>
        [JsonProperty]
        public int Order { get; protected set; }

        /// <summary>
        /// The group id that the component is within (0 by default).
        /// </summary>
        [JsonProperty("group_id")]
        public int GroupId { get; protected set; }

        /// <summary>
        /// Whether the component is enabled (true by default).
        /// </summary>
        [JsonProperty]
        public bool Enabled { get; protected set; }

        /// <summary>
        /// When the component was created.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// When the component was updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// When the component was deleted.
        /// </summary>
        [JsonProperty("deleted_at")]
        public DateTime? DeletedAt { get; protected set; }

        /// <summary>
        /// A name of the status.
        /// </summary>
        [JsonProperty("status_name")]
        public string StatusName { get; protected set; }

        /// <summary>
        /// Tags of the component.
        /// </summary>
        [JsonProperty]
        public string[] Tags { get; protected set; }

        protected Component()
        {
        }

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

