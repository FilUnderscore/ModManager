using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public class ModLoadInfo : IComparable<ModLoadInfo>
    {
        public readonly ModInfo.ModInfo modInfo;
        public readonly ModManifest manifest;
        public readonly string modPath;
        public List<ModLoadDependency> dependencies = new List<ModLoadDependency>();
        public bool load = true;

        public ModLoadInfo(ModInfo.ModInfo modInfo, ModManifest manifest, string modPath)
        {
            this.modInfo = modInfo;
            this.manifest = manifest;
            this.modPath = modPath;
        }

        public class ModLoadDependency
        {
            public string parentName;
            public readonly bool success;

            public ModLoadInfo parent;
            public ModLoadInfo child;

            public ModLoadDependency(ModLoadInfo child, string parentName)
            {
                this.child = child;
                this.parentName = parentName;
                this.success = false;
            }

            public ModLoadDependency(ModLoadInfo child, ModLoadInfo parent)
            {
                this.child = child;
                this.parent = parent;
                this.parent.dependencies.Add(this);
                this.success = true;
            }
        }

        public int CompareTo(ModLoadInfo other)
        {
            if (other == null)
                return 0;

            if (dependencies.Any(dep => dep.child == other))
                return -1;

            if (!dependencies.Any(dep => dep.parent == other))
                return 1;

            return 0;
        }
    }
}
