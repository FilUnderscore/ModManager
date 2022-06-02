using System;
using UnityEngine;

namespace CustomModManager.UI
{
    public class XUiC_Mods : XUiController
    {
        public static string ID = "";

        private ModEntry currentMod = null;

        public XUiC_ModsList modsList;
        private XUiC_TabSelector modTabs;
        private XUiC_ModsListModInfo modInfo;
        private XUiC_ModsListModSettings modSettings;

        public override void Init()
        {
            base.Init();
            XUiC_Mods.ID = this.WindowGroup.ID;

            modsList = (XUiC_ModsList)this.GetChildById("mods");
            modsList.SelectionChanged += ModsList_SelectionChanged;
            
            modTabs = (XUiC_TabSelector)this.GetChildById("modTabs");
            modTabs.OnTabChanged += ModTabs_OnTabChanged;

            modInfo = (XUiC_ModsListModInfo)this.GetChildById("modInfo");
            modInfo.mods = this;

            modSettings = (XUiC_ModsListModSettings)this.GetChildById("modSettings");
            modSettings.mods = this;
            
            ((XUiC_SimpleButton)this.GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
        }

        internal void ModEnabledToggle()
        {
            modsList.IsDirty = true;
        }

        private void ModsList_SelectionChanged(XUiC_ListEntry<XUiC_ModsList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ModsList.ListEntry> _newEntry)
        {
            currentMod = _newEntry == null ? null : _newEntry.GetEntry().modEntry;
            modInfo.currentModEntry = currentMod;
            modSettings.currentModEntry = currentMod;

            modTabs.ViewComponent.IsVisible = currentMod != null;
            modSettings.ViewComponent.IsVisible = currentMod != null ? ModManagerModSettings.modSettingsInstances.ContainsKey(currentMod) : false; // TODO..
            modTabs.GetTabButton(1).Enabled = modSettings.ViewComponent.IsVisible;

            if(!modSettings.ViewComponent.IsVisible)
            {
                modTabs.SelectedTabIndex = 0;
            }

            modInfo.RefreshBindings(true);
            modInfo.UpdateView();

            modSettings.RefreshBindings(true);
            modSettings.UpdateView(modTabs.SelectedTabIndex == 1);
        }

        private void ModTabs_OnTabChanged(int tabIndex, string tabName)
        {
            modInfo.RefreshBindings(true);
            modInfo.UpdateView();

            modSettings.RefreshBindings(true);
            modSettings.UpdateView(tabIndex == 1);
        }

        private void BtnBack_OnPressed(XUiController _sender, int mouseButton)
        {
            this.xui.playerUI.windowManager.Close(this.windowGroup.ID);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.windowGroup.isEscClosable = true;

            modTabs.ViewComponent.IsVisible = false;
        }

        public override void OnClose()
        {
            base.OnClose();
            
            if (CustomModManager.Save())
            {
                // Open dialog informing that changes will be applied on game restart.
                XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(this.xui, Localization.Get("xuiModsListChanged"), Localization.Get("xuiModsListChangedText"), "", Localization.Get("xuiOk"), null, () => { }, true, false);
            }
            else
            {
                this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
            }
        }
    }
}
