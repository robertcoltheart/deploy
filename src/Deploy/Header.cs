using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Deploy
{
    internal class Header
    {
        private static readonly byte[] Signature =
        {
            0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1
        };

        public Header(ushort version)
        {
            Version = version;
        }

        public ushort Version { get; private set; }

        public void Read(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            var signature = reader.ReadBytes(8);

            if (!signature.SequenceEqual(Signature))
            {
                throw new PackageException();
            }

            var classId = new Guid(reader.ReadBytes(16));

            if (classId != Guid.Empty)
            {
                throw new PackageException();
            }

            var minorVersion = reader.ReadUInt16();
            var majorVersion = reader.ReadUInt16();

            if (majorVersion != 3 && majorVersion != 4)
            {
                throw new PackageException();
            }

            var byteOrder = reader.ReadUInt16();

            if (byteOrder != 0xfffe)
            {
                throw new PackageException();
            }

            var sectorShift = reader.ReadUInt16();

            if (majorVersion == 3 && sectorShift != 0x9)
            {
                throw new PackageException();
            }

            if (majorVersion == 4 && sectorShift != 0xc)
            {
                throw new PackageException();
            }

            var miniSectorShift = reader.ReadUInt16();

            if (miniSectorShift != 0x6)
            {
                throw new PackageException();
            }

            var reserved = reader.ReadBytes(6);

            if (reserved.Any(x => x != 0))
            {
                throw new PackageException();
            }

            var directorySectorsCount = reader.ReadUInt32();
            var fatSectorsCount = reader.ReadUInt32();
            var firstDirectorySector = reader.ReadUInt32();
            var transactionSignature = reader.ReadUInt32();
            var miniStreamCutoffLength = reader.ReadUInt32();

            if (miniStreamCutoffLength != 0x1000)
            {
                throw new PackageException();
            }

            var firstMiniFatSector = reader.ReadUInt32();
            var miniFatSectorsCount = reader.ReadUInt32();
            var firstDifatSector = reader.ReadUInt32();
            var difatSectorsCount = reader.ReadUInt32();
            var initialDifat = reader.ReadUint32Array(109);

            if (majorVersion == 4)
            {
                var remaining = reader.ReadBytes(3584);

                if (remaining.Any(x => x != 0))
                {
                    throw new PackageException();
                }
            }

            Version = majorVersion;
        }
    }
}
