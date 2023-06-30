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

        public ModInfo(global::Mod mod) : this(mod.Path, mod.Name, mod.DisplayName, mod.Description, mod.Author, SemVer.Parse(mod.Version.ToString()), mod.Website)
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
    }
}
