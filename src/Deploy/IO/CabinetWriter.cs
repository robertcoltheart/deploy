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

        private readonly string[] _filenames;
        private readonly Func<string, string> _nameFunc;

        private int _dataOffset;
        private int _blocksCount;
        private int _cabinetLength;

        public CabinetWriter(IEnumerable<string> filenames, Func<string, string> nameFunc)
        {
            _nameFunc = nameFunc;

            _filenames = filenames.ToArray();
        }

        public void Write(Stream stream)
        {
            FileInfo[] files = _filenames.Select(x => new FileInfo(x)).ToArray();

            int fileNamesLength = _filenames.Sum(x => Encoding.UTF8.GetByteCount(_nameFunc(x))) + _filenames.Length;
            int totalFilesLength = files.Length*FileOverheadLength + fileNamesLength;
            int dataLength = Convert.ToInt32(files.Sum(x => x.Length));

            _dataOffset = HeaderLength + FolderLength + totalFilesLength;
            _blocksCount = (dataLength + BlockSize - 1)/BlockSize;

            int totalDataLength = dataLength + _blocksCount * DataOverheadLength;
            _cabinetLength = HeaderLength + FolderLength + totalFilesLength + totalDataLength;

            var writer = new BinaryWriter(stream);
            
            WriteHeader(writer);
            WriteFolder(writer);
            WriteFiles(writer);
            WriteData(writer);
        }

        private void WriteData(BinaryWriter writer)
        {
            var buffer = new byte[BlockSize];
            int length = 0;

            foreach (FileInfo file in _filenames.Select(x => new FileInfo(x)))
            {
                using (FileStream fileStream = file.OpenRead())
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
                WriteDataBlock(writer, buffer, length);
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

            foreach (string filename in _filenames)
            {
                var file = new FileInfo(filename);

                DateTime dateTime = file.LastWriteTime;
                int date = ((dateTime.Year - 1980) << 9) + (dateTime.Month << 5) + dateTime.Day;
                int time = (dateTime.Hour << 11) + (dateTime.Minute << 5) + (dateTime.Second / 2);

                writer.Write((uint)file.Length);
                writer.Write((uint)offset);
                writer.Write((ushort)0);
                writer.Write((ushort)date);
                writer.Write((ushort)time);
                writer.Write((ushort)0x20);
                writer.Write(Encoding.UTF8.GetBytes(_nameFunc(filename)));
                writer.Write((byte)0);

                offset += file.Length;
            }
        }

        private void WriteFolder(BinaryWriter writer)
        {
            writer.Write(_dataOffset);
            writer.Write((ushort) _blocksCount);
            writer.Write((ushort)0);
        }

        private void WriteHeader(BinaryWriter writer)
        {
            writer.Write(Signature);
            writer.Write(0);
            writer.Write(_cabinetLength);
            writer.Write(0);
            writer.Write(0x2c);
            writer.Write(0);
            writer.Write((byte) 3);
            writer.Write((byte) 1);
            writer.Write((ushort) 1);
            writer.Write((ushort) _filenames.Length);
            writer.Write((ushort) 0);
            writer.Write((ushort) 0);
            writer.Write((ushort) 0);
        }
    }
}