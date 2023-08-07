using CustomModManager.Mod;

namespace CustomModManager.UI
{
    public class XUiC_Mods : XUiController
    {
        public static string ID = "";

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
            modSettings.settingsTabButton = modTabs.GetTabButton(1);
            
            ((XUiC_SimpleButton)this.GetChildById("btnBack")).OnPressed += BtnBack_OnPressed;
        }

        private void ModsList_SelectionChanged(XUiC_ListEntry<XUiC_ModsList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ModsList.ListEntry> _newEntry)
        {
            Mod.Mod mod = _newEntry?.GetEntry().modEntry;

            modTabs.ViewComponent.IsVisible = mod != null;
            modInfo.SetCurrentMod(mod);
            
            if(!modSettings.SetCurrentMod(mod, modTabs.SelectedTabIndex))
            {
                modTabs.SelectedTabIndex = 0;
            }
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
            
            if (ModLoader.Instance.SaveModChanges())
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
