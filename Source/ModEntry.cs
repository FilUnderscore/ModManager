using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public class ModEntry
    {
        internal readonly string folderName;
        public readonly Mod instance;
        public readonly ModInfo.ModInfo info;

        private readonly EModDisableState modDisableState;
        private readonly bool isLoaded = false;
        private bool? willBeEnabled = null;

        public readonly ModManifest manifest;

        public ModEntry(string folderName, ModInfo.ModInfo info)
        {
            this.folderName = folderName;
            this.info = info;
            this.manifest = ModManifestFromXml.FromXml(this);
        }

        public ModEntry(string folderName, Mod mod, EModDisableState modDisableState) : this(folderName, mod.ModInfo)
        {
            this.instance = mod;

            this.isLoaded = true;

            this.modDisableState = modDisableState;
        }

        public bool IsLoaded()
        {
            return this.isLoaded;
        }

        public bool? WillBeEnabled()
        {
            return this.willBeEnabled;
        }

        public void Toggle()
        {
            if (this.willBeEnabled == null)
                this.willBeEnabled = !this.isLoaded;
            else
                this.willBeEnabled = !this.willBeEnabled;
        }

        public bool HasBeenChanged()
        {
            return this.willBeEnabled != null && this.willBeEnabled.Value != this.isLoaded;
        }

        public EModDisableState GetModDisableState()
        {
            return this.modDisableState;
        }

        public enum EModDisableState
        {
            Allowed,
            Disallowed,
            Disallowed_Preloaded
        }

        public string GetModDisableStateReason()
        {
            switch (this.modDisableState)
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
