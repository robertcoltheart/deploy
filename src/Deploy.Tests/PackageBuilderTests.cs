using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Deploy
{
    public class PackageBuilderTests : IDisposable
    {
        private string _filename;

        public PackageBuilderTests()
        {
            _filename = Path.GetTempFileName();
        }

        public void Dispose()
        {
            File.Delete(_filename);
        }

        [Fact]
        public void NoProductNameThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void InvalidProductNameThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("\\Name*?")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void NullProductNameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.ProductName(null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void EmptyProductNameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.ProductName(string.Empty));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void NullProductIconThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.ProductIcon(null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void EmptyProductIconThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.ProductIcon(string.Empty));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void NoVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid());

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void NoAuthorSucceeds()
        {
            new PackageBuilder()
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [Fact]
        public void NoPlatformUsesDefault()
        {
            new PackageBuilder()
                .Author("author")
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [Fact]
        public void NoUpgradeCodeThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void EmptyUpgradeCodeThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.UpgradeCode(Guid.Empty));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void InvalidTwoPartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void InvalidThreePartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void InvalidFourPartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0, 0));

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void NullVersionThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.Version(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void MissingFileThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File("missing.file");

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void TooManyFilesThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid());

            for (int i = 0; i < 60000; i++)
                builder.File("file" + i);

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void NullFilenameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.File(null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void EmptyFilenameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.File(string.Empty));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void MissingShortcutIconThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, "missing.ico");

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void MissingShortcutNameThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, file + ".ico");

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void InvalidShortcutNameThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, file + ".ico", "name*with\\invalid?path");

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void InvalidIconExtensionThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, "missing.none");

            var exception = Record.Exception(() => builder.Build(_filename));

            Assert.IsType<PackageException>(exception);
        }

        [Fact]
        public void BuildWithNullFilenameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.Build(null));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void BuildWithEmptyFilenameThrows()
        {
            var builder = new PackageBuilder();

            var exception = Record.Exception(() => builder.Build(string.Empty));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void PackageBuilds()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X64)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .File(file)
                .Build(_filename);

            var info = new FileInfo(_filename);

            Assert.True(info.Exists, "Package was not built");
            Assert.True(info.Length > 0, "Package was empty");
        }
    }
}
