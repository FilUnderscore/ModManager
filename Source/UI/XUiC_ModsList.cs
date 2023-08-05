using CustomModManager.Mod;
using CustomModManager.Mod.Version;
using CustomModManager.UI.Wrappers;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static XUiC_DropDown;

namespace CustomModManager.UI
{
    public class XUiC_ModsList : XUiC_List<XUiC_ModsList.ListEntry>
    {
        private static string ModVersionCompatibleColor = "0,255,0,255";
        private static string ModVersionNotCompatibleColor = "255,0,0,255";
        private static string ModVersionMayNotBeCompatibleColor = "255,216,0,255";
        private static string ModVersionNoManifestFoundColor = "255,255,255,255";

        private static string ModEnabledColor = "0,255,0,255";
        private static string ModDisabledColor = "255,0,0,255";
        private static string ModEnabledChangeColor = "255,165,0,255";

        private SortingType sortingType = SortingType.Alphanumerical;
        private readonly Dictionary<XUiController, ListEntryControllerData> defaults = new Dictionary<XUiController, ListEntryControllerData>();

        public XUiC_ModsList()
        {
            ListEntry_SetEntry_Patch.Instance = this;
        }

        private class ListEntryControllerData
        {
            public readonly XUiW_Texture IconTexture;

            public readonly XUiV_Label NameLabel;
            public readonly XUiV_Label EnabledLabel;

            public readonly Vector2i OriginalNameLabelPosition;
            public readonly Vector2i OriginalEnabledLabelPosition;

            public ListEntryControllerData(XUiController controller)
            { 
                this.IconTexture = new XUiW_Texture(controller, "Icon");

                this.NameLabel = (XUiV_Label)controller.GetChildById("Name").ViewComponent;
                this.OriginalNameLabelPosition = this.NameLabel.Position;

                this.EnabledLabel = (XUiV_Label)controller.GetChildById("Enabled").ViewComponent;
                this.OriginalEnabledLabelPosition = this.EnabledLabel.Position;
            }
        }

        public override void Init()
        {
            base.Init();

            this.GetChildById("btnSort").ViewComponent.Controller.OnPress += SortButton_OnPress;
            this.GetChildById("btnRefresh").ViewComponent.Controller.OnPress += RefreshButton_OnPress;

            foreach(var listController in this.listEntryControllers)
            {
                defaults.Add(listController, new ListEntryControllerData(listController));
            }
        }

        private void RefreshButton_OnPress(XUiController _sender, int _mouseButton)
        {
            ModLoader.Instance.Load(ModManagerMod.modPaths.ToArray());

            this.RebuildList(true);
            this.RefreshBindings(true);
            this.RefreshView(true);
        }

        private void SortButton_OnPress(XUiController _sender, int _mouseButton)
        {
            this.sortingType = this.sortingType.CycleEnum();
            this.RefreshBindings(true);

            this.allEntries.Sort((x, y) => Sort(x, y));
            this.RefreshView(false);
        }

        private int Sort(ListEntry a, ListEntry b)
        {
            switch(sortingType)
            {
                case SortingType.Alphanumerical:
                    return a.modEntry.Info.DisplayName.CompareTo(b.modEntry.Info.DisplayName);
                case SortingType.Author:
                    return a.modEntry.Info.Author.CompareTo(b.modEntry.Info.Author);
            }

            return 0;
        }

        private string GetSortingNameUnlocalized()
        {
            switch(sortingType)
            {
                case SortingType.Alphanumerical:
                    return Localization.Get("xuiModsSortAlphanumerical");
                case SortingType.Author:
                    return Localization.Get("xuiModsSortAuthor");
            }

            return Localization.Get("xuiModsSortUndefined");
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.RebuildList(false);
            this.RefreshBindings(true);
        }

