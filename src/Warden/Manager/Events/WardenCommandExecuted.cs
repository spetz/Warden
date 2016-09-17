namespace Warden.Manager.Events
{
    public class WardenCommandExecuted : IWardenEvent
    {
        public string Name { get; set; }
    }
}