using System;
using System.Collections.Generic;
using Deploy.IO;
using Deploy.Properties;

namespace Deploy
{
    /// <summary>
    /// Builds an MSI installer.
    /// </summary>
    public class PackageBuilder : IPackage
    {
        private readonly List<PackageFile> files = new List<PackageFile>();

        private string packageAuthor;

        private PackagePlatform packagePlatform;

        private string packageProductIcon;

        private string packageProductName;

        private Guid packageUpgradeCode;

        private Version packageVersion;

        string IPackage.ProductName => packageProductName;

        string IPackage.ProductIcon => packageProductIcon;

        string IPackage.Author => packageAuthor;

        PackagePlatform IPackage.Platform => packagePlatform;

        Guid IPackage.UpgradeCode => packageUpgradeCode;

        Version IPackage.Version => packageVersion;

        IEnumerable<PackageFile> IPackage.Files => files;

        /// <summary>
        /// Sets the name of the product.
        /// </summary>
        /// <remarks>
        /// The product name is used as the installation package title, and appears in the Windows Programs list. It is also
        /// used as the directory name in the Program Files directory, and as such, must not contain any illegal characters.
        /// </remarks>
        /// <param name="productName">The name of the product.</param>
        /// <exception cref="ArgumentException">The <paramref name="productName"/> parameter is null or empty.</exception>
        /// <returns>The package builder.</returns>
        public PackageBuilder ProductName(string productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(productName));
            }

            packageProductName = productName;

            return this;
        }

        /// <summary>
        /// Sets the product icon filename.
        /// </summary>
        /// <remarks>
        /// The product icon is used in the Windows Programs list.
        /// </remarks>
        /// <param name="productIcon">The product icon filename.</param>
        /// <exception cref="ArgumentException">The <paramref name="productIcon"/> parameter is null or empty.</exception>
        /// <returns>The package builder.</returns>
        public PackageBuilder ProductIcon(string productIcon)
        {
            if (string.IsNullOrEmpty(productIcon))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(productIcon));
            }

            packageProductIcon = productIcon;

            return this;
        }

        /// <summary>
        /// Sets the package author.
        /// </summary>
        /// <param name="author">The package author</param>
        /// <returns>The package builder.</returns>
        public PackageBuilder Author(string author)
        {
            packageAuthor = author;

            return this;
        }

        /// <summary>
        /// Sets the target platform for the package.
        /// </summary>
        /// <remarks>
        /// MSI installation packages can be targeted at either 32 or 64-bit platforms. Based on this setting, the package
        /// will install to either Program Files (x86) or Program Files on a 64-bit system.
        /// </remarks>
        /// <param name="platform">The target platform for the package.</param>
        /// <returns>The package builder.</returns>
        public PackageBuilder Platform(PackagePlatform platform)
        {
            packagePlatform = platform;

            return this;
        }

        /// <summary>
        /// Sets the upgrade code for the package.
        /// </summary>
        /// <remarks>
        /// The upgrade code must remain consistent for the same application. It is used by Windows to upgrade older versions
        /// of the same application. If the upgrade code changes, Windows assumes that a new and different application is being
        /// installed.
        /// </remarks>
        /// <param name="upgradeCode">The upgrade code for the package.</param>
        /// <exception cref="ArgumentException">The <paramref name="upgradeCode"/> parameter is an empty <see cref="Guid"/>.</exception>
        /// <returns>The package builder.</returns>
        public PackageBuilder UpgradeCode(Guid upgradeCode)
        {
            if (upgradeCode == Guid.Empty)
            {
                throw new ArgumentException(Resources.Argument_EmptyGuid, nameof(upgradeCode));
            }

            packageUpgradeCode = upgradeCode;

            return this;
        }

        /// <summary>
        /// Sets the package version.
        /// </summary>
        /// <remarks>
        /// The version is used by Windows to track upgrades of the same application. Newer versions will replace older
        /// versions,
        /// but an older version cannot replace a newer version.
        /// </remarks>
        /// <param name="version">The package version.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> parameter is null.</exception>
        /// <returns>The package builder.</returns>
        public PackageBuilder Version(Version version)
        {
            packageVersion = version ?? throw new ArgumentNullException(nameof(version));

            return this;
        }

        /// <summary>
        /// Adds a file to the package to be installed.
        /// </summary>
        /// <remarks>
        /// When a shortcut icon is specified, an icon is placed in the Start Menu. The icon appears at the root level of the
        /// Start Menu with the icon name specified.
        /// </remarks>
        /// <param name="filename">The filename of the file to be added.</param>
        /// <param name="shortcutIcon">An optional parameter specifying the path to the icon to be used for the Start Menu.</param>
        /// <param name="shortcutName">An optional parameter specifying the name of the icon to display in the Start Menu.</param>
        /// <exception cref="ArgumentException">The <paramref name="filename"/> parameter is null or empty.</exception>
        /// <returns>The package builder.</returns>
        public PackageBuilder File(string filename, string shortcutIcon = null, string shortcutName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(filename));
            }

            files.Add(new PackageFile(filename, shortcutIcon, shortcutName));

            return this;
        }

        /// <summary>
        /// Builds the MSI package to the specified filename.
        /// </summary>
        /// <param name="filename">The filename of the MSI package.</param>
        /// <exception cref="ArgumentException">The <paramref name="filename"/> parameter is null or empty.</exception>
        /// <exception cref="PackageException">A validation error or other platform error occurred</exception>
        public void Build(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(filename));
            }

            var validator = new PackageValidator(this);
            validator.Validate();

            try
            {
                using (var writer = new PackageWriter(this, filename))
                {
                    writer.Write();
                }
            }
            catch (Exception ex)
            {
                throw new PackageException(Resources.Package_PlatformError, ex);
            }
        }
    }
}