        public override void RebuildList(bool _resetFilter = false)
        {
            this.allEntries.Clear();
            this.ScanMods();
            this.allEntries.Sort((x, y) => Sort(x, y));
            base.RebuildList(_resetFilter);
        }

        private void ScanMods()
        {
            List<Mod.Mod> loadedMods = ModLoader.Instance.GetMods();

            foreach(var mod in loadedMods)
            {
                ListEntry entry = new ListEntry(mod);
                this.allEntries.Add(entry);
            }
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch(_bindingName)
            {
                case "sortingTooltip":
                    _value = string.Format(Localization.Get("xuiModsSort"), GetSortingNameUnlocalized());
                    return true;
                case "refreshTooltip":
                    _value = Localization.Get("xuiModsRefresh");
                    return true;
                default:
                    return base.GetBindingValue(ref _value, _bindingName);
            }
        }

        public class ListEntry : XUiListEntry
        {
            public readonly Mod.Mod modEntry;

            public ListEntry(Mod.Mod modEntry)
            {
                this.modEntry = modEntry;
            }

            public override int CompareTo(object _otherEntry)
            {
                if (!(_otherEntry is ListEntry))
                    return 1;

                return 1 * modEntry.Info.DisplayName.CompareTo(((ListEntry)_otherEntry).modEntry.Info.DisplayName);
            }

            public override bool GetBindingValue(ref string _value, string _bindingName)
            {
                switch(_bindingName)
                {
                    case "modName":
                        _value = modEntry.Info.DisplayName;
                        return true;
                    case "modVersion":
                        _value = modEntry.Version.ToString() + (modEntry.Manifest != null && modEntry.Manifest.UpToDate() == EVersionStatus.Not_Up_To_Date ? "\u2191" : "");
                        return true;
                    case "modVersionColor":
                        if (modEntry.Manifest != null)
                        {
                            EVersionUpdateComparisonResult versionUpdateComparisonResult = modEntry.Manifest.CurrentVersionCompatibleWithDifferentGameVersion();
                            EVersionComparisonResult versionComparisonResult = modEntry.Manifest.CurrentVersionCompatibleWithGameVersion();

                            if (versionUpdateComparisonResult == EVersionUpdateComparisonResult.Compatible || versionComparisonResult == EVersionComparisonResult.Compatible)
                            {
                                _value = ModVersionCompatibleColor;
                            }
                            else if(versionUpdateComparisonResult == EVersionUpdateComparisonResult.May_Not_Be_Compatible)
                            {
                                _value = ModVersionMayNotBeCompatibleColor;
                            }
                            else if(versionUpdateComparisonResult == EVersionUpdateComparisonResult.Not_Compatible)
                            {
                                _value = ModVersionNotCompatibleColor;
                            }
                        }
                        else
                        {
                            _value = ModVersionNoManifestFoundColor;
                        }

                        return true;
                    case "modVersionTooltip":
                        _value = string.Format(Localization.Get("xuiModVersionTooltipCurrentVersion"), modEntry.Version.ToString());

                        if (modEntry.Manifest != null)
                        {
                            if (modEntry.Manifest.GameVersionInformation != null)
                            {
                                _value += "\n" + string.Format(Localization.Get("xuiModVersionTooltipForGameVersion"), modEntry.Manifest.GameVersionInformation.LongString);
                            }

                            if (modEntry.Manifest.UpToDate() == EVersionStatus.Not_Up_To_Date)
                            {
                                _value += "\n\n" + string.Format(Localization.Get("xuiModVersionTooltipAnUpdateIsAvailable"), modEntry.Manifest.RemoteManifest.Version);
                                
                                if (modEntry.Manifest.RemoteManifest.GameVersionInformation != null)
                                {
                                    _value += "\n" + string.Format(Localization.Get("xuiModVersionTooltipForGameVersion"), modEntry.Manifest.RemoteManifest.GameVersionInformation.LongString);
                                }
                            }
                            else if(modEntry.Manifest != null && modEntry.Manifest.UpToDate() == EVersionStatus.Up_To_Date)
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipUpToDate");
                            }

                            EVersionUpdateComparisonResult versionUpdateComparisonResult = modEntry.Manifest.CurrentVersionCompatibleWithDifferentGameVersion();

                            if (versionUpdateComparisonResult == EVersionUpdateComparisonResult.May_Not_Be_Compatible)
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipCurrentVersionMayNotBeCompatible");
                            }
                            else if (versionUpdateComparisonResult == EVersionUpdateComparisonResult.Not_Compatible)
                            {
                                _value += "\n\n" + Localization.Get("xuiModVersionTooltipCurrentVersionNotCompatible");
                            }

                            if (versionUpdateComparisonResult != EVersionUpdateComparisonResult.Compatible && modEntry.Manifest.RemoteManifest == null)
                            {
                                EVersionComparisonResult versionComparisonResult = modEntry.Manifest.CurrentVersionCompatibleWithGameVersion();

                                if(versionComparisonResult == EVersionComparisonResult.Not_Compatible)
                                    _value += "\n\n" + (modEntry.Manifest.RemoteManifest == null ? Localization.Get("xuiModVersionTooltipCurrentVersionNotSpecifiedNoManifestExists") : Localization.Get("xuiModVersionTooltipCurrentVersionNotSpecifiedManifest"));
                            }
                        }

