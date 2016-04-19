using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Warden.Web.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool Empty(this string target) => string.IsNullOrWhiteSpace(target);

        public static bool NotEmpty(this string target) => !target.Empty();

        public static string TrimToUpper(this string value)
        {
            return value.OrEmpty().Trim().ToUpperInvariant();
        }

        public static string TrimToLower(this string value)
        {
            return value.OrEmpty().Trim().ToLowerInvariant();
        }

        public static string OrEmpty(this string value)
        {
            return value.Empty() ? "" : value;
        }

        public static bool EqualsCaseInvariant(this string value, string valueToCompare)
        {
            if (value.Empty())
                return valueToCompare.Empty();
            if (valueToCompare.Empty())
                return false;

            var fixedValue = value.TrimToUpper();
            var fixedValueToCompare = valueToCompare.TrimToUpper();

            return fixedValue == fixedValueToCompare;
        }

        public static bool Like(this string value, string valueToCompare)
        {
            if (value.Empty())
                return valueToCompare.Empty();

            var fixedValue = value.TrimToUpper();
            var fixedValueToCompare = valueToCompare.TrimToUpper();

            return fixedValue.Contains(fixedValueToCompare);
        }

        public static string AggregateLines(this IEnumerable<string> values)
            => values.Aggregate((x, y) => $"{x.Trim()}\n{y.Trim()}");

        public static bool IsEmail(this string value) => EmailRegex.IsMatch(value);
    }
}