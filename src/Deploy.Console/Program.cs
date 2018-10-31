using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deploy.Text;

namespace Deploy.Console
{
    public class Program
    {
        private const uint EndOfChain = 0xffff_fffe;
        private const uint FreeSector = 0xffff_ffff;
        private const int LongStringLength = 3;

        private const int Dictionary = 0;
        private const int CodePage = 1;
        private const int Title = 2;
        private const int Subject = 3;
        private const int Author = 4;
        private const int Keywords = 5;
        private const int Comments = 6;
        private const int Template = 7;
        private const int LastAuthor = 8;
        private const int RevisionNumber = 9;
        private const int EditTime = 10;
        private const int LastPrinted = 11;
        private const int CreateDateTime = 12;
        private const int LastSaveDateTime = 13;
        private const int PageCount = 14;
        private const int WordCount = 15;
        private const int CharCount = 16;
        private const int Thumbnail = 17;
        private const int ApplicationName = 18;
        private const int Security = 19;

        private static int sectorLength;
        private static uint sectorCutoff;
        private static uint[] fat;
        private static uint[] miniFat;
        private static DirectoryEntry[] directoryEntries;
        private static DirectoryEntry root;
        private static byte[] miniStream;
        private static int bytesPerString;
        private static StringValue[] strings;
        private static Row[] tables;
        private static Row[] columns;

        public static void Main(string[] args)
        {
            const string mediumFile = @"C:\Users\robert\Downloads\cffbe5f.msi";
            const string largeFile = @"C:\Windows\Installer\2ef3ee3.msi";
            const string orcaFile = @"C:\Users\robert\Downloads\orca.msi";
            const string myFile = @"C:\Users\robert\Downloads\deploy.msi";

            var encoding = new MsiEncoding();
            //encoding.GetBytes("Root Entry");
            //encoding.GetBytes("_Validation");

            var files = Directory.GetFiles(@"C:\Windows\Installer", "*.msi");

            foreach (var file in files)
            {
                using (var stream = File.OpenRead(file))
                using (var reader = new BinaryReader(stream))
                {
                    ReadHeader(reader);
                }
            }

            using (var reader = new BinaryReader(File.OpenRead(myFile)))
            {
                ReadHeader(reader);
            }
        }

        private static void ReadHeader(BinaryReader reader)
        {
            var signature = reader.ReadBytes(8);
            var clsid = new Guid(reader.ReadBytes(16));
            var minorVersion = reader.ReadUInt16();
            var majorVersion = reader.ReadUInt16();
            var byteOrder = reader.ReadUInt16();
            var sectorShift = reader.ReadUInt16();
            var miniSectorShift = reader.ReadUInt16();
            var reserved = reader.ReadBytes(6);
            var directorySectorsCount = reader.ReadUInt32();
            var fatSectorsCount = reader.ReadUInt32();
            var firstDirectorySector = reader.ReadUInt32();
            var transactionSignature = reader.ReadUInt32();
            var miniStreamCutoffLength = reader.ReadUInt32();
            var firstMiniFatSector = reader.ReadUInt32();
            var miniFatSectorsCount = reader.ReadUInt32();
            var firstDifatSector = reader.ReadUInt32();
            var difatSectorsCount = reader.ReadUInt32();
            var initialDifat = reader.ReadUint32Array(109);

            sectorLength = majorVersion == 3
                ? 512
                : 4096;

            sectorCutoff = miniStreamCutoffLength;

            fat = ReadFat(reader, initialDifat, firstDifatSector);
            miniFat = ReadMiniFat(reader, firstMiniFatSector);

            directoryEntries = ReadDirectoryEntries(reader, firstDirectorySector).ToArray();
            root = directoryEntries.First();

            miniStream = ReadStream(reader, root.Sector, root.Length);
            strings = ReadStrings(reader).ToArray();

            tables = ReadTables(reader).ToArray();
            columns = ReadColumns(reader).ToArray();

            ReadSummaryInformation(reader);
        }

