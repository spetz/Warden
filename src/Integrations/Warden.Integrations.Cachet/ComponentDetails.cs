namespace Warden.Integrations.Cachet
{
    public class ComponentDetails : Component
    {
        /// <summary>
        /// ID of the component to update.
        /// </summary>
        public int Id { get; }

        protected ComponentDetails(int id, string name, int status, string description,
            string link, int order, int groupId, bool enabled)
            : base(name, status, description, link, order, groupId, enabled)
        {
            Id = id;
        }
    }
}