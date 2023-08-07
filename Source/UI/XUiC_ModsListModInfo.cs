using UnityEngine;
using CustomModManager.Mod;
using System.IO;
using CustomModManager.UI.Wrappers;
using static CustomModManager.UI.Wrappers.XUiW_Texture;

namespace CustomModManager.UI
{
    public class XUiC_ModsListModInfo : XUiController
    {
        public Mod.Mod currentModEntry = null;

        internal XUiC_Mods mods;

        private XUiC_ToggleButton enabledButton;
        private XUiC_SimpleButton websiteButton;
        private XUiC_SimpleButton folderButton;

        private XUiV_Label NameLabel, AuthorLabel, VersionLabel, DescriptionLabel;
        private Vector2i OriginalNameLabelPosition, OriginalAuthorLabelPosition, OriginalVersionLabelPosition, OriginalDescriptionLabelPosition;

        private XUiW_Texture BannerTexture;

        public override void Init()
        {
            base.Init();

            enabledButton = (XUiC_ToggleButton)this.GetChildById("enableDisableButton");
            enabledButton.OnValueChanged += EnabledButton_OnValueChanged;

            websiteButton = (XUiC_SimpleButton) this.GetChildById("websiteButton");
            websiteButton.OnPressed += WebsiteButton_OnPressed;

            folderButton = (XUiC_SimpleButton) this.GetChildById("folderButton");
            folderButton.OnPressed += FolderButton_OnPressed;

            NameLabel = (XUiV_Label)this.GetChildById("Name").ViewComponent;
            OriginalNameLabelPosition = NameLabel.Position;

            AuthorLabel = (XUiV_Label)this.GetChildById("Author").ViewComponent;
            OriginalAuthorLabelPosition = AuthorLabel.Position;

            VersionLabel = (XUiV_Label)this.GetChildById("Version").ViewComponent;
            OriginalVersionLabelPosition = VersionLabel.Position;

            DescriptionLabel = (XUiV_Label)this.GetChildById("Description").ViewComponent;
            OriginalDescriptionLabelPosition = DescriptionLabel.Position;

            BannerTexture = new XUiW_Texture(this, "bannerTexture");
        }

        private void FolderButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (currentModEntry == null)
                return;

            Application.OpenURL(currentModEntry.Info.Path);
        }

        private void EnabledButton_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
        {
            if (currentModEntry == null)
                return;

            if(currentModEntry.GetModDisableState() != EModDisableState.Allowed)
            {
                enabledButton.Value = true;
                return;
            }

            currentModEntry.ToggleNextState();
            this.mods.modsList.IsDirty = true;
        }

        private void WebsiteButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (currentModEntry == null || currentModEntry.Info.Website == null)
                return;

            Application.OpenURL(currentModEntry.Info.Website);
        }

        internal void UpdateView()
        {
            enabledButton.Value = currentModEntry != null ? currentModEntry.NextState : false;
            enabledButton.Tooltip = currentModEntry != null ? (currentModEntry.GetModDisableState() != EModDisableState.Allowed ? currentModEntry.GetModDisableStateReason() : "") : "";

            websiteButton.Enabled = currentModEntry != null ? (!string.IsNullOrEmpty(currentModEntry.Info.Website)) : false;
            websiteButton.Tooltip = currentModEntry != null ? (!string.IsNullOrEmpty(currentModEntry.Info.Website) ? currentModEntry.Info.Website : "") : "";

            folderButton.Enabled = currentModEntry != null;

            // Update banner texture

            bool banner = false;

            if (this.currentModEntry != null)
            {
                if (banner = this.currentModEntry.TryGetBannerImage(out IXUiTexture bannerImage))
                    this.BannerTexture.SetTexture(bannerImage);
            }

            int banner_y_offset = banner ? this.BannerTexture.GetHeight() + 5 : 0;
            Vector2i banner_y_offset_v2i = new Vector2i(0, banner_y_offset);

            // Update label positions
            NameLabel.Position = OriginalNameLabelPosition - banner_y_offset_v2i;
            AuthorLabel.Position = OriginalAuthorLabelPosition - banner_y_offset_v2i;
            VersionLabel.Position = OriginalVersionLabelPosition - banner_y_offset_v2i;
            DescriptionLabel.Position = OriginalDescriptionLabelPosition - banner_y_offset_v2i;
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch (_bindingName)
            {
                case "modName":
                    _value = currentModEntry != null ? currentModEntry.Info.DisplayName : "";
                    return true;
                case "modAuthor":
                    _value = currentModEntry != null ? currentModEntry.Info.Author : "";
                    return true;
                case "modVersion":
                    _value = currentModEntry != null ? currentModEntry.Version.ToString() : "";
                    return true;
                case "modDescription":
                    _value = currentModEntry != null ? currentModEntry.Info.Description : "";
                    return true;
                case "hasModBanner":
                    _value = currentModEntry != null ? this.currentModEntry.TryGetBannerImage(out _).ToString() : false.ToString();
                    return true;
                default:
                    _value = "";
                    return false;
            }
        }
    }
}
