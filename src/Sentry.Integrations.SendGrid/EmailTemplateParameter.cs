using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.Integrations.SendGrid
{
    public class EmailTemplateParameter
    {
        public string ReplacementTag { get; }
        public IEnumerable<string> Values { get; }

        public EmailTemplateParameter(string replacementTag, IEnumerable<string> values)
        {
            if (string.IsNullOrWhiteSpace(replacementTag))
                throw new ArgumentException("Replacement tag can not be empty", nameof(replacementTag));
            if (values?.Any() == false)
                throw new ArgumentException("Template values can not be empty", nameof(replacementTag));

            ReplacementTag = replacementTag;
            Values = values;
        }

        public static EmailTemplateParameter Create(string replacementTag, IList<string> values)
            => new EmailTemplateParameter(replacementTag, values);
    }
}