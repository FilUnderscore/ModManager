using CustomModManager.Mod.Info.Parser.Parsers;
using System.IO;
using System.Xml.Linq;

namespace CustomModManager.Mod.Info.Parser
{
    public sealed class ModInfoParserFactory
    {
        public static IModInfoParser CreateParser(string modDir)
        {
            if(!File.Exists(modDir + "/ModInfo.xml"))
                return null;

            XmlFile xml = new XmlFile(modDir, "ModInfo.xml");
            XElement root = xml.XmlDoc.Root;

            if (root == null)
                return null;

            if (root.Element("ModInfo") != null)
                return new ModInfoV1Parser(modDir, xml);
            else
                return new ModInfoV2Parser(modDir, root);
        }
    }
}
