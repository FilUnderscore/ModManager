using UnityEngine;

using HarmonyLib;
using System.Reflection;

namespace CustomModManager.UI
{
    public class XUiC_ModsListModInfo : XUiController
    {
        public ModEntry currentModEntry = null;

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

            Application.OpenURL(CustomModManager.GetModEntryFolderLocation(currentModEntry));
        }

        private void EnabledButton_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
        {
            if (currentModEntry == null)
                return;

            if(currentModEntry.GetModDisableState() != ModEntry.EModDisableState.Allowed)
            {
                enabledButton.Value = true;
                return;
            }

            currentModEntry.Toggle();
            this.mods.ModEnabledToggle();
        }

        private void WebsiteButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (currentModEntry == null || currentModEntry.info.Website == null)
                return;

            Application.OpenURL(currentModEntry.info.Website.Value);
        }

        internal void UpdateView()
        {
            enabledButton.Value = currentModEntry != null ? (currentModEntry.WillBeEnabled() != null ? currentModEntry.WillBeEnabled().Value : currentModEntry.IsLoaded()) : false;
            enabledButton.Tooltip = currentModEntry != null ? (currentModEntry.GetModDisableState() != ModEntry.EModDisableState.Allowed ? currentModEntry.GetModDisableStateReason() : "") : "";

            websiteButton.Enabled = currentModEntry != null ? (currentModEntry.info.Website != null) : false;
            websiteButton.Tooltip = currentModEntry != null ? (currentModEntry.info.Website != null ? currentModEntry.info.Website.Value : "") : "";

            folderButton.Enabled = currentModEntry != null;
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch (_bindingName)
            {
                case "modName":
                    _value = currentModEntry != null ? currentModEntry.info.Name.Value : "";
                    return true;
                case "modAuthor":
                    _value = currentModEntry != null ? currentModEntry.info.Author.Value : "";
                    return true;
                case "modVersion":
                    _value = currentModEntry != null ? currentModEntry.info.Version.Value : "";
                    return true;
                case "modDescription":
                    _value = currentModEntry != null ? currentModEntry.info.Description.Value : "";
                    return true;
                default:
                    _value = "";
                    return false;
            }
        }
    }
}
