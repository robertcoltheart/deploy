using System;
using System.IO;

namespace Deploy
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteInt24(this BinaryWriter writer, int value)
        {
            writer.Write((byte) (value >> 16 & 0xff));
            writer.Write((byte) (value >> 8 & 0xff));
            writer.Write((byte) (value & 0xff));
        }

        public static void Write(this BinaryWriter writer, Guid value)
        {
            writer.Write(value.ToByteArray());
        }

        public static void Write(this BinaryWriter writer, Enum value)
        {
            writer.Write(Convert.ToInt32(value));
        }
    }
}
