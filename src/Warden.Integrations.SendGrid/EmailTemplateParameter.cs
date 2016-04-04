using System;
using System.Collections.Generic;
using System.Linq;

namespace Warden.Integrations.SendGrid
{
    //Template parameter for SendGrid transactional template
    public class EmailTemplateParameter
    {
        public string ReplacementTag { get; }
        public IEnumerable<string> Values { get; }

        public EmailTemplateParameter(string replacementTag, IEnumerable<string> values)
        {
            if (string.IsNullOrWhiteSpace(replacementTag))
                throw new ArgumentException("Replacement tag can not be empty", nameof(replacementTag));
            if (values?.Any() == false)
                throw new ArgumentException("Replacement tag values can not be empty", nameof(replacementTag));

            ReplacementTag = replacementTag;
            Values = values;
        }

        /// <summary>
        /// Factory method for creating a new instance of EmailTemplateParameter.
        /// </summary>
        /// <param name="replacementTag">Name of the replacement tag.</param>
        /// <param name="values">Replacement tag values.</param>
        /// <returns>Instance of EmailTemplateParameter.</returns>
        public static EmailTemplateParameter Create(string replacementTag, IEnumerable<string> values)
            => new EmailTemplateParameter(replacementTag, values);
    }
}