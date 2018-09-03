using System.Diagnostics;

namespace Deploy.Console
{
    [DebuggerDisplay("{Name}")]
    public class DirectoryEntry
    {
        public DirectoryEntry(string name, uint sector, ulong length)
        {
            Name = name;
            Sector = sector;
            Length = length;
        }

        public string Name { get; }

        public uint Sector { get; }

        public ulong Length { get; }
    }
}
