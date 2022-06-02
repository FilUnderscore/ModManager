using HarmonyLib;

using CustomModManager.API;
using System.IO;
using System.Linq;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
        //private bool showPatchNotesOnStartup = true, showUpdatesOnStartup = true;
        private int selectedModDir = 0;
        private ModManagerAPI.ModSettings.ModSetting<int> currentModDirSetting;
        private ModManagerAPI.ModSettings.ModSetting<string> openModDirButton;

        public ModManagerMod()
        {
            var harmony = new Harmony("filunderscore.modmanager");
            harmony.PatchAll();

            ModLoader.harmony = harmony;
            ModSettingsFromXml.Load();
        }

        public void InitMod(Mod _modInstance)
        {
            ModLoader.CheckForPreloadedMods();

            if (ModManagerAPI.IsModManagerLoaded())
            {
                ModManagerAPI.ModSettings settings = ModManagerAPI.GetModSettings(_modInstance);

                settings.Hook("modDir", "xuiModManagerModDirSetting", value =>
                {
                    string[] paths = value.Split(';').ToArray();
                    ModLoader.modPaths = paths;

                    if (currentModDirSetting != null)
                    {
                        currentModDirSetting.SetMinimumMaximumAndIncrementValues(0, paths.Length, 1);
                        currentModDirSetting.Update();

                        if(openModDirButton != null)
                            openModDirButton.Update();
                    }
                }, () => ModLoader.modPaths.ToList().StringFromList(";"), toStr =>
                {
                    int dirCount = toStr.Split(';').Length;
                    return (toStr, dirCount + " Director" + (dirCount > 1 ? "ies" : "y"));
                }, str =>
                {
                    string[] paths = str.Split(';').ToArray();
                    bool success = paths.All(path => Directory.Exists(path.Trim()));

                    return (str, success);
                });

                this.currentModDirSetting = settings.Hook("currentModDir", "xuiModManagerCurrentModDirSetting", value =>
                {
                    selectedModDir = value;

                    if (openModDirButton != null)
                        openModDirButton.Update();
                }, () => selectedModDir, toStr => (toStr.ToString(), toStr <= ModLoader.modPaths.Length && toStr > 0 ? ModLoader.modPaths[toStr - 1] : "Choose a Mod Directory"), str =>
                {
                    bool success = int.TryParse(str, out int val);
                    return (val, success);
                }).SetMinimumMaximumAndIncrementValues(0, ModLoader.modPaths.Length, 1);

                this.openModDirButton = settings.Button("openModDirButton", "xuiModManagerOpenModDirButton", () =>
                {
                    if(selectedModDir > 0 && selectedModDir <= ModLoader.modPaths.Length)
                        UnityEngine.Application.OpenURL(ModLoader.modPaths[selectedModDir - 1]);
                }, 
                () => Localization.Get("xuiModManagerOpenModDirButtonText")).SetEnabled(() => ModLoader.modPaths.Length > 0 && selectedModDir > 0 && selectedModDir <= ModLoader.modPaths.Length);

                /*
                settings.Category("startup", "Startup");

                settings.Hook("showPatchNotesOnStartup", "xuiModManagerShowPatchNotesOnStartupSetting", value => showPatchNotesOnStartup = value, () => showPatchNotesOnStartup, (value) => (value.ToString(), value ? "Yes" : "No"), (str) =>
                {
                    bool result = bool.TryParse(str, out bool val);
                    return (val, result);
                }).SetAllowedValues(new bool[] { true, false });

                settings.Hook("showUpdatesOnStartup", "xuiModManagerShowUpdatesOnStartupSetting", value => showUpdatesOnStartup = value, () => showUpdatesOnStartup, (value) => (value.ToString(), value ? "Yes" : "No"), (str) =>
                {
                    bool result = bool.TryParse(str, out bool val);
                    return (val, result);
                }).SetAllowedValues(new bool[] { true, false });
                */
            }
        }
    }
}
