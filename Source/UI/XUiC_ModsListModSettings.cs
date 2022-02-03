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

        private XUiC_SimpleButton resetButton;

        private readonly List<XUiC_ModSettingSelector> modSettingSelectorList = new List<XUiC_ModSettingSelector>();

        private int page;

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

            foreach (XUiC_ModSettingSelector xuiCModSettingSelector in this.GetChildById("settings").GetChildrenByType<XUiC_ModSettingSelector>())
            {
                modSettingSelectorList.Add(xuiCModSettingSelector);
            }
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

        private void UpdateSettings()
        {
            foreach (var selector in modSettingSelectorList)
            {
                selector.ViewComponent.IsVisible = false;
            }

            int entryIndex = 0;
            for (int index = this.pager.CurrentPageNumber * (modSettingSelectorList.Count - 1); index < ModManagerModSettings.modSettingsInstances[currentModEntry].settings.Count; index++)
            {
                if (entryIndex == modSettingSelectorList.Count - 1)
                    break;

                var settingEntry = ModManagerModSettings.modSettingsInstances[currentModEntry].settings.ElementAt(index);

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

            this.UpdateSettings();

            this.pager.CurrentPageNumber = 0;
            this.pager.LastPageNumber = settingsCount / modSettingSelectorList.Count;
        }
    }
}
