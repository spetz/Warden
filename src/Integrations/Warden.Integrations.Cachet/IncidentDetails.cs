using System;

namespace Warden.Integrations.Cachet
{
    public class IncidentDetails : Incident
    {
        /// <summary>
        /// ID of the incident to update.
        /// </summary>
        public int Id { get; }

        public IncidentDetails(int id, string name, string message, int status,
            int visible, string componentId, int componentStatus,
            bool notify, DateTime createdAt, string template, params string[] vars)
            : base(name, message, status, visible, componentId,
                componentStatus, notify, createdAt, template, vars)
        {
            Id = id;
        }
    }
}