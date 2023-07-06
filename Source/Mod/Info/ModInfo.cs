using CustomModManager.Mod.Version;

namespace CustomModManager.Mod.Info
{
    public sealed class ModInfo
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }
        public IModVersion Version { get; private set; }
        public string Website { get; private set; }

        public ModInfo(global::Mod mod) : this(mod.Path, mod.Name, mod.DisplayName, mod.Description, mod.Author, Parse(mod.Version), mod.Website)
        {
        }

        public ModInfo(string path, string name, string displayName, string description, string author, IModVersion version, string website)
        {
            this.Path = path;
            this.Name = name;
            this.DisplayName = displayName;
            this.Description = description;
            this.Author = author;
            this.Version = version;
            this.Website = website;
        }

        private static IModVersion Parse(System.Version version)
        {
            if (version != null)
            {
                IModVersion modVersion = SemVer.Parse(version.ToString());

                if (modVersion == null)
                    modVersion = SemVer.Parse($"{version.Major}.{version.Minor}.{version.Build}-rev.{version.Revision}");

                return modVersion;
            }
            else
            {
                return new UndefinedModVersion();
            }
        }

        private sealed class UndefinedModVersion : IModVersion
        {
            public override string ToString()
            {
                return "Undefined";
            }
        }
    }
}