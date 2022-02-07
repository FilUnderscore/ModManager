using HarmonyLib;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CustomModManager.UI;
using CustomModManager.API;

namespace CustomModManager
{
    public class CustomModManager
    {
        private static readonly FieldInfo MOD_MANAGER_MOD_PATH_FIELD = AccessTools.DeclaredField(typeof(ModManager), "MOD_PATH");
        private static readonly string modSettingsFilename = "mod-settings.xml";

        private static readonly string modPath = (string)MOD_MANAGER_MOD_PATH_FIELD.GetValue(null);
        
        static CustomModManager()
        {
            ModSettingsFromXml.Load();
        }

        internal static bool Save()
        {
            SaveModSettings();

            List<ModEntry> updatedMods = new List<ModEntry>();

            foreach(var mod in ModLoader.GetLoadedMods())
            {
                if (!mod.HasBeenChanged())
                    continue;

                updatedMods.Add(mod);
            }

            if (updatedMods.Count == 0)
                return false;

            string file = modPath + "/" + ModLoader.disabledModsFilename;
            List<string> lines = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();

            foreach (var mod in updatedMods)
            {
                if (mod.WillBeEnabled().Value)
                    lines.RemoveAll(folderName => Path.GetFileName(mod.loadInfo.modPath) == folderName);
                else
                    lines.Add(Path.GetFileName(mod.loadInfo.modPath));
            }

            File.WriteAllLines(file, lines);
            
            return true;
        }

        internal static bool HasModBeenChanged()
        {
            return ModLoader.GetLoadedMods().Any(mod => mod.HasBeenChanged());
        }

        private static void SaveModSettings()
        {
            ModSettingsFromXml.Save();
        }

        public static string GetModsFolderLocation()
        {
            return modPath;
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
        }

        
    }
}
