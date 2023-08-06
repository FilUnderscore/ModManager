using CustomModManager.Mod.Version;
using System.IO;
using static CustomModManager.UI.Wrappers.XUiW_Texture;

namespace CustomModManager.Mod
{
    public class Mod
    {
        public readonly Info.ModInfo Info;
        public readonly Manifest.ModManifest Manifest;

        private global::Mod instance;

        public IModVersion Version
        {
            get
            {
                return this.Manifest != null ? this.Manifest.Version : this.Info.Version;
            }
        }

        private bool stateSwapFlag;
        private EModDisableState modDisableState;

        public bool Loaded
        {
            get
            {
                return instance != null && initialized || forceloaded;
            }
        }

        protected bool forceloaded = false;
        private bool preloaded = true;
        internal bool initialized = false;

        public bool NextState
        {
            get
            {
                return Loaded ^ stateSwapFlag;
            }
        }

        public bool WillChangeState
        {
            get
            {
                return Loaded != NextState;
            }
        }

        public Mod(Info.ModInfo info, Manifest.ModManifest manifest, EModDisableState modDisableState)
        {
            this.Info = info;
            this.Manifest = manifest;
            this.modDisableState = modDisableState;
            this.preloaded = false;
        }

        public Mod(Info.ModInfo info, Manifest.ModManifest manifest, global::Mod instance, EModDisableState modDisableState)
        {
            this.Info = info;
            this.Manifest = manifest;
            this.instance = instance;
            this.modDisableState = modDisableState;
        }

        public EModDisableState GetModDisableState()
        {
            if (this.modDisableState == EModDisableState.Disallowed)
                return this.modDisableState;
            else
            {
                if (this.preloaded)
                    return EModDisableState.Disallowed_Preloaded;
                else
                    return EModDisableState.Allowed;
            }
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

        public void ToggleNextState()
        {
            this.stateSwapFlag = !this.stateSwapFlag;
        }

        public bool Load()
        {
            if (this.forceloaded)
                return true;

            if (this.instance == null)
            {
                this.instance = global::Mod.LoadFromFolder(this.Info.Path);
                ThreadManager.RunCoroutineSync(ModManager.LoadPatchStuff(false));
            }

            if (this.stateSwapFlag)
                this.stateSwapFlag = false;

            preloaded = false;

            return this.Loaded || (this.instance != null && (this.initialized = this.Initialize()));
        }

        private bool Initialize()
        {
            if (this.Loaded)
                return true;

            return this.instance.InitModCode();
        }

        public bool DoesModInstanceMatch(global::Mod mod)
        {
            if (!this.Loaded)
                return false;

            return this.instance == mod;
        }

        protected string GetModFolderPath(string subpath)
        {
            return $"@modfolder({this.Info.Name}):{subpath}";
        }

        protected bool TryGetModFolderPath(string subpath, out string path)
        {
            path = GetModFolderPath(subpath);
            return File.Exists(ModManager.PatchModPathString(path));
        }

        public virtual bool TryGetIconImage(out IXUiTexture texture)
        {
            if(!TryGetModFolderPath("icon.png", out string iconImagePath))
            {
                texture = null;
                return false;
            }

            texture = new XUiTexturePath(iconImagePath);
            return true;
        }

        public virtual bool TryGetBannerImage(out IXUiTexture texture)
        {
            if (!TryGetModFolderPath("banner.png", out string iconImagePath))
            {
                texture = null;
                return false;
            }

            texture = new XUiTexturePath(iconImagePath);
            return true;
        }
    }
}
