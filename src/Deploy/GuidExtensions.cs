using System;

namespace Deploy
{
    internal static class GuidExtensions
    {
        public static string ToStringFormatted(this Guid guid)
        {
            return $"{{{guid.ToString().ToUpper()}}}";
        }
    }
}
