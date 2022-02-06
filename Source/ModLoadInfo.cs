using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public class ModLoadInfo
    {
        public ModInfo.ModInfo modInfo;
        public ModManifest manifest;
        public int loadIndex;
        public List<ModLoadDependency> dependencies;

        public EModLoadState loadState;

        public enum EModLoadState
        {
            Loaded,
            Unloaded,
            Error,
            Missing_Dependencies
        }

        public class ModLoadDependency
        {
            public ModLoadInfo parent;
            public ModLoadInfo child;
        }
    }
}
