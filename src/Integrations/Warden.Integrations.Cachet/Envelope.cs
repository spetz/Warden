namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Envelopes the data returned from the Cachet API.
    /// </summary>
    /// <typeparam name="T">Type of an object.</typeparam>
    public class Envelope<T>
    {
        public T Data { get; set; }
    }
}