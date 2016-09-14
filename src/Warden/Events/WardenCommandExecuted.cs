namespace Warden.Events
{
    public class WardenCommandExecuted : IWardenEvent
    {
        public string Name { get; set; }
    }
}