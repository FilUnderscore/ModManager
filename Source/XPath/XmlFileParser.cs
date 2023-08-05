using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CustomModManager.XPath
{
    public sealed class XmlFileParser : XmlEntry
    {
        public XmlFileParser(XmlFile file) : base(file.XmlDoc.Root)
        {
        }
    }

    public class XmlEntry
    {
        private readonly XElement self;
        private readonly List<XmlEntry> entries;

        public XmlEntry(XElement self)
        {
            this.self = self;
            this.entries = new List<XmlEntry>();

            this.self.Elements().ToList().ForEach(node =>
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    entries.Add(new XmlEntry(node));
                }
            });
        }

        public List<XmlEntry> GetEntries(string tag)
        {
            return this.entries.Where(entry => entry.self.Name.ToString().Equals(tag)).ToList();
        }

        public bool GetAttribute(string attributeName, out string attributeValue)
        {
            if (this.self.HasAttribute(attributeName))
            {
                attributeValue = this.self.GetAttribute(attributeName);
                return true;
            }

            attributeValue = null;
            return false;
        }

        public string GetValueAsString()
        {
            return this.self.Value;
        }
    }
}
