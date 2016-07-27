using System.Collections.Generic;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Envelopes the collection data returned from the Cachet API.
    /// </summary>
    /// <typeparam name="T">Type of an object.</typeparam>
    public class EnvelopeCollection<T>
    {
        public List<T> Data { get; set; }
    }
}