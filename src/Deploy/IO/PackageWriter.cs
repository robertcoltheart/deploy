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

        private readonly IPackage _package;
        private readonly Dictionary<string, int> _ids = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        private readonly MsiConnection _connection;

        public PackageWriter(IPackage package, string filename)
        {
            _package = package;

            _connection = new MsiConnection(filename);
        }

        private void BuildIds()
        {
            int sequence = 1;
            
            foreach (PackageFile file in _package.Files)
            {
                _ids[file.Filename.Trim()] = sequence++;

                if (!string.IsNullOrEmpty(file.ShortcutIcon))
                    _ids[file.ShortcutIcon.Trim()] = sequence++;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
        
        public void Write()
        {
            Guid productCode = Guid.NewGuid();

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

            _connection.Commit(_package.ProductName, _package.Author, _package.Platform, CultureInfo.CurrentCulture.LCID, productCode);
        }

        private void Write(string table, params object[] parameters)
        {
            _connection.Execute(DefaultData.Tables.First(x => x.Name == table).Insert(parameters));
        }

        private void WriteDirectories()
        {
            string programFiles = _package.Platform == PackagePlatform.X64 ? "ProgramFiles64Folder" : "ProgramFilesFolder";

            Write("Directory", InstallFolder, programFiles, _package.ProductName);
            Write("Directory", programFiles, "TARGETDIR", ".");
            Write("Directory", "TARGETDIR", null, "SourceDir");
        }

        private void WriteComponents()
        {
            Write("Feature", DefaultFeature, null, null, null, 1, 1, null, 0);

            int attributes = _package.Platform == PackagePlatform.X64 ? 256 : 0;

            foreach (PackageFile file in _package.Files)
            {
                string id = _ids[file.Filename].ToString();

                Write("Component", id, Guid.NewGuid().ToStringFormatted(), InstallFolder, attributes, null, id);
                Write("FeatureComponents", DefaultFeature, id);
            }
        }

        private void WriteIcons()
        {
            if (!string.IsNullOrEmpty(_package.ProductIcon))
            {
                Write("Property", "ARPPRODUCTICON", "ProductIcon");
                _connection.ExecuteStream("Icon", _package.ProductIcon, "ProductIcon");
            }

            var shortcuts = _package.Files.Select(x => x.ShortcutIcon)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct();

            foreach (string shortcut in shortcuts)
                _connection.ExecuteStream("Icon", shortcut, _ids[shortcut] + ".ico");
        }

        private void WriteFiles()
        {
            var binaries = _package.Files.Select(x => x.ShortcutIcon)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct()
                .Concat(_package.Files.Select(x => x.Filename.Trim()))
                .ToArray();
            
            Write("Media", 1, binaries.Length + 1, null, CabinetName, null, null);

            foreach (PackageFile file in _package.Files)
            {
                var info = new FileInfo(file.Filename);
                int id = _ids[file.Filename];

                Write("File", id.ToString(), id.ToString(), $"{id}|{info.Name}", (int)info.Length, null, null, 512, id);
            }

            string cabinet = Path.GetTempFileName();

            using (var stream = File.OpenWrite(cabinet))
            {
                var writer = new CabinetWriter(binaries, x => _ids[x].ToString());
                writer.Write(stream);
            }

            _connection.ExecuteStream("_Streams", cabinet, CabinetName.Substring(1));

            File.Delete(cabinet);
        }

        private void WriteProperties(Guid productCode)
        {
            Write("Property", "ALLUSERS", 1);
            Write("Property", "ARPNOMODIFY", 1);
            Write("Property", "ARPNOREPAIR", 1);
            Write("Property", "Manufacturer", _package.Author);
            Write("Property", "ProductLanguage", CultureInfo.CurrentCulture.LCID);
            Write("Property", "ProductName", _package.ProductName);
            Write("Property", "ProductCode", productCode.ToStringFormatted());
            Write("Property", "ProductVersion", _package.Version.ToString());
            Write("Property", "SecureCustomProperties", "NEWERVERSIONDETECTED;OLDERVERSIONBEINGUPGRADED");
            Write("Property", "UpgradeCode", _package.UpgradeCode.ToStringFormatted());
        }

        private void WriteSequencing()
        {
            foreach (string key in DefaultData.Sequences.Keys)
            {
                int sequence = 1;

                foreach (string value in DefaultData.Sequences[key])
                    Write(key, value, null, sequence++);
            }
        }
        
        private void WriteShortcuts()
        {
            Write("Directory", ProgramMenuFolder, "TARGETDIR", ".");
            Write("Directory", "ProductFolder", ProgramMenuFolder, _package.ProductName);

            foreach (PackageFile file in _package.Files)
            {
                if (!string.IsNullOrEmpty(file.ShortcutIcon))
                {
                    string id = _ids[file.ShortcutIcon].ToString();
                    string fileId = _ids[file.Filename].ToString();

                    Write("Shortcut", id, "ProductFolder", file.ShortcutName, fileId, DefaultFeature, null, file.ShortcutName, null, id + ".ico", null, null, InstallFolder, null, null, null, null);
                }
            }
        }

        private void WriteTables()
        {
            foreach (Table table in DefaultData.Tables)
                _connection.Execute(table.Create());
        }

        private void WriteUpgrade()
        {
            Write("Upgrade", _package.UpgradeCode.ToStringFormatted(), _package.Version, null, null, 258, null, "NEWERVERSIONDETECTED");
            Write("Upgrade", _package.UpgradeCode.ToStringFormatted(), "0.0.0", _package.Version, null, 256, null, "OLDERVERSIONBEINGUPGRADED");
            Write("LaunchCondition", "NOT NEWERVERSIONDETECTED", "A newer version of this software is already installed.");
        }

        private void WriteValidation()
        {
            foreach (Table table in DefaultData.Tables)
            {
                foreach (Column column in table.Columns)
                {
                    string category = column.Type == DataType.Object ? "Binary" : null;
                    string nullable = column.Constraint == Constraint.Null ? "Y" : "N";

                    Write("_Validation", table.Name, column.Name, nullable, null, null, null, null, category, null, null);
                }
            }
        }
    }
}