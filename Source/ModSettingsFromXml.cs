using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CustomModManager
{
    internal class ModSettingsFromXml
    {
        internal static void Load()
        {
            var filePath = CustomModManager.GetSettingsFileLocation();

            if (!File.Exists(filePath))
                return;

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            foreach(XmlNode node in document.DocumentElement.ChildNodes)
            {
                if(node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = (XmlElement)node;

                    if(element.Name.Equals("mod"))
                    {
                        ParseModSettings(element);
                    }
                }
            }
        }

        private static void ParseModSettings(XmlElement element)
        {
            if (!element.HasAttribute("name"))
            {
                Log.Warning("[Mod Manager] Mod settings without a mod name attached were found.");
                return;
            }

            string mod = element.GetAttribute("name");

            if(ModManagerModSettings.loadedSettings.ContainsKey(mod))
            {
                Log.Warning($"[Mod Manager] Mod settings for mod {mod} have already been loaded.");
                return;
            }

            Dictionary<string, string> loadedSettings = new Dictionary<string, string>();

            foreach(XmlNode childNode in element.ChildNodes)
            {
                if(childNode.NodeType == XmlNodeType.Element)
                {
                    XmlElement settingElement = (XmlElement)childNode;

                    if (!settingElement.Name.Equals("setting") || !settingElement.HasAttribute("key") || !settingElement.HasAttribute("value"))
                        continue;

                    string key = settingElement.GetAttribute("key");
                    string value = settingElement.GetAttribute("value");

                    loadedSettings.Add(key, value);
                }
            }

            ModManagerModSettings.loadedSettings.Add(mod, loadedSettings);
        }

        internal static void Save()
        {
            if (!ModManagerModSettings.changed)
                return;

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement settingsRoot = xmlDoc.AddXmlElement("mod_settings");

            foreach (var settingsEntry in ModManagerModSettings.modSettingsInstances)
            {
                var mod = settingsEntry.Key;
                var loadedSettings = settingsEntry.Value;

                XmlElement modElement = settingsRoot.AddXmlElement("mod");
                modElement.SetAttribute("name", mod.info.Name.Value);
                
                foreach(var settingEntry in loadedSettings.settings)
                {
                    var key = settingEntry.Key;
                    var setting = settingEntry.Value;

                    XmlElement settingElement = modElement.AddXmlElement("setting");
                    settingElement.SetAttribute("key", key);
                    settingElement.SetAttribute("value", setting.GetValueAsString().unformatted);
                    modElement.AppendChild(settingElement);

                    setting.SetLastValueInternal();
                }

                settingsRoot.AppendChild(modElement);
            }

            xmlDoc.Save(CustomModManager.GetSettingsFileLocation());

            ModManagerModSettings.changed = false;
        }
    }
}
