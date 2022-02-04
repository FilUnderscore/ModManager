using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager.UI
{
    public class XUiC_ModsList : XUiC_List<XUiC_ModsList.ListEntry>
    {
        private static string ModVersionCompatibleColor = "0,255,0,255";
        private static string ModVersionNotCompatibleColor = "255,0,0,255";
        private static string ModVersionMayNotBeCompatibleColor = "255,216,0,255";
        
        private static string ModEnabledColor = "0,255,0,255";
        private static string ModDisabledColor = "255,0,0,255";
        private static string ModEnabledChangeColor = "255,165,0,255";

        public override void Init()
        {
            base.Init();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.RebuildList(false);
        }

        public override void RebuildList(bool _resetFilter = false)
        {
            this.allEntries.Clear();
            this.ScanMods();
            this.allEntries.Sort();
            base.RebuildList(_resetFilter);
        }

        private void ScanMods()
        {
            List<ModEntry> loadedMods = CustomModManager.GetLoadedMods();

            foreach(var mod in loadedMods)
            {
                ListEntry entry = new ListEntry(mod);
                this.allEntries.Add(entry);
            }
        }

        public class ListEntry : XUiListEntry
        {
            public readonly ModEntry modEntry;
            public readonly ModInfo.ModInfo modInfo;

            public ListEntry(ModEntry modEntry)
            {
                this.modEntry = modEntry;
                this.modInfo = modEntry.info;
            }

            public override int CompareTo(object _otherEntry)
            {
                if (!(_otherEntry is ListEntry))
                    return 1;

                return 1 * modInfo.Name.Value.CompareTo(((ListEntry)_otherEntry).modInfo.Name.Value);
            }

            public override bool GetBindingValue(ref string _value, string _bindingName)
            {
                switch(_bindingName)
                {
                    case "modName":
                        _value = modInfo.Name.Value;
                        return true;
                    case "modVersion":
                        _value = modInfo.Version.Value + (modEntry.HasUpdateAvailable() ? "\u2191" : "");
                        return true;
                    case "modVersionColor":
                        if (modEntry.manifest != null)
                        {
                            ModManifest.EVersionUpdateComparisonResult versionUpdateComparisonResult = modEntry.manifest.CurrentVersionCompatibleWithDifferentGameVersion();
                            ModManifest.EVersionComparisonResult versionComparisonResult = modEntry.manifest.CurrentVersionCompatibleWithGameVersion();

                            if (versionUpdateComparisonResult == ModManifest.EVersionUpdateComparisonResult.Compatible || versionComparisonResult == ModManifest.EVersionComparisonResult.Compatible)
                            {
                                _value = ModVersionCompatibleColor;
                            }
                            else if(versionUpdateComparisonResult == ModManifest.EVersionUpdateComparisonResult.May_Not_Be_Compatible)
                            {
                                _value = ModVersionMayNotBeCompatibleColor;
                            }
                            else if(versionUpdateComparisonResult == ModManifest.EVersionUpdateComparisonResult.Not_Compatible)
                            {
                                _value = ModVersionNotCompatibleColor;
                            }
                        }

                        return true;
                    case "modVersionTooltip":
                        _value = Localization.Get("xuiModVersionTooltipCurrentVersion") + ": " + modInfo.Version.Value;

                        if (modEntry.manifest != null)
                        {
                            if (modEntry.manifest.GameVersionInformation != null)
                            {
                                _value += "\n" + Localization.Get("xuiModVersionTooltipForGameVersion") + ": " + modEntry.manifest.GameVersionInformation.LongString;
                            }

                            if (modEntry.HasUpdateAvailable())
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipAnUpdateIsAvailable") + ": " + modEntry.manifest.RemoteManifest.Version;
                                
                                if (modEntry.manifest.RemoteManifest.GameVersionInformation != null)
                                {
                                    _value += "\n" + Localization.Get("xuiModVersionTooltipForGameVersion") + ": " + modEntry.manifest.RemoteManifest.GameVersionInformation.LongString;
                                }
                            }
                            else if(modEntry.manifest != null && modEntry.manifest.UpToDate())
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipUpToDate");
                            }

                            ModManifest.EVersionUpdateComparisonResult versionUpdateComparisonResult = modEntry.manifest.CurrentVersionCompatibleWithDifferentGameVersion();

                            if (versionUpdateComparisonResult == ModManifest.EVersionUpdateComparisonResult.May_Not_Be_Compatible)
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipCurrentVersionMayNotBeCompatible");
                            }
                            else if (versionUpdateComparisonResult == ModManifest.EVersionUpdateComparisonResult.Not_Compatible)
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipCurrentVersionNotCompatible");
                            }

                            if (versionUpdateComparisonResult != ModManifest.EVersionUpdateComparisonResult.Compatible)
                            {
                                ModManifest.EVersionComparisonResult versionComparisonResult = modEntry.manifest.CurrentVersionCompatibleWithGameVersion();

                                if(versionComparisonResult == ModManifest.EVersionComparisonResult.Not_Compatible)
                                    _value += "\n\n" + (modEntry.manifest.RemoteManifest == null ? Localization.Get("xuiModVersionTooltipCurrentVersionNotSpecifiedNoManifestExists") : Localization.Get("xuiModVersionTooltipCurrentVersionNotSpecifiedManifest"));
                            }
                        }

                        return true;
                    case "modEnabled":
                        _value = GetModLoadedLocalization(modEntry.IsLoaded()) + (ModEnabledWillBeUpdated() ? "" + " \u2192 " + GetModLoadedLocalization(modEntry.WillBeEnabled().Value) : "");
                        return true;
                    case "modEnabledColor":
                        _value = ModEnabledWillBeUpdated() ? ModEnabledChangeColor : (!modEntry.IsLoaded() ? ModDisabledColor : ModEnabledColor);
                        return true;
                    case "modAuthor":
                        _value = modInfo.Author.Value;
                        return true;
                    default:
                        return false;
                }
            }

            private bool ModEnabledWillBeUpdated()
            {
                return modEntry.WillBeEnabled() != null && modEntry.WillBeEnabled() != modEntry.IsLoaded();
            }

            private string GetModLoadedLocalization(bool value)
            {
                return value ? Localization.Get("xuiModEnabled") : Localization.Get("xuiModDisabled");
            }

            public override bool MatchesSearch(string _searchString)
            {
                return modInfo.Name.Value.ToLowerInvariant().StartsWith(_searchString.ToLowerInvariant());
            }

            public static bool GetNullBindingValues(ref string _value, string _bindingName)
            {
                switch(_bindingName)
                {
                    case "modName":
                    case "modVersion":
                    case "modVersionTooltip":
                    case "modEnabled":
                    case "modAuthor":
                        _value = "";
                        return true;
                    case "modVersionColor":
                    case "modEnabledColor":
                        _value = "0,0,0";
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
