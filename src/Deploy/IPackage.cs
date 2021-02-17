using System;
using System.Collections.Generic;

namespace Deploy
{
    internal interface IPackage
    {
        string ProductName { get; }

        string ProductIcon { get; }

        string Author { get; }

        PackagePlatform Platform { get; }

        Guid UpgradeCode { get; }

        Version Version { get; }

        IEnumerable<PackageFile> Files { get; }
    }
}
