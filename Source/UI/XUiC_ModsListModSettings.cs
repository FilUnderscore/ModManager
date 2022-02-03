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

        private readonly List<XUiC_ModSettingSelector> modSettingSelectorList = new List<XUiC_ModSettingSelector>();

        private int page;

        public override void Init()
        {
            base.Init();

            this.noModSettingsDetectedLabel = (XUiV_Label)this.GetChildById("NoModSettingsDetected").ViewComponent;
            this.settingsPanel = (XUiV_Panel)this.GetChildById("settingsPanel").ViewComponent;
            this.pagerPanel = (XUiV_Panel)this.GetChildById("pagerPanel").ViewComponent;

            foreach (XUiC_ModSettingSelector xuiCModSettingSelector in this.GetChildById("settings").GetChildrenByType<XUiC_ModSettingSelector>())
            {
                modSettingSelectorList.Add(xuiCModSettingSelector);
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();

        }

        internal void UpdateView(bool visible)
        {
            if (currentModEntry == null || !ModManagerModSettings.modSettingsInstances.ContainsKey(currentModEntry))
                return;

            bool anySettings = ModManagerModSettings.modSettingsInstances[currentModEntry].settings.Count > 0;

            this.noModSettingsDetectedLabel.IsVisible = visible && !anySettings;
            this.settingsPanel.IsVisible = visible && anySettings;
            this.pagerPanel.IsVisible = visible && anySettings;

            foreach(var selector in modSettingSelectorList)
            {
                selector.ViewComponent.IsVisible = false;
            }

            int entryIndex = 0;
            foreach(var settingEntry in ModManagerModSettings.modSettingsInstances[currentModEntry].settings)
            {
                if (entryIndex == modSettingSelectorList.Count - 1)
                    break;

                var key = settingEntry.Key;
                var setting = settingEntry.Value;

                XUiC_ModSettingSelector selector = modSettingSelectorList[entryIndex];
                selector.ViewComponent.IsVisible = true;
                selector.UpdateModSetting(key, setting);

                entryIndex++;
            }
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            Log.Out(_bindingName);
            switch(_bindingName)
            {
                case "isvisible":
                    _value = (currentModEntry != null && ModManagerModSettings.modSettingsInstances.ContainsKey(currentModEntry) && ModManagerModSettings.modSettingsInstances[currentModEntry].settings.Count > 0).ToString();
                    Log.Out(_value);
                    return true;
                default:
                    return false;
            }
        }
    }
}
