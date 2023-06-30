using HarmonyLib;

using CustomModManager.API;
using CustomModManager.Mod;
using System.Linq;
using System.IO;
using CustomModManager.UI;
using System.Collections.Generic;
using System.Reflection;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
        private static readonly PropertyInfo MODS_BASE_PATH_PROPERTY = AccessTools.Property(typeof(ModManager), "ModsBasePath");
        private static readonly FieldInfo MODS_BASE_PATH_LEGACY_FIELD = AccessTools.Field(typeof(ModManager), "ModsBasePathLegacy");

        private ModManagerAPI.ModSettings.ModSetting<string> modDirSetting;
        private ModManagerAPI.ModSettings.ModSetting<int> currentModDirSetting;
        private ModManagerAPI.ModSettings.ModSetting<string> openModDirButton;

        private readonly ISet<string> modPaths = new HashSet<string>();
        private int selectedModDir;

        private bool showPatchNotesOnStartup = true, showUpdatesOnStartup = true;
        private ModLoader loader;

        public ModManagerMod()
        {
            var harmony = new Harmony("filunderscore.modmanager");
            harmony.PatchAll();

            this.AddBasePaths();

            loader = new ModLoader();
        }

        private void AddBasePaths()
        {
            if (MODS_BASE_PATH_LEGACY_FIELD != null)
                this.modPaths.Add((string)MODS_BASE_PATH_LEGACY_FIELD.GetValue(null));

            if (MODS_BASE_PATH_PROPERTY != null)
                this.modPaths.Add((string)MODS_BASE_PATH_PROPERTY.GetValue(null));
        }

        public void InitMod(global::Mod _modInstance)
        {
            loader.SetSelfAndDetectMods(_modInstance);

            if (ModManagerAPI.IsModManagerLoaded())
            {
                ModManagerAPI.ModSettings settings = ModManagerAPI.GetModSettings(_modInstance);
                this.InitSettings(settings);
            }
        }

        private void InitSettings(ModManagerAPI.ModSettings settings)
        {
            settings.Hook("modDir", "xuiModManagerModDirSetting", value =>
            {
                List<string> paths = value.Split(';').ToList();

                this.modPaths.Clear();

                this.AddBasePaths();
                foreach (var path in paths)
                {
                    this.modPaths.Add(path);
                }
                this.loader.Load(this.modPaths.ToArray());

                if (currentModDirSetting != null)
                {
                    currentModDirSetting.SetMinimumMaximumAndIncrementValues(0, paths.Count, 1);
                    currentModDirSetting.Update();

                    if (openModDirButton != null)
                        openModDirButton.Update();
                }
            }, () => this.modPaths.ToList().StringFromList(";"), toStr =>
            {
                int dirCount = toStr.Split(';').Length;
                return (toStr, dirCount + " Director" + (dirCount > 1 ? "ies" : "y"));
            }, str =>
            {
                string[] paths = str.Split(';').ToArray();
                bool success = paths.All(path => Directory.Exists(path.Trim()));

                return (str, success);
            });

            this.currentModDirSetting = settings.Hook("currentModDir", "xuiModManagerCurrentModDirSetting", value =>
            {
                selectedModDir = value;

                if (openModDirButton != null)
                    openModDirButton.Update();
            }, () => selectedModDir, toStr => (toStr.ToString(), toStr <= this.modPaths.Count && toStr > 0 ? this.modPaths.ToList()[toStr - 1] : "Choose a Mod Directory"), str =>
            {
                bool success = int.TryParse(str, out int val);
                return (val, success);
            }).SetMinimumMaximumAndIncrementValues(0, this.modPaths.Count, 1);

            this.openModDirButton = settings.Button("openModDirButton", "xuiModManagerOpenModDirButton", () =>
            {
                if (selectedModDir > 0 && selectedModDir <= this.modPaths.Count)
                    UnityEngine.Application.OpenURL(this.modPaths.ToList()[selectedModDir - 1]);
            },
            () => Localization.Get("xuiModManagerOpenModDirButtonText")).SetEnabled(() => this.modPaths.Count > 0 && selectedModDir > 0 && selectedModDir <= this.modPaths.Count);

            /*
            settings.Category("startup", "Startup");

            settings.Hook("showPatchNotesOnStartup", "xuiModManagerShowPatchNotesOnStartupSetting", value => showPatchNotesOnStartup = value, () => showPatchNotesOnStartup, (value) => (value.ToString(), value ? "Yes" : "No"), (str) =>
            {
                bool result = bool.TryParse(str, out bool val);
                return (val, result);
            }).SetAllowedValues(new bool[] { true, false });

            settings.Hook("showUpdatesOnStartup", "xuiModManagerShowUpdatesOnStartupSetting", value => showUpdatesOnStartup = value, () => showUpdatesOnStartup, (value) => (value.ToString(), value ? "Yes" : "No"), (str) =>
            {
                bool result = bool.TryParse(str, out bool val);
                return (val, result);
            }).SetAllowedValues(new bool[] { true, false });            
            */
        }

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(XUiC_MainMenu))]
            [HarmonyPatch(nameof(XUiC_MainMenu.Init))]
            class XUiC_MainMenuInitHook
            {
                static void Postfix(XUiC_MainMenu __instance)
                {
                    ((XUiC_SimpleButton)__instance.GetChildById("btnMods")).OnPressed += XUiC_MainMenuInitHook_OnPressed;
                }

                private static void XUiC_MainMenuInitHook_OnPressed(XUiController _sender, int _mouseButton)
                {
                    _sender.xui.playerUI.windowManager.Close(_sender.WindowGroup.ID);
                    _sender.xui.playerUI.windowManager.Open(XUiC_Mods.ID, true);
                }
            }

            [HarmonyPatch(typeof(XUiC_LoadingScreen))]
            [HarmonyPatch("GetBindingValue")]
            class XUiC_LoadingScreenGetBindingValueHook
            {
                static void Postfix(XUiC_LoadingScreen __instance, ref bool __result, ref string _value, string _bindingName)
                {
                    switch (_bindingName)
                    {
                        case "modlisttext":
                            List<Mod.Mod> loadedMods = ModLoader.Instance.GetMods();

                            string loadedModsStr = loadedMods[0].Info.DisplayName;

                            for (int i = 1; i < loadedMods.Count; i++)
                            {
                                loadedModsStr += " \u2022 " + loadedMods[i].Info.DisplayName;
                            }

                            _value = loadedModsStr;
                            __result = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
