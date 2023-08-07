using UnityEngine;
using CustomModManager.Mod;
using CustomModManager.UI.Wrappers;
using static CustomModManager.UI.Wrappers.XUiW_Texture;

namespace CustomModManager.UI
{
    public class XUiC_ModsListModInfo : XUiController
    {
        private Mod.Mod mod;

        internal XUiC_Mods mods;

        private XUiC_ToggleButton enabledButton;
        private XUiC_SimpleButton websiteButton;
        private XUiC_SimpleButton folderButton;

        private XUiW_Label nameLabel, authorLabel, versionLabel, descriptionLabel;
        
        private XUiW_Texture bannerTexture;

        public override void Init()
        {
            base.Init();

            this.enabledButton = (XUiC_ToggleButton)this.GetChildById("enableDisableButton");
            this.enabledButton.OnValueChanged += EnabledButton_OnValueChanged;

            this.websiteButton = (XUiC_SimpleButton) this.GetChildById("websiteButton");
            this.websiteButton.OnPressed += WebsiteButton_OnPressed;

            this.folderButton = (XUiC_SimpleButton) this.GetChildById("folderButton");
            this.folderButton.OnPressed += FolderButton_OnPressed;

            this.nameLabel = new XUiW_Label(this, "Name");
            this.authorLabel = new XUiW_Label(this, "Author");
            this.versionLabel = new XUiW_Label(this, "Version");
            this.descriptionLabel = new XUiW_Label(this, "Description");

            this.bannerTexture = new XUiW_Texture(this, "bannerTexture");
        }

        private void FolderButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (mod == null)
                return;

            Application.OpenURL(mod.Info.Path);
        }

        private void EnabledButton_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
        {
            if (mod == null)
                return;

            if(mod.GetModDisableState() != EModDisableState.Allowed)
            {
                enabledButton.Value = true;
                return;
            }

            mod.ToggleNextState();
            this.mods.modsList.IsDirty = true;
        }

        private void WebsiteButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (mod == null || mod.Info.Website == null)
                return;

            Application.OpenURL(mod.Info.Website);
        }

        internal void UpdateView()
        {
            enabledButton.Value = mod != null ? mod.NextState : false;
            enabledButton.Tooltip = mod != null ? (mod.GetModDisableState() != EModDisableState.Allowed ? mod.GetModDisableStateReason() : "") : "";

            websiteButton.Enabled = mod != null ? (!string.IsNullOrEmpty(mod.Info.Website)) : false;
            websiteButton.Tooltip = mod != null ? (!string.IsNullOrEmpty(mod.Info.Website) ? mod.Info.Website : "") : "";

            folderButton.Enabled = mod != null;

            // Update banner texture

            bool banner = false;

            if (this.mod != null)
            {
                if (banner = this.mod.TryGetBannerImage(out IXUiTexture bannerImage))
                    this.bannerTexture.SetTexture(bannerImage);
            }

            int banner_y_offset = banner ? this.bannerTexture.GetHeight() + 5 : 0;
            
            // Update label positions
            this.nameLabel.Offset(0, -banner_y_offset);
            this.authorLabel.Offset(0, -banner_y_offset);
            this.versionLabel.Offset(0, -banner_y_offset);
            this.descriptionLabel.Offset(0, -banner_y_offset);
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch (_bindingName)
            {
                case "modName":
                    _value = mod != null ? mod.Info.DisplayName : "";
                    return true;
                case "modAuthor":
                    _value = mod != null ? mod.Info.Author : "";
                    return true;
                case "modVersion":
                    _value = mod != null ? mod.Version.ToString() : "";
                    return true;
                case "modDescription":
                    _value = mod != null ? mod.Info.Description : "";
                    return true;
                case "hasModBanner":
                    _value = mod != null ? this.mod.TryGetBannerImage(out _).ToString() : false.ToString();
                    return true;
                default:
                    _value = "";
                    return false;
            }
        }

        public void SetCurrentMod(Mod.Mod mod)
        {
            this.mod = mod;

            this.RefreshBindings(true);
            this.UpdateView();
        }
    }
}
