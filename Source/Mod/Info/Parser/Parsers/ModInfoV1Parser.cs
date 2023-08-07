using ModInfo;
using CustomModManager.Mod.Version;
using System;
using System.Collections.Generic;

namespace CustomModManager.Mod.Info.Parser.Parsers
{
    public sealed class ModInfoV1Parser : IModInfoParser
    {
        private readonly string modPath;
        private readonly XmlFile xml;

        public ModInfoV1Parser(string modPath, XmlFile xml)
        {
            this.modPath = modPath;
            this.xml = xml;
        }

        public bool TryParse(out ModInfo modInfo)
        {
            List<global::ModInfo.ModInfo> modInfoEntries = new List<global::ModInfo.ModInfo>();

            try
            {
                modInfoEntries = ModInfoLoader.ParseXml(this.modPath + "/ModInfo.xml", xml);
            }
            catch(Exception e)
            {
                modInfo = default(ModInfo);
                return false;
            }

            if(modInfoEntries.Count != 1) 
            {
                modInfo = default(ModInfo);
                return false;
            }

            global::ModInfo.ModInfo modInfoEntry = modInfoEntries[0];

            if(modInfoEntry == null)
            {
                modInfo = default(ModInfo);
                return false;
            }

            string name = modInfoEntry.Name.Value;
            if(string.IsNullOrEmpty(name))
            {
                modInfo = default(ModInfo);
                return false;
            }

            System.Version result = null;
            if(!string.IsNullOrEmpty(modInfoEntry.Version?.Value))
                System.Version.TryParse(modInfoEntry.Version.Value, out result);

            IModVersion version = null;
            if (result != null)
                version = SemVer.Parse($"{result.Major}.{result.Minor}.{result.Build}-rev.{result.Revision}");
            
            modInfo = new ModInfo(this.modPath, name, name, modInfoEntry.Description?.Value, modInfoEntry.Author?.Value, version, modInfoEntry.Website?.Value);
            return true;
        }
    }
}
