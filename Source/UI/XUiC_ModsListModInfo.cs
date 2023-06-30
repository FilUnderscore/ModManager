using UnityEngine;
using CustomModManager.Mod;

namespace CustomModManager.UI
{
    public class XUiC_ModsListModInfo : XUiController
    {
        public Mod.Mod currentModEntry = null;

        internal XUiC_Mods mods;

        private XUiC_ToggleButton enabledButton;
        private XUiC_SimpleButton websiteButton;
        private XUiC_SimpleButton folderButton;

        public override void Init()
        {
            base.Init();

            enabledButton = (XUiC_ToggleButton)this.GetChildById("enableDisableButton");
            enabledButton.OnValueChanged += EnabledButton_OnValueChanged;

            websiteButton = (XUiC_SimpleButton) this.GetChildById("websiteButton");
            websiteButton.OnPressed += WebsiteButton_OnPressed;

            folderButton = (XUiC_SimpleButton) this.GetChildById("folderButton");
            folderButton.OnPressed += FolderButton_OnPressed;
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
            this.mods.ModEnabledToggle();
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
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch (_bindingName)
            {
                case "modName":
                    _value = currentModEntry != null ? currentModEntry.Info.Name : "";
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
                default:
                    _value = "";
                    return false;
            }
        }
    }
}
