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
        private static readonly FieldInfo MOD_MANAGER_LOADED_MODS_FIELD = AccessTools.DeclaredField(typeof(ModManager), "loadedMods");
        private static readonly string disabledModsFilename = "disabled-mods.txt";
        private static readonly string modSettingsFilename = "mod-settings.xml";

        private static readonly DictionaryList<string, ModEntry> loadedMods = new DictionaryList<string, ModEntry>();

        private static readonly string modPath = (string)MOD_MANAGER_MOD_PATH_FIELD.GetValue(null);
        private static readonly List<string> disabledMods = new List<string>();

        static CustomModManager()
        {
            LoadModList();
            ModSettingsFromXml.Load();
        }

        private static void LoadModList()
        {
            string file = modPath + "/" + disabledModsFilename;

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (var line in lines)
                {
                    Log.Out(line);
                    disabledMods.Add(line);
                }
            }
        }

        internal static bool Save()
        {
            SaveModSettings();

            List<ModEntry> updatedMods = new List<ModEntry>();

            foreach(var mod in loadedMods.list)
            {
                if (!mod.HasBeenChanged())
                    continue;

                updatedMods.Add(mod);
            }

            if (updatedMods.Count == 0)
                return false;

            string file = modPath + "/" + disabledModsFilename;
            List<string> lines = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();

            foreach (var mod in updatedMods)
            {
                if (mod.WillBeEnabled().Value)
                    lines.RemoveAll(folderName => mod.folderName == folderName);
                else
                    lines.Add(mod.folderName);
            }

            File.WriteAllLines(file, lines);

            return true;
        }

        private static void SaveModSettings()
        {
            ModSettingsFromXml.Save();
        }

        public static string GetModsFolderLocation()
        {
            return modPath;
        }

        public static string GetModEntryFolderLocation(ModEntry entry)
        {
            return modPath + "/" + entry.folderName;
        }

        internal static ModEntry GetModEntryFromModInstance(Mod modInstance)
        {
            foreach(var mod in loadedMods.list)
            {
                if (mod.instance == modInstance)
                    return mod;
            }

            return null;
        }

        internal static ModEntry GetModEntryFromModName(string modName)
        {
            foreach(var mod in loadedMods.list)
            {
                if(mod.info.Name == modName)
                {
                    return mod;
                }
            }

            return null;
        }

        internal static string GetSettingsFileLocation()
        {
            return GameIO.GetApplicationPath() + "/" + modSettingsFilename;
        }

        internal static void CheckForUndetectedMods()
        {
            DictionaryList<string, Mod> mmloadedMods = (DictionaryList<string, Mod>)MOD_MANAGER_LOADED_MODS_FIELD.GetValue(null);

            foreach(var modEntry in mmloadedMods.dict)
            {
                string modName = modEntry.Key;
                Mod mod = modEntry.Value;

                if(!loadedMods.dict.ContainsKey(modName))
                {
                    loadedMods.Add(modName, new ModEntry(GetModFolderName(mod.Path), mod, mod.ApiInstance is ModManagerMod ? ModEntry.EModDisableState.Disallowed : ModEntry.EModDisableState.Disallowed_Preloaded));
                }
            }
        }

        public static List<ModEntry> GetLoadedMods()
        {
            return loadedMods.list;
        }

        private static string GetModFolderName(string path)
        {
            return path.Substring(modPath.Length).Trim('\\', '/');
        }

        private static bool LoadMod(string path)
        {
            string str = path + "/ModInfo.xml";
            
            if(!File.Exists(str))
            {
                return true;
            }

            List<ModInfo.ModInfo> xml = ModInfo.ModInfoLoader.ParseXml(str, File.ReadAllText(str));

            if (xml.Count != 1)
                return true;

            ModInfo.ModInfo modInfo = xml[0];

            if (string.IsNullOrEmpty(modInfo.Name.Value))
                return true;

            if (ModManager.ModLoaded(modInfo.Name.Value))
                return true;

            if(disabledMods.Contains(GetModFolderName(path)))
            {
                loadedMods.Add(modInfo.Name.Value, new ModEntry(GetModFolderName(path), modInfo));

                Log.Out($"[Mod Manager] Disabled mod {modInfo.Name.Value} - disregard warning below.");

                return false;
            }

            return true;
        }

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(Mod))]
            [HarmonyPatch(nameof(Mod.LoadFromFolder))]
            class ModLoadFromFolderHook
            {
                static bool Prefix(string _path)
                {
                    return LoadMod(_path);
                }

                static void Postfix(Mod __result)
                {
                    if (__result == null)
                        return;

                    loadedMods.Add(__result.ModInfo.Name.Value, new ModEntry(GetModFolderName(__result.Path), __result, ModEntry.EModDisableState.Allowed));
                }
            }

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