        private static uint[] ReadFat(BinaryReader reader, uint[] initialDifat, uint firstDifatSector)
        {
            var difat = ReadDifat(reader, initialDifat, firstDifatSector);

            var blocks = new List<uint>();

            foreach (var sector in difat)
            {
                if (sector == FreeSector)
                    break;

                SetPosition(reader, sector);

                var block = reader.ReadUint32Array(sectorLength / 4);

                blocks.AddRange(block);
            }

            return blocks.ToArray();
        }

        private static uint[] ReadDifat(BinaryReader reader, uint[] initialDifat, uint firstDifatSector)
        {
            var sectors = new List<uint>(initialDifat);

            var sector = firstDifatSector;

            while (sector != EndOfChain && sector != FreeSector)
            {
                SetPosition(reader, sector);

                var block = reader.ReadUint32Array(sectorLength / 4);
                sectors.AddRange(block.Take(block.Length - 1));

                sector = block.Last();
            }

            return sectors.ToArray();
        }

        private static uint[] ReadMiniFat(BinaryReader reader, uint firstSector)
        {
            var blocks = new List<uint>();

            var chain = GetFatSectorChain(firstSector);

            foreach (var sector in chain)
            {
                SetPosition(reader, sector);

                var items = reader.ReadUint32Array(sectorLength / 4);

                blocks.AddRange(items);
            }

            return blocks.ToArray();
        }

        private static void ReadSummaryInformation(BinaryReader reader)
        {
            var buffer = ReadStream(reader, "\u0005SummaryInformation");

            using (var stream = new MemoryStream(buffer))
            using (var streamReader = new BinaryReader(stream))
            {
                var byteOrder = streamReader.ReadUInt16();
                var format = streamReader.ReadUInt16();
                var osVersion = streamReader.ReadUInt32();
                var clsid = new Guid(streamReader.ReadBytes(16));
                var reserved = streamReader.ReadUInt32();
                var formatId = new Guid(streamReader.ReadBytes(16));
                var offset = streamReader.ReadUInt32();

                streamReader.BaseStream.Position = offset;

                var section = streamReader.ReadUInt32();
                var propertiesCount = streamReader.ReadUInt32();

                var propertyOffsets = new List<Tuple<uint, uint>>();

                for (var i = 0; i < propertiesCount; i++)
                {
                    var propertyId = streamReader.ReadUInt32();
                    var propertyOffset = streamReader.ReadUInt32();

                    propertyOffsets.Add(Tuple.Create(propertyId, propertyOffset));
                }

                foreach (var propertyOffset in propertyOffsets)
                {
                    streamReader.BaseStream.Position = offset + propertyOffset.Item2;

                    var type = GetPropertyType(propertyOffset.Item1);

                    var valueType = (PropertyValueType) streamReader.ReadUInt32();

                    if (type == PropertyType.String)
                    {
                        var length = streamReader.ReadInt32();
                        var bytes = streamReader.ReadBytes(length);

                        var value = Encoding.ASCII.GetString(bytes, 0, length);
                    }
                    else if (type == PropertyType.Short)
                    {
                        var value = streamReader.ReadUInt32();
                    }
                    else if (type == PropertyType.Long)
                    {
                        var value = streamReader.ReadUInt32();
                    }
                    else if (type == PropertyType.FileTime)
                    {
                        var value = streamReader.ReadUInt64();
                    }
                }
            }
        }

        private static IEnumerable<DirectoryEntry> ReadDirectoryEntries(BinaryReader reader, uint firstSector)
        {
            var chain = GetFatSectorChain(firstSector);

            foreach (var sector in chain)
            {
                SetPosition(reader, sector);

                for (var i = 0; i < sectorLength / 128; i++)
                {
                    var name = reader.ReadBytes(64);
                    var nameLength = reader.ReadUInt16();
                    var objectType = reader.ReadByte();
                    var colorFlag = reader.ReadByte();
                    var leftSibling = reader.ReadUInt32();
                    var rightSibling = reader.ReadUInt32();
                    var child = reader.ReadUInt32();
                    var clsid = reader.ReadBytes(16);
                    var state = reader.ReadUInt32();
                    var creationTime = reader.ReadUInt64();
                    var modifiedTime = reader.ReadUInt64();
                    var startingSector = reader.ReadUInt32();
                    var streamLength = reader.ReadUInt64();

                    if (nameLength > 0)
                        yield return new DirectoryEntry(DecodeName(name), startingSector, streamLength);

                    //using (var stream = new MemoryStream(name))
                    //using (var nameReader = new BinaryReader(stream))
                    //{
                    //    var nameArray = nameReader.ReadUint16Array(32);
                    //}
                }
            }
        }

