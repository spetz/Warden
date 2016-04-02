using System.Collections.Generic;

namespace Sentry.Integrations.SendGrid
{
    public class EmailTemplateParameter
    {
        public string ReplacementTag { get; }
        public IEnumerable<string> Values { get; }

        public EmailTemplateParameter(string replacementTag, IEnumerable<string> values)
        {
            ReplacementTag = replacementTag;
            Values = values;
        }

        public static EmailTemplateParameter Create(string replacementTag, IList<string> values)
            => new EmailTemplateParameter(replacementTag, values);
    }
}