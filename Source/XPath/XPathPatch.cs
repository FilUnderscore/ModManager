namespace CustomModManager.XPath
{
    public sealed class XPathPatch
    {
        public enum PatchType
        {
            SetAttribute
        }

        public PatchType type;

        public string xpath;
        public string name;
        public string value;
    }
}
