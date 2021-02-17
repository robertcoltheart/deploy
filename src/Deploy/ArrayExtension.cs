using System;

namespace Deploy
{
    internal static class ArrayExtension
    {
        public static T GetOrDefault<T>(this T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return default;

            return array[index];
        }

        public static void Write<T>(this T[] array, char value, int index)
        {
            BitConverter.GetBytes(value).CopyTo(array, index);
        }

        public static void Write<T>(this T[] array, ushort value, int index)
        {
            BitConverter.GetBytes(value).CopyTo(array, index);
        }
    }
}
