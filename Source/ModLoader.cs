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
        private static readonly DictionaryList<string, ModEntry> loadedMods = new DictionaryList<string, ModEntry>();

        private static bool loading = false;

        internal static void CheckForUndetectedMods()
        {
            DictionaryList<string, Mod> mmloadedMods = (DictionaryList<string, Mod>)MOD_MANAGER_LOADED_MODS_FIELD.GetValue(null);

            foreach (var modEntry in mmloadedMods.dict)
            {
                string modName = modEntry.Key;
                Mod mod = modEntry.Value;

                if (!loadedMods.dict.ContainsKey(modName) && !modName.EndsWith(":Ignore"))
                {
                    loadedMods.Add(modName, new ModEntry(GetModFolderName(mod.Path), mod, mod.ApiInstance is ModManagerMod ? ModEntry.EModDisableState.Disallowed : ModEntry.EModDisableState.Disallowed_Preloaded, ModManifestFromXml.FromXml(mod.ModInfo.Name.Value, mod.Path)));
                }
            }
        }

        internal static string GetModFolderName(string path)
        {
            return path.Substring(modPath.Length).Trim('\\', '/');
        }

        internal static void StartLoading()
        {
            harmony.Unpatch(typeof(ThreadManager).GetMethod("RunCoroutineSync"), HarmonyPatchType.Prefix, "filunderscore.modmanager");

            loading = true;

            string[] mods = Directory.GetDirectories(modPath);

            Dictionary<string, ModLoadEntry> detectedEntries = new Dictionary<string, ModLoadEntry>();
            int loadIndex = 0;
            foreach(var modDir in mods)
            {
                ModInfo.ModInfo info = LoadModInfoFromXml(modDir);

                if (info == null || loadedMods.dict.ContainsKey(info.Name.Value))
                    continue;

                detectedEntries.Add(info.Name.Value, new ModLoadEntry
                {
                    path = modDir,
                    info = info,
                    manifest = ModManifestFromXml.FromXml(info.Name.Value, modDir),
                    loadOrder = loadIndex++
                });
            }

            foreach(var modLoadEntry in detectedEntries)
            {
                var modEntry = modLoadEntry.Value;

                if (modEntry.manifest == null || modEntry.manifest.Dependencies == null)
                    continue;

                List<string> dependencies = modEntry.manifest.Dependencies;
                List<string> missingDependencies = new List<string>(); ;

                for (int index = 0; index < dependencies.Count; index++)
                {
                    string dependency = dependencies[index];
                    
                    if(!detectedEntries.ContainsKey(dependency))
                    {
                        missingDependencies.Add(dependency);

                        continue;
                    }

                    int dependencyLoadIndex = detectedEntries[dependency].loadOrder;

                    if(dependencyLoadIndex > modEntry.loadOrder)
                        modEntry.loadOrder = dependencyLoadIndex + 1;

                    modEntry.dependentOn.Add(detectedEntries[dependency]);
                }

                if(missingDependencies.Count > 0)
                {
                    string dependenciesMissingListStr = "";

                    for(int index = 0; index < missingDependencies.Count; index++)
                    {
                        dependenciesMissingListStr += missingDependencies[index];

                        if (index < missingDependencies.Count - 1)
                            dependenciesMissingListStr += ", ";
                    }

                    Log.Warning($"[Mod Loader] Failed to load mod {modEntry.info.Name.Value} due to the following dependencies missing: " + dependenciesMissingListStr);

                    continue;
                }
            }

            List<ModLoadEntry> loadOrder = new List<ModLoadEntry>(detectedEntries.Values);
            loadOrder.Sort();

            DictionaryList<string, Mod> modManagerLoadedMods = (DictionaryList<string, Mod>) MOD_MANAGER_LOADED_MODS_FIELD.GetValue(null);

            foreach (var entry in loadOrder)
            {
                Log.Out("Loading " + entry.info.Name.Value);

                if(entry.dependentOn.Count(dependency => !dependency.loaded) > 0)
                {
                    Log.Warning($"[Mod Loader] Failed to load mod {entry.info.Name.Value} due to one of its dependencies failing to load.");
                    continue;
                }

                Mod mod = null;

                try
                {
                    mod = Mod.LoadFromFolder(entry.path);

                    if (mod == null)
                    {
                        Log.Warning($"[Mod Loader] Failed to load mod {entry.info.Name.Value} from folder {entry.path}.");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (mod == null)
                    {
                        Log.Error($"[Mod Loader] Failed to load mod {entry.info.Name.Value} from folder {entry.path}.");
                        Log.Exception(ex);
                        continue;
                    }
                }

                loadedMods.Add(mod.ModInfo.Name.Value, new ModEntry(GetModFolderName(entry.path), mod, ModEntry.EModDisableState.Allowed, entry.manifest));
                entry.loaded = true;
                entry.instance = mod;

                modManagerLoadedMods.Remove(entry.path);
                modManagerLoadedMods.Add(mod.ModInfo.Name.Value, mod);
            }

            foreach(var loadedModEntry in loadedMods.dict)
            {
                if (loadedModEntry.Value.GetModDisableState() != ModEntry.EModDisableState.Allowed)
                    continue;

                Mod mod = loadedModEntry.Value.instance;

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
                        continue;
                    }
                }
            }

            loading = false;
        }

        class ModLoadEntry : IComparable<ModLoadEntry>
        {
            public string path;
            public ModInfo.ModInfo info;
            public ModManifest manifest;
            public int loadOrder;
            public bool loaded = false;
            public Mod instance;
            public List<ModLoadEntry> dependentOn = new List<ModLoadEntry>();

            public int CompareTo(ModLoadEntry other)
            {
                if (other == null)
                    return 1;

                return loadOrder.CompareTo(other.loadOrder);
            }
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
            return loadedMods.list;
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