        private static IEnumerable<StringValue> ReadStrings(BinaryReader reader)
        {
            var pool = ReadStream(reader, "_StringPool");
            var data = ReadStream(reader, "_StringData");

            using (var poolStream = new MemoryStream(pool))
            using (var dataStream = new MemoryStream(data))
            using (var poolReader = new BinaryReader(poolStream))
            using (var dataReader = new BinaryReader(dataStream))
            {
                var header1 = poolReader.ReadUInt16();
                var header2 = poolReader.ReadUInt16();

                bytesPerString = (header2 & 0x8000) != 0 ? LongStringLength : 2;
                var codePage = header1 | (header2 & ~0x8000) << 16;

                var encoding = Encoding.GetEncoding(codePage);

                for (var i = 1; i < pool.Length / 4; i++)
                {
                    int length = poolReader.ReadUInt16();
                    int refs = poolReader.ReadUInt16();

                    if (length == 0 && refs == 0)
                        continue;

                    if (length == 0)
                        length = (refs << 16) | length;

                    var value = encoding.GetString(dataReader.ReadBytes(length));

                    yield return new StringValue(i, value);
                }
            }
        }

        private static IEnumerable<Row> ReadTables(BinaryReader reader)
        {
            var columns = new[]
            {
                new ColumnInfo(1, "Name", 0)
            };

            return ReadTable(reader, columns, "_Tables");
        }

        private static IEnumerable<Row> ReadColumns(BinaryReader reader)
        {
            var columns = new[]
            {
                new ColumnInfo(1, "Table", 0),
                new ColumnInfo(2, "Number", 1),
                new ColumnInfo(3, "Name", 0),
                new ColumnInfo(4, "Type", 1)
            };

            return ReadTable(reader, columns, "_Columns");
        }

        private static IEnumerable<Row> ReadTable(BinaryReader reader, ColumnInfo[] columns, string name)
        {
            var data = ReadStream(reader, name);

            var rowLength = GetRowLength(columns);
            var rowCount = data.Length / rowLength;

            using (var dataStream = new MemoryStream(data))
            using (var dataReader = new BinaryReader(dataStream))
            {
                var rows = new object[rowCount][];

                for (var i = 0; i < columns.Length; i++)
                {
                    var column = columns[i];

                    for (var j = 0; j < rowCount; j++)
                    {
                        if (rows[j] == null)
                            rows[j] = new object[columns.Length];

                        if (column.Type == 0)
                        {
                            var id = bytesPerString == 2
                                ? dataReader.ReadUInt16()
                                : dataReader.ReadUint24();

                            rows[j][i] = strings.FirstOrDefault(x => x.Id == id)?.Value;
                        }
                        else if (column.Type == 1)
                        {
                            rows[j][i] = dataReader.ReadUInt16();
                        }
                    }
                }

                foreach (var row in rows)
                    yield return new Row(row);
            }
        }

        private static int GetRowLength(ColumnInfo[] columns)
        {
            var length = 0;

            foreach (var column in columns)
            {
                if (column.Type == 0)
                    length += bytesPerString;
                else if (column.Type == 1)
                    length += 2;
            }

            return length;
        }

        private static byte[] ReadStream(BinaryReader reader, string name)
        {
            var entry = directoryEntries.First(x => x.Name == name);

            if (entry.Length < sectorCutoff)
                return ReadMiniStream(entry.Sector, entry.Length);

            return ReadStream(reader, entry.Sector, entry.Length);
        }