                        return true;
                    case "modEnabled":
                        _value = GetModLoadedLocalization(modEntry.Loaded) + (modEntry.WillChangeState ? "" + " \u2192 " + GetModLoadedLocalization(modEntry.NextState) : "");
                        return true;
                    case "modEnabledColor":
                        _value = modEntry.WillChangeState ? ModEnabledChangeColor : (!modEntry.Loaded ? ModDisabledColor : ModEnabledColor);
                        return true;
                    case "modAuthor":
                        _value = modEntry.Info.Author;
                        return true;
                    case "hasModIcon":
                        _value = modEntry.DoesFileExist(modEntry.GetModFolderPath("icon.png")).ToString();
                        return true;
                    default:
                        return false;
                }
            }

            private string GetModLoadedLocalization(bool value)
            {
                return value ? Localization.Get("xuiModEnabled") : Localization.Get("xuiModDisabled");
            }

            public override bool MatchesSearch(string _searchString)
            {
                return modEntry.Info.DisplayName.ToLowerInvariant().Contains(_searchString.ToLowerInvariant());
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
                    case "hasModIcon":
                        _value = false.ToString();
                        return true;
                    default:
                        return false;
                }
            }
        }

        private enum SortingType
        {
            Alphanumerical,
            Author
        }

        [HarmonyPatch(typeof(XUiC_ListEntry<ListEntry>))]
        [HarmonyPatch("SetEntry")]
        private class ListEntry_SetEntry_Patch
        {
            public static XUiC_ModsList Instance;

            static void Prefix(XUiC_ListEntry<ListEntry> __instance, ListEntry _data)
            {
                if (__instance.GetType() != typeof(XUiC_ListEntry<ListEntry>))
                    return;

                if(_data != __instance.GetEntry() && _data != null)
                {
                    ListEntryControllerData data = Instance.defaults[__instance];
                    int icon_x_offset = data.IconTexture.GetWidth() + 8;

                    if (_data.modEntry.DoesFileExist(_data.modEntry.GetModFolderPath("icon.png")))
                    {
                        data.IconTexture.SetTexture(_data.modEntry.GetModFolderPath("icon.png"));
                    }

                    Vector2i icon_x_offset_v2i = new Vector2i(icon_x_offset, 0);

                    data.NameLabel.Position = data.OriginalNameLabelPosition + icon_x_offset_v2i;
                    data.EnabledLabel.Position = data.OriginalEnabledLabelPosition + icon_x_offset_v2i;
                }
            }
        }
    }
}
