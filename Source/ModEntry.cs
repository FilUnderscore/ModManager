using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public class ModEntry
    {
        public readonly ModLoadInfo loadInfo;
        public readonly Mod instance;

        public ModEntry(ModLoadInfo loadInfo, Mod modInstance)
        {
            this.loadInfo = loadInfo;
            this.instance = modInstance;
        }
    }
}
