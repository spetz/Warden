using System.Collections.Generic;
using Newtonsoft.Json;

namespace Warden.Integrations.SendGrid
{
    //Custom SendGrid message class - hopefully, a temporary solution.
    public class SendGridEmailMessage
    {
        public List<Personalization> Personalizations { get; set; } = new List<Personalization>();
        public Person From { get; set; } = new Person();
        public string Subject { get; set; }
        public List<MessageContent> Content { get; set; }

        [JsonProperty("template_id")]
        public string TemplateId { get; set; }

        public class Person
        {
            public string Email { get; set; }
        }

        public class Personalization
        {
            public List<Person> To { get; set; }
            public Dictionary<string, List<string>> Substitutions { get; set; }
        }

        public class MessageContent
        {
            public string Type { get; set; } = "text/plain";
            public string Value { get; set; }
        }
    }
}