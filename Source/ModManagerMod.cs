using HarmonyLib;

using CustomModManager.API;
using System;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
        private bool showPatchNotesOnStartup = true, showUpdatesOnStartup = true;
        
        public ModManagerMod()
        {
            var harmony = new Harmony("filunderscore.modmanager");
            harmony.PatchAll();

            ModLoader.harmony = harmony;
        }

        public void InitMod(Mod _modInstance)
        {
            ModLoader.CheckForPreloadedMods();

            if (ModManagerAPI.IsModManagerLoaded())
            {
                ModManagerAPI.ModSettings settings = ModManagerAPI.GetModSettings(_modInstance);

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
            }

            ModEvents.GameStartDone.RegisterHandler(GameStartDone);
        }

        private void GameStartDone()
        {
            throw new NotImplementedException();
        }
    }
}
