using CustomModManager.Mod.Info.Parser;
using CustomModManager.Mod.Manifest;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomModManager.Mod
{
    public sealed class ModLoader
    {
        private readonly List<Mod> mods = new List<Mod>();
        private readonly ISet<string> disabledModNames = new HashSet<string>();

        public static ModLoader Instance;
        private Mod self;

        public ModLoader()
        {
            Instance = this;
        }

        public void Load(string[] folders)
        {
            foreach(var folder in folders)
            {
                FindMods(folder, out var mods);
                this.mods.AddRange(mods);
            }
        }

        private bool ContainsMod(string modName)
        {
            return this.GetMods(true).Any(mod => mod.Info.Name == modName);
        }

        private void FindMods(string modFolderPath, out List<Mod> mods)
        {
            mods = new List<Mod>();

            if (!Directory.Exists(modFolderPath))
            {
                Directory.CreateDirectory(modFolderPath);
                return;
            }

            var modDirectories = Directory.GetDirectories(modFolderPath);

            foreach(var modDir in modDirectories)
            {
                var parser = ModInfoParserFactory.CreateParser(modDir);

                if(parser == null || !parser.TryParse(out var modInfo) || ContainsMod(modInfo.Name))
                {
                    // failed to parse.
                    continue;
                }

                var manifest = ModManifestFromXml.FromXml(modInfo.Name, modInfo.Path);
                mods.Add(new Mod(modInfo, manifest, EModDisableState.Allowed));
            }
        }

        public List<Mod> GetMods(bool includeSelf = true)
        {
            if(includeSelf)
            {
                List<Mod> mods = this.mods.ToList();
                mods.Add(self);
                return mods;
            }

            return this.mods;
        }

        private Mod ConvertInstanceToMod(global::Mod _modInstance, EModDisableState modDisableState)
        {
            return new Mod(new Info.ModInfo(_modInstance), ModManifestFromXml.FromXml(_modInstance.DisplayName, _modInstance.Path), _modInstance, modDisableState);
        }

        public void SetSelfAndDetectMods(global::Mod _modInstance)
        {
            self = ConvertInstanceToMod(_modInstance, EModDisableState.Disallowed);
            self.initialized = true;

            string file = this.self.Info.Path + "/../" + "disabled-mods.txt";
            ISet<string> disabledModNames = File.Exists(file) ? File.ReadAllLines(file).ToHashSet() : new HashSet<string>();

            foreach(var modName in disabledModNames)
                this.disabledModNames.Add(modName);

            foreach (var loadedMod in global::ModManager.GetLoadedMods())
            {
                if (loadedMod == _modInstance)
                    continue;

                Mod mod = ConvertInstanceToMod(loadedMod, EModDisableState.Allowed);
                this.mods.Add(mod);
            }
        }

        public bool IsModEnabled(Mod mod)
        {
            return !disabledModNames.Contains(mod.Info.Name);
        }

        public Mod GetModFromInstance(global::Mod instance)
        {
            return this.GetMods(true).Find(mod => mod.DoesModInstanceMatch(instance));
        }

        public bool SaveModChanges()
        {
            SaveModSettings();

            string file = this.self.Info.Path + "/../" + "disabled-mods.txt";
            List<string> lines = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();

            bool flag = false;

            foreach(var mod in this.GetMods(false))
            {
                flag |= (mod.WillChangeState && mod.Loaded);

                if (mod.WillChangeState)
                {
                    if (mod.NextState)
                    {
                        lines.RemoveAll(modName => mod.Info.Name.EqualsCaseInsensitive(modName));
                        mod.Load();
                    }
                    else if(!lines.Contains(mod.Info.Name))
                    {
                        lines.Add(Path.GetFileName(mod.Info.Name));
                    }
                }
                else
                {
                    lines.RemoveAll(modName => mod.Info.Name.EqualsCaseInsensitive(modName));
                }
            }

            if (lines.Count > 0)
                File.WriteAllLines(file, lines.ToArray());
            else
                File.Delete(file);

            return flag;
        }

        private static void SaveModSettings()
        {
            ModSettingsFromXml.Save();
        }
    }

    public enum EModDisableState
    {
        Allowed,
        Disallowed,
        Disallowed_Preloaded
    }
}
