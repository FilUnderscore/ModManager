using CustomModManager.Mod.Version;
using System.Xml.Linq;

namespace CustomModManager.Mod.Info.Parser.Parsers
{
    public sealed class ModInfoV2Parser : IModInfoParser
    {
        private readonly string modPath;
        private readonly XElement root;

        public ModInfoV2Parser(string modPath, XElement root)
        {
            this.modPath = modPath;
            this.root = root;
        }

        public bool TryParse(out ModInfo modInfo)
        {
            if(!TryGetElementAttributeValue(this.root, "Name", out var name))
            {
                modInfo = null;
                return false;
            }

            TryGetElementAttributeValue(this.root, "Version", out var version);
            TryGetElementAttributeValue(this.root, "DisplayName", out var displayName);
            TryGetElementAttributeValue(this.root, "Description", out var description);
            TryGetElementAttributeValue(this.root, "Author", out var author);
            TryGetElementAttributeValue(this.root, "Website", out var website);

            IModVersion modVersion = SemVer.Parse(version);

            modInfo = new ModInfo(this.modPath, name, displayName, description, author, modVersion, website);
            return true;
        }

        private bool TryGetElementAttributeValue(XElement element, string name, out string value)
        {
            if(element == null || element.Element(name) == null || !element.Element(name).HasAttribute("value"))
            {
                value = null;
                return false;
            }

            value = element.Element(name).Attribute("value").Value;
            return true;
        }
    }
}
