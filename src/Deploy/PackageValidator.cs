using System;
using System.IO;
using System.Linq;
using Deploy.Properties;

namespace Deploy
{
    internal class PackageValidator
    {
        private readonly IPackage _package;
        
        public PackageValidator(IPackage package)
        {
            _package = package;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(_package.ProductName))
                throw new PackageException(Resources.Package_NoProductName);

            if (_package.ProductName.Any(x => Path.GetInvalidFileNameChars().Contains(x)))
                throw new PackageException(Resources.Package_InvalidProductName);

            if (_package.Version == null)
                throw new PackageException(Resources.Package_NoProductVersion);

            if (_package.Version.Major < 0 || _package.Version.Major > 255)
                throw new PackageException(Resources.Package_InvalidVersion);

            if (_package.Version.Minor < 0 || _package.Version.Minor > 255)
                throw new PackageException(Resources.Package_InvalidVersion);

            if (_package.Version.Build < 0 || _package.Version.Build > 65535)
                throw new PackageException(Resources.Package_InvalidVersion);

            if (_package.Version.Revision != -1)
                throw new PackageException(Resources.Package_InvalidVersion);

            if (_package.Version == Version.Parse("0.0.0"))
                throw new PackageException(Resources.Package_ProductVersionZero);

            if (_package.UpgradeCode == Guid.Empty)
                throw new PackageException(Resources.Package_NoUpgradeCode);

            if (_package.Files.Count() > 32768)
                throw new PackageException(Resources.Package_OverFileLimit);

            foreach (PackageFile file in _package.Files)
            {
                if (!File.Exists(file.Filename))
                    throw new PackageException($"{Resources.Package_MissingFile}: {file.Filename}");

                if (!string.IsNullOrEmpty(file.ShortcutIcon))
                {
                    if (Path.GetExtension(file.ShortcutIcon).ToLower() != ".ico")
                        throw new PackageException($"{Resources.Package_InvalidIcon}: {file.ShortcutIcon}");

                    if (!File.Exists(file.ShortcutIcon))
                        throw new PackageException($"{Resources.Package_MissingFile}: {file.ShortcutIcon}");

                    if (string.IsNullOrEmpty(file.ShortcutName))
                        throw new PackageException($"{Resources.Package_NoShortcutName}: {file.ShortcutIcon}");

                    if (file.ShortcutName.Any(x => Path.GetInvalidFileNameChars().Contains(x)))
                        throw new PackageException("Shortcut name cannot contain invalid characters or path separators");
                }
            }
        }
    }
}