        private static byte[] ReadMiniStream(uint firstSector, ulong length)
        {
            var chain = GetMiniFatSectorChain(firstSector);

            using (var stream = new MemoryStream())
            using (var mini = new MemoryStream(miniStream))
            using (var miniReader = new BinaryReader(mini))
            {
                foreach (var sector in chain)
                {
                    miniReader.BaseStream.Position = sector * 64;

                    var buffer = miniReader.ReadBytes(64);

                    stream.Write(buffer, 0, buffer.Length);
                }

                var data = new byte[length];

                Array.Copy(stream.ToArray(), data, (int) length);

                return data;
            }
        }

        private static byte[] ReadStream(BinaryReader reader, uint firstSector, ulong length)
        {
            var chain = GetFatSectorChain(firstSector);

            using (var stream = new MemoryStream())
            {
                foreach (var sector in chain)
                {
                    SetPosition(reader, sector);

                    var buffer = reader.ReadBytes(sectorLength);

                    stream.Write(buffer, 0, buffer.Length);
                }

                var data = new byte[length];

                Array.Copy(stream.ToArray(), data, (int) length);

                return data;
            }
        }

        private static PropertyType GetPropertyType(uint type)
        {
            switch (type)
            {
                case CodePage:
                    return PropertyType.Short;

                case Subject:
                case Author:
                case Keywords:
                case Comments:
                case Template:
                case LastAuthor:
                case RevisionNumber:
                case ApplicationName:
                case Title:
                    return PropertyType.String;

                case LastPrinted:
                case CreateDateTime:
                case LastSaveDateTime:
                    return PropertyType.FileTime;

                case WordCount:
                case CharCount:
                case Security:
                case PageCount:
                    return PropertyType.Long;
            }

            throw new FormatException();
        }

        private static string DecodeName(byte[] bytes)
        {
            const string base64Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz._";

            var chars = new List<char>();

            for (var i = 0; i < bytes.Length; i += 2)
            {
                var value = BitConverter.ToUInt16(bytes, i);

                if (value >= 0x3800 && value < 0x4800)
                {
                    var c = value - 0x3800;

                    chars.Add(base64Chars[c & 0x3f]);
                    chars.Add(base64Chars[(c >> 6) & 0x3f]);
                }
                else if (value >= 0x4800 && value < 0x4840)
                {
                    chars.Add(base64Chars[value - 0x4800]);
                }
                else if (value != 0x4840)
                {
                    chars.Add((char)value);
                }
            }

            return new string(chars.ToArray()).TrimEnd('\0');
        }

        private static string DecodeName(ushort[] chars)
        {
            int GetUtf(int x)
            {
                if (x < 10)
                    return x + '0';

                if (x < 10 + 26)
                    return x - 10 + 'A';

                if (x < 10 + 26 + 26)
                    return x - 10 - 26 + 'a';

                if (x == 10 + 26 + 26)
                    return '.';

                return '_';
            }

            var value = new StringBuilder();

            foreach (var c in chars)
            {
                if (c >= 0x3800 && c < 0x4840)
                {
                    if (c >= 0x4800)
                    {
                        var x = GetUtf(c - 0x4800);

                        value.Append(char.ConvertFromUtf32(x));
                    }
                    else
                    {
                        var x = c - 0x3800;
                        var y = GetUtf(x & 0x3f);
                        var z = GetUtf((x >> 6) & 0x3f);

                        value.Append(char.ConvertFromUtf32(y));
                        value.Append(char.ConvertFromUtf32(z));
                    }
                }
                else if (c != 0x4840)
                {
                    value.Append(char.ConvertFromUtf32(c));
                }
            }

            return value.ToString().TrimEnd('\0');
        }

        private static IEnumerable<uint> GetFatSectorChain(uint firstSector)
        {
            return GetSectorChain(fat, firstSector);
        }

        private static IEnumerable<uint> GetMiniFatSectorChain(uint firstSector)
        {
            return GetSectorChain(miniFat, firstSector);
        }

        private static IEnumerable<uint> GetSectorChain(uint[] table, uint firstSector)
        {
            var sector = firstSector;

            while (sector != EndOfChain && sector != FreeSector)
            {
                yield return sector;

                sector = table[sector];
            }
        }

        private static void SetPosition(BinaryReader reader, uint sector)
        {
            reader.BaseStream.Position = (sector + 1) * sectorLength;
        }
    }
}
