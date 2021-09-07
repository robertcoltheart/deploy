using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deploy.Serialization
{
    public class SummaryInformationSerializer
    {
        private const ushort ByteOrder = 0xfffe;
        private const ushort Format = 0;
        private const int OsVersion = 0x2000a;
        private const int Section = 0xf0;
        private const int PropertiesCount = 8;

        private readonly Guid _formatId = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9");

        public byte[] Serialize(SummaryInformation summary)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(ByteOrder);
                writer.Write(Format);
                writer.Write(OsVersion);
                writer.Write(Guid.Empty);
                writer.Write(1);
                writer.Write(_formatId);
                writer.Write(Convert.ToInt32(stream.Position));
                writer.Write(Section);
                writer.Write(PropertiesCount);

                for (var i = 0; i < PropertiesCount; i++)
                {
                    writer.Write(0);
                    writer.Write(0);
                }

                // Title
                writer.Write(SummaryInformationType.String);

                return stream.ToArray();
            }
        }

        private IEnumerable<Tuple<string, SummaryInformationType>> CollectProperties(SummaryInformation summary)
        {
            yield return Tuple.Create(summary.PageCount.ToString(), SummaryInformationType.Long);

        }
    }
}
