using System.IO;

namespace Deploy
{
    public class Package
    {
        private readonly Header header;

        public Package()
            : this(PackageVersion.Version_3)
        {
        }

        public Package(PackageVersion version)
        {
            header = version == PackageVersion.Version_3
                ? new Header(3)
                : new Header(4);
        }

        public Package(string fileName)
            : this(new FileStream(fileName, FileMode.Open))
        {
        }

        public Package(Stream stream)
        {
            header.Read(stream);
        }
    }
}
