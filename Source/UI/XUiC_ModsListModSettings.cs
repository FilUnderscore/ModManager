using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager.UI
{
    public class XUiC_ModsListModSettings : XUiController
    {
        public ModEntry currentModEntry = null;

        internal XUiC_Mods mods;

        private XUiV_Label noModSettingsDetectedLabel;
        private XUiV_Panel settingsPanel;
        private XUiV_Panel pagerPanel;
        private XUiC_Paging pager;

        private XUiC_TabSelector settingsTabs;

        private XUiC_SimpleButton resetButton;

        private readonly List<XUiC_ModSettingSelector> modSettingSelectorList = new List<XUiC_ModSettingSelector>();

        private string currentTabKey;
        private readonly List<XUiV_Sprite> tabButtonSpacers = new List<XUiV_Sprite>();

        public override void Init()
        {
            base.Init();

            this.noModSettingsDetectedLabel = (XUiV_Label)this.GetChildById("NoModSettingsDetected").ViewComponent;
            this.settingsPanel = (XUiV_Panel)this.GetChildById("settingsPanel").ViewComponent;
            this.pagerPanel = (XUiV_Panel)this.GetChildById("pagerPanel").ViewComponent;
            this.pager = this.GetChildByType<XUiC_Paging>();
            this.pager.OnPageChanged += Pager_OnPageChanged;

            this.resetButton = (XUiC_SimpleButton) this.GetChildById("resetSettingsButton");
            this.resetButton.OnPressed += ResetButton_OnPressed;

            this.settingsTabs = (XUiC_TabSelector)this.GetChildById("settingsTabs");
            this.settingsTabs.OnTabChanged += SettingsTabs_OnTabChanged;
            
            foreach (XUiController tabButtonSpacer in this.GetChildrenById("tabButtonSpacer"))
            {
                if(tabButtonSpacer.ViewComponent is XUiV_Sprite)
                    tabButtonSpacers.Add(tabButtonSpacer.ViewComponent as XUiV_Sprite);
            }

            foreach (XUiC_ModSettingSelector xuiCModSettingSelector in this.GetChildById("settings").GetChildrenByType<XUiC_ModSettingSelector>())
            {
                modSettingSelectorList.Add(xuiCModSettingSelector);
            }
        }

        private void SettingsTabs_OnTabChanged(int tabIndex, string tabName)
        {
            currentTabKey = ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs.ElementAt(tabIndex).Key;
            this.UpdateSettings();
        }

        private void Pager_OnPageChanged()
        {
            if (currentModEntry == null || !ModManagerModSettings.modSettingsInstances.ContainsKey(currentModEntry))
                return;

            this.UpdateSettings();
        }

        private void ResetButton_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (currentModEntry == null || !ModManagerModSettings.modSettingsInstances.ContainsKey(currentModEntry))
                return;

            foreach(var settingEntry in ModManagerModSettings.modSettingsInstances[currentModEntry].settings)
            {
                var setting = settingEntry.Value;

                setting.Reset();
                this.UpdateView(true);
            }
        }

        private void UpdateTabs()
        {
            Dictionary<string, ModManagerModSettings.ModSettingTab> tabs = ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs;
            bool anyTabs = tabs.Count > 0;

            if (anyTabs)
            {
                int tabIndex = 0;

                for (int index = 0; index < tabs.Count; index++) // TODO: Change index = to currentPageNumber of pager.
                {
                    if (tabIndex == settingsTabs.TabCount)
                        break;

                    var tabEntry = ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs.ElementAt(index);
                    ModManagerModSettings.ModSettingTab tab = tabEntry.Value;

                    settingsTabs.GetTabButton(tabIndex).Label = Localization.Get(tab.nameUnlocalized);
                    settingsTabs.GetTabButton(tabIndex).ViewComponent.IsVisible = true;
                    tabButtonSpacers[tabIndex].IsVisible = true;

                    tabIndex++;
                }
            }
        }

        private void UpdateSettings()
        {
            foreach (var selector in modSettingSelectorList)
            {
                selector.ViewComponent.IsVisible = false;
            }

            bool anyTabs = ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs.Count > 0;
            Dictionary<string, ModManagerModSettings.BaseModSetting> settings = ModManagerModSettings.modSettingsInstances[currentModEntry].settings.Where(entry => !anyTabs || (currentTabKey != null && entry.Value.GetTabKey() == currentTabKey)).ToDictionary(entry => entry.Key, entry => entry.Value);

            int entryIndex = 0;
            for (int index = this.pager.CurrentPageNumber * modSettingSelectorList.Count; index < settings.Count; index++)
            {
                if (entryIndex == modSettingSelectorList.Count)
                    break;

                var settingEntry = settings.ElementAt(index);

                var key = settingEntry.Key;
                var setting = settingEntry.Value;

                XUiC_ModSettingSelector selector = modSettingSelectorList[entryIndex];
                selector.ViewComponent.IsVisible = true;
                selector.UpdateModSetting(key, setting);

                entryIndex++;
            }
        }

        internal void UpdateView(bool visible)
        {
            if (currentModEntry == null || !ModManagerModSettings.modSettingsInstances.ContainsKey(currentModEntry))
                return;

            int settingsCount = ModManagerModSettings.modSettingsInstances[currentModEntry].settings.Count;
            bool anySettings = settingsCount > 0;

            this.noModSettingsDetectedLabel.IsVisible = visible && !anySettings;
            this.settingsPanel.IsVisible = visible && anySettings;
            this.pagerPanel.IsVisible = visible && anySettings && settingsCount > modSettingSelectorList.Count;

            for (int tabIndex = 0; tabIndex < settingsTabs.TabCount; tabIndex++)
            {
                settingsTabs.GetTabButton(tabIndex).ViewComponent.IsVisible = false;
                tabButtonSpacers[tabIndex].IsVisible = false;
            }

            if (ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs.Count > 0)
            {
                settingsTabs.SelectedTabIndex = 0;
                currentTabKey = ModManagerModSettings.modSettingsInstances[currentModEntry].settingTabs.ElementAt(0).Key;
            }

            this.pager.CurrentPageNumber = 0;
            this.pager.LastPageNumber = settingsCount / modSettingSelectorList.Count;

            this.UpdateTabs();
            this.UpdateSettings();
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch(_bindingName)
            {
                case "tabButtonPositionX":
                    _value = "30";
                    return true;
                case "tabButtonCols":
                    _value = currentModEntry != null ? "4" : "3";
                    return true;
                default:
                    return false;
            }
        }
    }
}
