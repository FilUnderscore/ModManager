using CustomModManager.Mod.Version;

namespace CustomModManager.Mod.Info
{
    public readonly struct ModInfo
    {
        public readonly string Path;
        public readonly string Name;
        public readonly string DisplayName;
        public readonly string Description;
        public readonly string Author;
        public readonly IModVersion Version;
        public readonly string Website;

        public ModInfo(global::Mod mod) : this(mod.Path, mod.Name, mod.DisplayName, mod.Description, mod.Author, new ModVersion(mod.Version), mod.Website)
        {
        }

        public ModInfo(string path, string name, string displayName, string description, string author, IModVersion version, string website)
        {
            this.Path = path;
            this.Name = name;
            this.DisplayName = string.IsNullOrEmpty(displayName) ? name : displayName;
            this.Description = description;
            this.Author = author;
            this.Version = version;
            this.Website = website;
        }

        private sealed class ModVersion : IModVersion
        {
            private readonly System.Version version;

            public ModVersion(System.Version version)
            {
                this.version = version;
            }

            public override string ToString()
            {
                return this.version != null ? this.version.ToString() : "Undefined";
            }
        }
    }
}