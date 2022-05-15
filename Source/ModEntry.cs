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
        internal Mod instance;

        private bool? willBeEnabled;
        internal bool flag;
        internal EModDisableState disableState;

        public ModInfo.ModInfo info => loadInfo.modInfo;
        public ModManifest manifest => loadInfo.manifest;

        public ModEntry(ModLoadInfo loadInfo)
        {
            this.loadInfo = loadInfo;
        }

        public bool IsLoaded()
        {
            return this.loadInfo.load;
        }

        public bool? WillBeEnabled()
        {
            return this.willBeEnabled;
        }

        public void Toggle()
        {
            if (this.willBeEnabled == null)
                this.willBeEnabled = !this.IsLoaded();
            else
                this.willBeEnabled = !this.willBeEnabled;

            this.flag = true;
        }

        public EModDisableState GetModDisableState()
        {
            return this.disableState;
        }

        public enum EModDisableState
        {
            Allowed,
            Disallowed,
            Disallowed_Preloaded
        }

        public string GetModDisableStateReason()
        {
            switch (this.disableState)
            {
                case EModDisableState.Allowed:
                    return "";
                case EModDisableState.Disallowed:
                    return Localization.Get("xuiModDisableStateReasonDisallowed");
                case EModDisableState.Disallowed_Preloaded:
                    return Localization.Get("xuiModDisableStateReasonDisallowedPreloaded");
                default:
                    return "";
            }
        }

        public string GetVersion()
        {
            return manifest != null && manifest.Version != null ? manifest.Version.ToString() : info.Version.Value;
        }
    }
}
