using CustomModManager.Mod.Version;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CustomModManager.Mod.Manifest
{
    public sealed class ModManifestFromXml
    {
        public static ModManifest FromXml(string modName, string path)
        {
            path += "/Manifest.xml";

            if (!File.Exists(path))
                return null;

            return FromUrl(modName, path);
        }

        private static ModManifest FromUrl(string modName, string path, bool remote = false)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);
                
                return ParseManifest(modName, document.DocumentElement, remote);
            }
            catch
            {
                Log.Warning($"[{modName}] [Manifest] Failed fetching manifest from {path}.");
            }

            return null;
        }

        private static ModManifest ParseManifest(string modName, XmlElement documentElement, bool remote)
        {
            ModManifest manifest = new ModManifest();

            foreach(XmlNode node in documentElement.ChildNodes)
            {
                if(node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = (XmlElement)node;

                    if(element.Name.EqualsCaseInsensitive("ManifestUrl") && !remote)
                    {
                        manifest.RemoteManifest = FromUrl(modName, element.InnerText, true);
                    }
                    else if(element.Name.EqualsCaseInsensitive("Version"))
                    {
                        manifest.Version = SemVer.Parse(element.InnerText);
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
                    else if(element.Name.EqualsCaseInsensitive("Dependencies"))
                    {
                        ParseDependencies(modName, manifest, element);
                    }
                    else if(element.Name.EqualsCaseInsensitive("PatchNotes"))
                    {
                        ParsePatchNotes(modName, manifest, element);
                    }
                }
            }

            return manifest;
        }

        private static void ParseDependencies(string modName, ModManifest manifest, XmlElement dependenciesElement)
        {
            manifest.Dependencies = new List<string>();

            foreach(XmlNode child in dependenciesElement.ChildNodes)
            {
                if(child.NodeType == XmlNodeType.Element)
                {
                    XmlElement dependencyElement = (XmlElement)child;

                    if(!dependencyElement.HasAttribute("Name"))
                    {
                        Log.Warning($"[{modName}] [Manifest] Failed to parse dependency.");
                        continue;
                    }

                    string dependency = dependencyElement.GetAttribute("Name");
                    manifest.Dependencies.Add(dependency);
                }
            }
        }

        private static void ParsePatchNotes(string modName, ModManifest manifest, XmlElement patchNotesElement)
        {
            manifest.PatchNotes = new SortedDictionary<SemVer, string>();

            foreach(XmlNode child in patchNotesElement.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    XmlElement patchNoteElement = (XmlElement)child;
                    SemVer version;

                    if (!patchNoteElement.HasAttribute("Version") || (version = SemVer.Parse(patchNoteElement.GetAttribute("Version"))) == null)
                    {
                        Log.Warning($"[{modName}] [Manifest] Failed to parse patch note version.");
                        continue;
                    }

                    manifest.PatchNotes.Add(version, patchNoteElement.InnerText);
                }
            }
        }
    }
}
