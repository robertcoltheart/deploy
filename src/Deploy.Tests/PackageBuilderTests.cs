using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Deploy
{
    [TestFixture]
    public class PackageBuilderTests
    {
        private string _filename;

        [SetUp]
        public void Setup()
        {
            _filename = Path.GetTempFileName();
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(_filename);
        }

        [Test]
        public void NoProductNameThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void InvalidProductNameThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("\\Name*?")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void NullProductNameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.ProductName(null), Throws.ArgumentException);
        }

        [Test]
        public void EmptyProductNameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.ProductName(string.Empty), Throws.ArgumentException);
        }

        [Test]
        public void NullProductIconThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.ProductIcon(null), Throws.ArgumentException);
        }

        [Test]
        public void EmptyProductIconThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.ProductIcon(string.Empty), Throws.ArgumentException);
        }

        [Test]
        public void NoVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid());

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void NoAuthorSucceeds()
        {
            new PackageBuilder()
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [Test]
        public void NoPlatformUsesDefault()
        {
            new PackageBuilder()
                .Author("author")
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [Test]
        public void NoUpgradeCodeThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void EmptyUpgradeCodeThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.UpgradeCode(Guid.Empty), Throws.ArgumentException);
        }

        [Test]
        public void InvalidTwoPartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void InvalidThreePartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void InvalidFourPartVersionThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0, 0));

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void NullVersionThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.Version(null), Throws.ArgumentNullException);
        }

        [Test]
        public void MissingFileThrows()
        {
            var builder = new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File("missing.file");

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
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

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void NullFilenameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.File(null), Throws.ArgumentException);
        }

        [Test]
        public void EmptyFilenameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.File(string.Empty), Throws.ArgumentException);
        }

        [Test]
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

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
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

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
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

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
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

            Assert.That(() => builder.Build(_filename), Throws.InstanceOf<PackageException>());
        }

        [Test]
        public void BuildWithNullFilenameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.Build(null), Throws.ArgumentException);
        }

        [Test]
        public void BuildWithEmptyFilenameThrows()
        {
            var builder = new PackageBuilder();

            Assert.That(() => builder.Build(string.Empty), Throws.ArgumentException);
        }

        [Test]
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

            Assert.IsTrue(info.Exists, "Package was not built");
            Assert.IsTrue(info.Length > 0, "Package was empty");
        }
    }
}
