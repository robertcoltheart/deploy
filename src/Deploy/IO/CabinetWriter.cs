using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Deploy.IO
{
    internal class CabinetWriter
    {
        private const uint Signature = 'M' | 'S' << 8 | 'C' << 16 | 'F' << 24;

        private const int BlockSize = 32768;

        private const int HeaderLength = 36;

        private const int FolderLength = 8;

        private const int FileOverheadLength = 16;

        private const int DataOverheadLength = 8;

        private readonly string[] files;

        private readonly Func<string, string> nameFunc;

        private int dataOffset;

        private int blocksCount;

        private int cabinetLength;

        public CabinetWriter(IEnumerable<string> files, Func<string, string> nameFunc)
        {
            this.nameFunc = nameFunc;

            this.files = files.ToArray();
        }

        public void Write(Stream stream)
        {
            var fileInfos = files.Select(x => new FileInfo(x)).ToArray();

            var fileNamesLength = files.Sum(x => Encoding.UTF8.GetByteCount(nameFunc(x))) + files.Length;
            var totalFilesLength = fileInfos.Length * FileOverheadLength + fileNamesLength;
            var dataLength = Convert.ToInt32(fileInfos.Sum(x => x.Length));

            dataOffset = HeaderLength + FolderLength + totalFilesLength;
            blocksCount = (dataLength + BlockSize - 1)/BlockSize;

            var totalDataLength = dataLength + blocksCount * DataOverheadLength;
            cabinetLength = HeaderLength + FolderLength + totalFilesLength + totalDataLength;

            var writer = new BinaryWriter(stream);
            
            WriteHeader(writer);
            WriteFolder(writer);
            WriteFiles(writer);
            WriteData(writer);
        }

        private void WriteData(BinaryWriter writer)
        {
            var buffer = new byte[BlockSize];
            var length = 0;

            foreach (var file in files.Select(x => new FileInfo(x)))
            {
                using (var fileStream = file.OpenRead())
                {
                    while (fileStream.Position < fileStream.Length)
                    {
                        length += fileStream.Read(buffer, length, buffer.Length - length);

                        if (length == buffer.Length)
                        {
                            WriteDataBlock(writer, buffer, length);
                            length = 0;
                        }
                    }
                }
            }

            if (length > 0)
            {
                WriteDataBlock(writer, buffer, length);
            }
        }

        private void WriteDataBlock(BinaryWriter writer, byte[] data, int length)
        {
            writer.Write(0);
            writer.Write((ushort)length);
            writer.Write((ushort)length);
            writer.Write(data, 0, length);
        }

        private void WriteFiles(BinaryWriter writer)
        {
            long offset = 0;

            foreach (var filename in files)
            {
                var file = new FileInfo(filename);

                var dateTime = file.LastWriteTime;
                var date = ((dateTime.Year - 1980) << 9) + (dateTime.Month << 5) + dateTime.Day;
                var time = (dateTime.Hour << 11) + (dateTime.Minute << 5) + (dateTime.Second / 2);

                writer.Write((uint)file.Length);
                writer.Write((uint)offset);
                writer.Write((ushort)0);
                writer.Write((ushort)date);
                writer.Write((ushort)time);
                writer.Write((ushort)0x20);
                writer.Write(Encoding.UTF8.GetBytes(nameFunc(filename)));
                writer.Write((byte)0);

                offset += file.Length;
            }
        }

        private void WriteFolder(BinaryWriter writer)
        {
            writer.Write(dataOffset);
            writer.Write((ushort) blocksCount);
            writer.Write((ushort)0);
        }

        private void WriteHeader(BinaryWriter writer)
        {
            writer.Write(Signature);
            writer.Write(0);
            writer.Write(cabinetLength);
            writer.Write(0);
            writer.Write(0x2c);
            writer.Write(0);
            writer.Write((byte) 3);
            writer.Write((byte) 1);
            writer.Write((ushort) 1);
            writer.Write((ushort) files.Length);
            writer.Write((ushort) 0);
            writer.Write((ushort) 0);
            writer.Write((ushort) 0);
        }
    }
}
