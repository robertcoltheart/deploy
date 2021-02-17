using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Deploy.Data;

namespace Deploy.IO
{
    internal class PackageWriter : IDisposable
    {
        private const string CabinetName = "#product.cab";

        private const string ProgramMenuFolder = "ProgramMenuFolder";

        private const string DefaultFeature = "Feature";

        private const string InstallFolder = "InstallFolder";

        private readonly IPackage package;

        private readonly Dictionary<string, int> ids = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        private readonly MsiConnection connection;

        public PackageWriter(IPackage package, string filename)
        {
            this.package = package;

            connection = new MsiConnection(filename);
        }

        private void BuildIds()
        {
            var sequence = 1;
            
            foreach (var file in package.Files)
            {
                ids[file.Filename.Trim()] = sequence++;

                if (!string.IsNullOrEmpty(file.ShortcutIcon))
                {
                    ids[file.ShortcutIcon.Trim()] = sequence++;
                }
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
        
        public void Write()
        {
            var productCode = Guid.NewGuid();

            BuildIds();

            WriteTables();
            WriteValidation();
            WriteSequencing();
            WriteUpgrade();
            WriteProperties(productCode);
            WriteDirectories();
            WriteComponents();
            WriteFiles();
            WriteShortcuts();
            WriteIcons();

            connection.Commit(package.ProductName, package.Author, package.Platform, CultureInfo.CurrentCulture.LCID, productCode);
        }

        private void Write(string table, params object[] parameters)
        {
            connection.Execute(DefaultData.Tables.First(x => x.Name == table).Insert(parameters));
        }

        private void WriteDirectories()
        {
            var programFiles = package.Platform == PackagePlatform.X64 ? "ProgramFiles64Folder" : "ProgramFilesFolder";

            Write("Directory", InstallFolder, programFiles, package.ProductName);
            Write("Directory", programFiles, "TARGETDIR", ".");
            Write("Directory", "TARGETDIR", null, "SourceDir");
        }

        private void WriteComponents()
        {
            Write("Feature", DefaultFeature, null, null, null, 1, 1, null, 0);

            var attributes = package.Platform == PackagePlatform.X64 ? 256 : 0;

            foreach (var file in package.Files)
            {
                var id = ids[file.Filename].ToString();

                Write("Component", id, Guid.NewGuid().ToStringFormatted(), InstallFolder, attributes, null, id);
                Write("FeatureComponents", DefaultFeature, id);
            }
        }

        private void WriteIcons()
        {
            if (!string.IsNullOrEmpty(package.ProductIcon))
            {
                Write("Property", "ARPPRODUCTICON", "ProductIcon");
                connection.ExecuteStream("Icon", package.ProductIcon, "ProductIcon");
            }

            var shortcuts = package.Files.Select(x => x.ShortcutIcon)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct();

            foreach (var shortcut in shortcuts)
            {
                connection.ExecuteStream("Icon", shortcut, ids[shortcut] + ".ico");
            }
        }

        private void WriteFiles()
        {
            var binaries = package.Files.Select(x => x.ShortcutIcon)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct()
                .Concat(package.Files.Select(x => x.Filename.Trim()))
                .ToArray();
            
            Write("Media", 1, binaries.Length + 1, null, CabinetName, null, null);

            foreach (var file in package.Files)
            {
                var info = new FileInfo(file.Filename);
                var id = ids[file.Filename];

                Write("File", id.ToString(), id.ToString(), $"{id}|{info.Name}", (int)info.Length, null, null, 512, id);
            }

            var cabinet = Path.GetTempFileName();

            using (var stream = File.OpenWrite(cabinet))
            {
                var writer = new CabinetWriter(binaries, x => ids[x].ToString());
                writer.Write(stream);
            }

            connection.ExecuteStream("_Streams", cabinet, CabinetName.Substring(1));

            File.Delete(cabinet);
        }

        private void WriteProperties(Guid productCode)
        {
            Write("Property", "ALLUSERS", 1);
            Write("Property", "ARPNOMODIFY", 1);
            Write("Property", "ARPNOREPAIR", 1);
            Write("Property", "Manufacturer", package.Author);
            Write("Property", "ProductLanguage", CultureInfo.CurrentCulture.LCID);
            Write("Property", "ProductName", package.ProductName);
            Write("Property", "ProductCode", productCode.ToStringFormatted());
            Write("Property", "ProductVersion", package.Version.ToString());
            Write("Property", "SecureCustomProperties", "NEWERVERSIONDETECTED;OLDERVERSIONBEINGUPGRADED");
            Write("Property", "UpgradeCode", package.UpgradeCode.ToStringFormatted());
        }

        private void WriteSequencing()
        {
            foreach (var key in DefaultData.Sequences.Keys)
            {
                var sequence = 1;

                foreach (var value in DefaultData.Sequences[key])
                    Write(key, value, null, sequence++);
            }
        }
        
        private void WriteShortcuts()
        {
            Write("Directory", ProgramMenuFolder, "TARGETDIR", ".");
            Write("Directory", "ProductFolder", ProgramMenuFolder, package.ProductName);

            foreach (var file in package.Files)
            {
                if (!string.IsNullOrEmpty(file.ShortcutIcon))
                {
                    var id = ids[file.ShortcutIcon].ToString();
                    var fileId = ids[file.Filename].ToString();

                    Write("Shortcut", id, "ProductFolder", file.ShortcutName, fileId, DefaultFeature, null, file.ShortcutName, null, id + ".ico", null, null, InstallFolder, null, null, null, null);
                }
            }
        }

        private void WriteTables()
        {
            foreach (var table in DefaultData.Tables)
            {
                connection.Execute(table.Create());
            }
        }

        private void WriteUpgrade()
        {
            Write("Upgrade", package.UpgradeCode.ToStringFormatted(), package.Version, null, null, 258, null, "NEWERVERSIONDETECTED");
            Write("Upgrade", package.UpgradeCode.ToStringFormatted(), "0.0.0", package.Version, null, 256, null, "OLDERVERSIONBEINGUPGRADED");
            Write("LaunchCondition", "NOT NEWERVERSIONDETECTED", "A newer version of this software is already installed.");
        }

        private void WriteValidation()
        {
            foreach (var table in DefaultData.Tables)
            {
                foreach (var column in table.Columns)
                {
                    var category = column.Type == DataType.Object ? "Binary" : null;
                    var nullable = column.Constraint == Constraint.Null ? "Y" : "N";

                    Write("_Validation", table.Name, column.Name, nullable, null, null, null, null, category, null, null);
                }
            }
        }
    }
}
