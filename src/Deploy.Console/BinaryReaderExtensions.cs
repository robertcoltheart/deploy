using System.IO;
using System.Linq;

namespace Deploy.Console
{
    public static class BinaryReaderExtensions
    {
        public static ushort[] ReadUint16Array(this BinaryReader reader, int count)
        {
            return Enumerable.Repeat(0, count)
                .Select(x => reader.ReadUInt16())
                .ToArray();
        }

        public static uint[] ReadUint32Array(this BinaryReader reader, int count)
        {
            return Enumerable.Repeat(0, count)
                .Select(x => reader.ReadUInt32())
                .ToArray();
        }

        public static uint ReadUint24(this BinaryReader reader)
        {
            return (uint) (reader.ReadByte() << 16 |
                           reader.ReadByte() << 8 |
                           reader.ReadByte());
        }
    }
}
