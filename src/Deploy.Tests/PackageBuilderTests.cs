using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Deploy
{
    [TestClass]
    public class PackageBuilderTests
    {
        private string _filename;

        [TestInitialize]
        public void Setup()
        {
            _filename = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void NoProductNameThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidProductNameThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("\\Name*?")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NullProductNameThrows()
        {
            new PackageBuilder()
                .ProductName(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyProductNameThrows()
        {
            new PackageBuilder()
                .ProductName(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NullProductIconThrows()
        {
            new PackageBuilder()
                .ProductIcon(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyProductIconThrows()
        {
            new PackageBuilder()
                .ProductIcon(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void NoVersionThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Build(_filename);
        }

        [TestMethod]
        public void NoAuthorSucceeds()
        {
            new PackageBuilder()
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [TestMethod]
        public void NoPlatformUsesDefault()
        {
            new PackageBuilder()
                .Author("author")
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void NoUpgradeCodeThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyUpgradeCodeThrows()
        {
            new PackageBuilder()
                .UpgradeCode(Guid.Empty);
        }
        
        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidTwoPartVersionThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidThreePartVersionThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidFourPartVersionThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .UpgradeCode(Guid.NewGuid())
                .Version(new Version(0, 0, 0, 0))
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullVersionThrows()
        {
            new PackageBuilder()
                .Version(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void MissingFileThrows()
        {
            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File("missing.file")
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
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

            builder.Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NullFilenameThrows()
        {
            new PackageBuilder()
                .File(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyFilenameThrows()
        {
            new PackageBuilder()
                .File(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void MissingShortcutIconThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, "missing.ico")
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void MissingShortcutNameThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, file + ".ico")
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidShortcutNameThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, file + ".ico", "name*with\\invalid?path")
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(PackageException))]
        public void InvalidIconExtensionThrows()
        {
            string file = Directory.GetFiles(Environment.CurrentDirectory).First();

            new PackageBuilder()
                .Author("author")
                .Platform(PackagePlatform.X86)
                .ProductName("product")
                .Version(new Version(1, 2, 3))
                .UpgradeCode(Guid.NewGuid())
                .File(file, "missing.none")
                .Build(_filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildWithNullFilenameThrows()
        {
            new PackageBuilder()
                .Build(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildWithEmptyFilenameThrows()
        {
            new PackageBuilder()
                .Build(string.Empty);
        }

        [TestMethod]
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