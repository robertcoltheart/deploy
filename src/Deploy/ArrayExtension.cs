using System;

namespace Deploy
{
    internal static class ArrayExtension
    {
        public static char GetOrDefault(this char[] array, int index)
        {
            if (index < 0 || index >= array.Length)
            {
                return default;
            }

            return array[index];
        }

        public static void Write(this byte[] array, char value, int index)
        {
            array[index] = (byte) (value & 0xff);
            array[index + 1] = (byte) (value >> 8);
        }

        public static void Write(this byte[] array, ushort value, int index)
        {
            array[index] = (byte) (value & 0xff);
            array[index + 1] = (byte) (value >> 8);

            var a = BitConverter.GetBytes(value);
            a.CopyTo(array, index);
        }
    }
}
