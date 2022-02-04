using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine;

namespace CustomModManager
{
    public class ModManifestFromXml
    {
        public static ModManifest FromXml(ModEntry entry)
        {
            string path = CustomModManager.GetModEntryFolderLocation(entry) + "/Manifest.xml";

            Log.Out("Path: " + path + " E: " + File.Exists(path));

            if (!File.Exists(path))
                return null;

            return FromXml(entry, path);
        }

        private static ModManifest FromXml(ModEntry entry, string path)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);

                return ParseManifest(entry, document.DocumentElement);
            }
            catch
            {
                Log.Warning($"[{entry.info.Name.Value}] [Manifest] Failed fetching manifest from {path}.");
            }

            return null;
        }

        private static ModManifest ParseManifest(ModEntry entry, XmlElement documentElement)
        {
            ModManifest manifest = new ModManifest(entry);

            foreach(XmlNode node in documentElement.ChildNodes)
            {
                if(node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = (XmlElement)node;

                    if(element.Name.EqualsCaseInsensitive("ManifestUrl"))
                    {
                        manifest.RemoteManifest = FromXml(entry, element.InnerText);
                    }
                    else if(element.Name.EqualsCaseInsensitive("Version"))
                    {
                        manifest.Version = element.InnerText;
                    }
                    else if(element.Name.EqualsCaseInsensitive("GameVersion"))
                    {
                        string releaseTypeStr = element.GetAttribute("ReleaseType");
                        string majorVersionStr = element.GetAttribute("Major");
                        string minorVersionStr = element.GetAttribute("Minor");
                        string buildVersionStr = element.GetAttribute("Build");

                        VersionInformation.EGameReleaseType gameReleaseType = (VersionInformation.EGameReleaseType) Enum.Parse(typeof(VersionInformation.EGameReleaseType), releaseTypeStr);
                        int majorVersion = int.Parse(majorVersionStr);
                        int minorVersion = int.Parse(minorVersionStr);
                        int buildVersion = int.Parse(buildVersionStr);

                        VersionInformation versionInformation = new VersionInformation(gameReleaseType, majorVersion, minorVersion, buildVersion);
                        manifest.GameVersionInformation = versionInformation;
                    }
                    else if(element.Name.EqualsCaseInsensitive("UpdateUrl"))
                    {
                        manifest.UpdateUrl = element.InnerText;
                    }
                    else if(element.Name.EqualsCaseInsensitive("PatchNotes"))
                    {
                        ParsePatchNotes(manifest, element);
                    }
                }
            }

            return manifest;
        }

        private static void ParsePatchNotes(ModManifest manifest, XmlElement patchNotesElement)
        {
            manifest.PatchNotes = new Dictionary<string, List<ModManifest.PatchNote>>();

            foreach(XmlNode child in patchNotesElement.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    XmlElement patchNoteElement = (XmlElement)child;

                    if (!patchNoteElement.HasAttribute("version"))
                    {
                        Log.Warning($"[{manifest.entry.info.Name.Value}] [Manifest] Failed to parse patch note version.");
                        continue;
                    }

                    string version = patchNoteElement.GetAttribute("version");

                    foreach (XmlNode entryChild in patchNoteElement.ChildNodes)
                    {
                        if(entryChild.NodeType == XmlNodeType.Element)
                        {
                            XmlElement entryNoteElement = (XmlElement)entryChild;

                            Color color = Color.white;

                            if (entryNoteElement.HasAttribute("color"))
                                color = StringParsers.ParseColor32(entryNoteElement.GetAttribute("color"));

                            string patchNote = entryNoteElement.InnerText;

                            if (!manifest.PatchNotes.ContainsKey(version))
                                manifest.PatchNotes.Add(version, new List<ModManifest.PatchNote>());

                            manifest.PatchNotes[version].Add(new ModManifest.PatchNote
                            {
                                Text = patchNote,
                                Color = color
                            });
                        }
                    }
                }
            }
        }
    }
}
