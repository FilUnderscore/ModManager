using System.Collections.Generic;

namespace CustomModManager.XPath
{
    public sealed class XPathPatchScanner
    {
        public static List<XPathPatch> ScanPatches(XmlFile file)
        {
            List<XPathPatch> patches = new List<XPathPatch>();
            XmlFileParser parser = new XmlFileParser(file);

            parser.GetEntries("setattribute").ForEach(entry =>
            {
                if (SetAttributePatch(entry, out XPathPatch patch))
                    patches.Add(patch);
            });

            return patches;
        }

        private static bool SetAttributePatch(XmlEntry entry, out XPathPatch patch)
        {
            if(!entry.GetAttribute("xpath", out string xpath) || !entry.GetAttribute("name", out string name))
            {
                patch = null;
                return false;
            }

            patch = new XPathPatch();

            patch.type = XPathPatch.PatchType.SetAttribute;
            patch.xpath = xpath;
            patch.name = name;
            patch.value = entry.GetValueAsString();

            return true;
        }
    }
}
