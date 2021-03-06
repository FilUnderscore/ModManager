using HarmonyLib;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using CustomModManager.UI;

namespace CustomModManager
{
    public class CustomModManager
    {
        private static readonly string modSettingsFilename = "mod-settings.xml";
        
        internal static bool Save()
        {
            SaveModSettings();

            List<ModEntry> updatedMods = new List<ModEntry>();
            int updatedCount = 0;

            foreach(var mod in ModLoader.GetLoadedMods())
            {
                if (!mod.flag)
                    continue;

                updatedMods.Add(mod);
                mod.flag = false;
            }

            if (updatedMods.Count == 0)
                return false;

            string file = ModLoader.modPath + "/" + ModLoader.disabledModsFilename;
            List<string> lines = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();

            foreach (var mod in updatedMods)
            {
                if (mod.WillBeEnabled().Value)
                    lines.RemoveAll(folderName => Path.GetFileName(mod.loadInfo.modPath) == folderName);
                else
                    lines.Add(Path.GetFileName(mod.loadInfo.modPath));

                if (mod.WillBeEnabled().Value != mod.IsLoaded())
                    updatedCount++;
            }

            File.WriteAllLines(file, lines);
            
            return updatedCount > 0;
        }

        private static void SaveModSettings()
        {
            ModSettingsFromXml.Save();
        }

        internal static string GetSettingsFileLocation()
        {
            return GameIO.GetApplicationPath() + "/" + modSettingsFilename;
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
                    switch(_bindingName)
                    {
                        case "modlisttext":
                            List<ModEntry> loadedMods = ModLoader.GetActiveMods();

                            string loadedModsStr = loadedMods[0].info.Name.Value;

                            for(int i = 1; i < loadedMods.Count; i++)
                            {
                                loadedModsStr += " \u2022 " + loadedMods[i].info.Name.Value;
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
