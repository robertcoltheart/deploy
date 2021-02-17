using System.Collections.Generic;

namespace Deploy
{
    internal static class EnumerableExtensions
    {
        public static string Merge(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }
    }
}
