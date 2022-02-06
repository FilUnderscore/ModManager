using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomModManager
{
    public class ModLoader
    {
        internal static Harmony harmony;

        private static readonly FieldInfo MOD_MANAGER_MOD_PATH_FIELD = AccessTools.DeclaredField(typeof(ModManager), "MOD_PATH");

        private static readonly string modPath = (string)MOD_MANAGER_MOD_PATH_FIELD.GetValue(null);

        private static readonly FieldInfo MOD_MANAGER_LOADED_MODS_FIELD = AccessTools.DeclaredField(typeof(ModManager), "loadedMods");
        private static readonly DictionaryList<string, ModEntry> mods = new DictionaryList<string, ModEntry>();

        private static bool loading = false;

        internal static void CheckForUndetectedMods()
        {
            DictionaryList<string, Mod> mmloadedMods = (DictionaryList<string, Mod>)MOD_MANAGER_LOADED_MODS_FIELD.GetValue(null);

            foreach (var modEntry in mmloadedMods.dict)
            {
                string modName = modEntry.Key;
                Mod mod = modEntry.Value;

                if (!mods.dict.ContainsKey(modName) && !modName.EndsWith(":Ignore"))
                {
                    mods.Add(modName, new ModEntry(new ModLoadInfo(mod.ModInfo, ModManifestFromXml.FromXml(mod.ModInfo.Name.Value, mod.Path), mod.Path))
                    {
                        instance = mod,
                        disableState = mod.ApiInstance is ModManagerMod ? ModEntry.EModDisableState.Disallowed : ModEntry.EModDisableState.Disallowed_Preloaded
                    });
                }
            }
        }

        internal static string GetModFolderName(string path)
        {
            return path.Substring(modPath.Length).Trim('\\', '/');
        }

        private static Dictionary<string, ModLoadInfo> DetectMods()
        {
            string[] mods = Directory.GetDirectories(modPath);

            Dictionary<string, ModLoadInfo> detectedEntries = new Dictionary<string, ModLoadInfo>();

            foreach (var modDir in mods)
            {
                ModInfo.ModInfo info = LoadModInfoFromXml(modDir);

                if (info == null || ModLoader.mods.dict.ContainsKey(info.Name.Value))
                    continue;

                detectedEntries.Add(info.Name.Value, new ModLoadInfo(info, ModManifestFromXml.FromXml(info.Name.Value, modDir), modDir));
            }

            return detectedEntries;
        }

        private static void LoadMods()
        {
            Dictionary<string, ModLoadInfo> detectedMods = DetectMods();

            foreach (var detectedMod in detectedMods.Values)
            {
                SortDependencies(detectedMod, detectedMods);
                mods.Add(detectedMod.modInfo.Name.Value, new ModEntry(detectedMod));
            }

            List<ModLoadInfo> sortedMods = new List<ModLoadInfo>(detectedMods.Values);
            sortedMods.Sort();

            DictionaryList<string, Mod> modManagerLoadedMods = (DictionaryList<string, Mod>) MOD_MANAGER_LOADED_MODS_FIELD.GetValue(null);

            foreach (var detectedMod in sortedMods)
            {
                if (detectedMod.load)
                {
                    List<ModLoadInfo.ModLoadDependency> failedToLoad = detectedMod.dependencies.FindAll(dep => !dep.parent.load);
                    
                    if(failedToLoad.Any())
                    {
                        Log.Error($"[Mod Loader] Failed to load {detectedMod.modInfo.Name.Value} due to the following dependencies failing to load: " + failedToLoad.StringFromList(dep => dep.parentName));
                        continue;
                    }

                    if(LoadMod(detectedMod, out Mod mod))
                    {
                        mods.dict[detectedMod.modInfo.Name.Value].instance = mod;

                        modManagerLoadedMods.Remove(detectedMod.modPath + ":Ignore");
                        modManagerLoadedMods.Add(mod.ModInfo.Name.Value, mod);
                    }

                    detectedMod.load = mod != null;
                }
            }

            foreach (var detectedMod in sortedMods)
            {
                if (detectedMod.load)
                {
                    detectedMod.load = InitMod(detectedMod);
                }
            }
        }

        private static void SortDependencies(ModLoadInfo modLoadInfo, Dictionary<string, ModLoadInfo> mods)
        {
            if (modLoadInfo.manifest == null || modLoadInfo.manifest.Dependencies == null)
                return;

            foreach(var dependency in modLoadInfo.manifest.Dependencies)
            {
                if (mods.ContainsKey(dependency))
                    modLoadInfo.dependencies.Add(new ModLoadInfo.ModLoadDependency(modLoadInfo, mods[dependency]));
                else
                    modLoadInfo.dependencies.Add(new ModLoadInfo.ModLoadDependency(modLoadInfo, dependency));
            }

            List<string> missingDependencies = new List<string>();

            modLoadInfo.dependencies.ForEach(dep =>
            {
                if (!dep.success)
                    missingDependencies.Add(dep.parentName);
            });

            if(missingDependencies.Count > 0)
            {
                modLoadInfo.load = false;
                Log.Error($"[Mod Loader] Could not load {modLoadInfo.modInfo.Name.Value} due to the following missing dependencies: {missingDependencies.StringFromList()}");
            }
        }

        private static bool LoadMod(ModLoadInfo modLoadInfo, out Mod modInstance)
        {
            Log.Out($"[Mod Loader] Loading {modLoadInfo.modInfo.Name.Value}");

            Mod mod = null;

            try
            {
                mod = Mod.LoadFromFolder(modLoadInfo.modPath);

                if (mod == null)
                {
                    Log.Warning($"[Mod Loader] Failed to load {modLoadInfo.modInfo.Name.Value} from folder {modLoadInfo.modPath}.");
                    modInstance = null;
                }
            }
            catch (Exception ex)
            {
                if (mod == null)
                {
                    Log.Error($"[Mod Loader] Failed to load {modLoadInfo.modInfo.Name.Value} from folder {modLoadInfo.modPath}.");
                    Log.Exception(ex);
                }
            }

            modInstance = mod;
            return mod != null;
        }

        private static bool InitMod(ModLoadInfo modLoadInfo)
        {
            Mod mod = mods.dict[modLoadInfo.modInfo.Name.Value].instance;

            if (mod.ApiInstance != null)
            {
                try
                {
                    mod.ApiInstance.InitMod(mod);
                }
                catch (Exception ex)
                {
                    Log.Error($"[Mod Loader] Failed initializing IModApi instance from mod {mod.ModInfo.Name.Value} from DLL {Path.GetFileName(mod.MainAssembly.Location)}");
                    Log.Exception(ex);
                    return false;
                }
            }

            return true;
        }

        internal static void StartLoading()
        {
            harmony.Unpatch(typeof(ThreadManager).GetMethod("RunCoroutineSync"), HarmonyPatchType.Prefix, "filunderscore.modmanager");

            loading = true;

            LoadMods();

            loading = false;
        }

        private static ModInfo.ModInfo LoadModInfoFromXml(string path)
        {
            path += "/ModInfo.xml";

            List<ModInfo.ModInfo> xml = ModInfo.ModInfoLoader.ParseXml(path, File.ReadAllText(path));

            if (xml.Count != 1)
                return null;

            return xml[0];
        }

        public static List<ModEntry> GetLoadedMods()
        {
            return mods.list;
        }

        internal static ModEntry GetModEntryFromModInstance(Mod modInstance)
        {
            foreach(var mod in mods.list)
            {
                if (mod.instance == modInstance)
                    return mod;
            }

            return null;
        }

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(Mod))]
            [HarmonyPatch(nameof(Mod.LoadFromFolder))]
            class ModLoadFromFolderHook
            {
                static bool Prefix(string _path, ref Mod __result)
                {
                    if(!loading)
                    {
                        __result = new Mod();
                        typeof(Mod).GetProperty("ModInfo").SetValue(__result, new ModInfo.ModInfo()
                        {
                            Name = new DataItem<string>("Name", _path + ":Ignore")
                        });
                    }

                    return loading;
                }
            }

            [HarmonyPatch(typeof(ThreadManager))]
            [HarmonyPatch(nameof(ThreadManager.RunCoroutineSync))]
            class ModManagerLoadPatchStuffHook
            {
                static void Prefix()
                {
                    StartLoading();
                }
            }
        }
    }
}
