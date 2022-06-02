using CustomModManager.UI;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CustomModManager
{
    public class ModLoader
    {
        internal static Harmony harmony;
        internal static bool mainMenuLoaded = false;

        private static readonly PropertyInfo MOD_MANAGER_MOD_PATH_PROPERTY = AccessTools.DeclaredProperty(typeof(ModManager), "ModsBasePath");
        private static readonly FieldInfo MOD_MANAGER_MOD_PATH_LEGACY_FIELD = AccessTools.DeclaredField(typeof(ModManager), "ModsBasePathLegacy");

        internal static readonly string modPath = (string)MOD_MANAGER_MOD_PATH_PROPERTY.GetValue(null);
        internal static string[] modPaths = constructDefaultModPaths();

        private static string[] constructDefaultModPaths()
        {
            List<string> paths = new List<string>();

            if (MOD_MANAGER_MOD_PATH_PROPERTY != null)
                paths.Add((string)MOD_MANAGER_MOD_PATH_PROPERTY.GetValue(null));

            if (MOD_MANAGER_MOD_PATH_LEGACY_FIELD != null)
                paths.Add((string)MOD_MANAGER_MOD_PATH_LEGACY_FIELD.GetValue(null));

            return paths.ToArray();
        }

        private static readonly FieldInfo MOD_MANAGER_LOADED_MODS_FIELD = AccessTools.DeclaredField(typeof(ModManager), "loadedMods");
        private static readonly DictionaryList<string, ModEntry> mods = new DictionaryList<string, ModEntry>();

        private static bool loading = false;
        private static readonly List<string> disabledMods = new List<string>();
        internal const string disabledModsFilename = "disabled-mods.txt";

        internal static void CheckForPreloadedMods()
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

        private static void CheckDisabledMods()
        {
            string file = modPath + "/" + disabledModsFilename;
            
            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (var line in lines)
                {
                    disabledMods.Add(line);
                }
            }
        }

        private static Dictionary<string, ModLoadInfo> DetectMods()
        {
            if(!Directory.Exists(modPath))
                Directory.CreateDirectory(modPath);

            CheckDisabledMods();

            List<string> mods = new List<string>();

            foreach (var modPath in modPaths)
            {
                if (Directory.Exists(modPath))
                    mods.AddRange(Directory.GetDirectories(modPath));
            }

            Dictionary<string, ModLoadInfo> detectedEntries = new Dictionary<string, ModLoadInfo>();

            foreach (var modDir in mods)
            {
                ModInfo.ModInfo info = LoadModInfoFromXml(modDir);

                if (info == null || ModLoader.mods.dict.ContainsKey(info.Name.Value))
                    continue;

                detectedEntries.Add(info.Name.Value, new ModLoadInfo(info, ModManifestFromXml.FromXml(info.Name.Value, modDir), modDir)
                {
                    load = !disabledMods.Contains(Path.GetFileName(modDir))
                });
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
                        Log.Error($"[Mod Loader] Failed to load {detectedMod.modInfo.Name.Value} due to the following dependencies failing to load: " + failedToLoad.StringFromList(dep => dep.parent.modInfo.Name.Value));
                        detectedMod.load = false;
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

        private static void StartLoading()
        {
            harmony.Unpatch(typeof(ThreadManager).GetMethod("RunCoroutineSync"), HarmonyPatchType.Prefix, "filunderscore.modmanager");

            loading = true;

            LoadMods();

            loading = false;
        }

        private static ModInfo.ModInfo LoadModInfoFromXml(string path)
        {
            path += "/ModInfo.xml";

            try
            {
                List<ModInfo.ModInfo> xml = ModInfo.ModInfoLoader.ParseXml(path, File.ReadAllText(path));

                if (xml.Count != 1)
                    return null;

                return xml[0];
            }
            catch(Exception ex)
            {
                Log.Error($"[Mod Loader] Failed to load ModInfo.xml from path {path}.");
                Log.Exception(ex);
                return null;
            }
        }

        public static List<ModEntry> GetLoadedMods()
        {
            return mods.list;
        }

        public static List<ModEntry> GetActiveMods()
        {
            return mods.list.Where(mod => mod.IsLoaded()).ToList();
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

            [HarmonyPatch(typeof(XUiC_MainMenu))]
            [HarmonyPatch("Open")]
            class XUiC_MainMenuOpenHook
            {
                static void Postfix(XUi _xuiInstance)
                {
                    if (mainMenuLoaded)
                        return;

                    mainMenuLoaded = true;
                    Dictionary<ModEntry, List<ModLoadInfo.ModLoadDependency>> dependencies = new Dictionary<ModEntry, List<ModLoadInfo.ModLoadDependency>>();

                    foreach(var mod in ModLoader.GetLoadedMods())
                    {
                        if (!mod.loadInfo.load)
                        {
                            List<ModLoadInfo.ModLoadDependency> failedToLoad = mod.loadInfo.dependencies.FindAll(dep => !dep.success || !dep.parent.load);

                            if(failedToLoad.Any())
                                dependencies.Add(mod, failedToLoad);
                        }
                    }

                    if(dependencies.Count > 0)
                    {
                        string str = "";

                        foreach(var dependency in dependencies)
                        {
                            List<ModLoadInfo.ModLoadDependency> disabled = dependency.Value.Where(dep => dep.success && !dep.parent.load).ToList();
                            List<ModLoadInfo.ModLoadDependency> missing = dependency.Value.Where(dep => !dep.success).ToList();

                            string dependencyStr = "";
                            if(disabled.Any())
                                dependencyStr += string.Format(Localization.Get("xuiModDependencyDisabled"), disabled.StringFromList(dep => dep.parent.modInfo.Name.Value));

                            if (disabled.Any() && missing.Any())
                                dependencyStr += ";";

                            if(missing.Any())
                                dependencyStr += string.Format(Localization.Get("xuiModDependencyMissing"), missing.StringFromList(dep => dep.parentName));
    
                            str += string.Format(Localization.Get("xuiModDependencyMissingEntry"), dependency.Key.info.Name.Value, dependencyStr);
                        }

                        str = string.Format(Localization.Get("xuiModDependenciesMissing"), str);

                        XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(_xuiInstance, Localization.Get("xuiGameModError"), str, "", Localization.Get("btnOk"), null, null, true, false);
                    }
                }
            }
        }
    }
}
