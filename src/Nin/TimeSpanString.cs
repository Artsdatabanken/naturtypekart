using System;
using System.Linq;

namespace Nin.Common
{
    public static class TimeSpanString
    {
        public static string ToString(TimeSpan span)
        {
            string[] parts = {
                    ToString(span.Days, "day"),
                    ToString(span.Hours, "hour"),
                    ToString(span.Minutes, "minute"),
                    ToString(span.Seconds, "second")
                };
            string formatted = string.Join(", ", parts.Where(x => !string.IsNullOrEmpty(x)));
            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        private static string ToString(int count, string unit)
        {
            if (count <= 0)
                return string.Empty;
            var format = $"{count:0} {unit}";
            if (count != 1) format += "s";
            return format;
        }
    }
}