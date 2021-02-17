namespace Deploy
{
    internal class PackageFile
    {
        public PackageFile(string filename, string shortcutIcon, string shortcutName)
        {
            Filename = filename;
            ShortcutIcon = shortcutIcon;
            ShortcutName = shortcutName;
        }

        public string Filename { get; }

        public string ShortcutName { get; }

        public string ShortcutIcon { get; }
    }
}
