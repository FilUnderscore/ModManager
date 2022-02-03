using HarmonyLib;

using CustomModManager.API;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
        private bool showPatchNotesOnStartup = true;
        int tester;
        
        public ModManagerMod()
        {
            var harmony = new Harmony("filunderscore.modmanager");
            harmony.PatchAll();
        }

        public void InitMod(Mod _modInstance)
        {
            CustomModManager.CheckForUndetectedMods();

            if(ModManagerAPI.IsModManagerLoaded())
            {
                ModManagerAPI.ModSettings settings = ModManagerAPI.GetModSettings(_modInstance);

                settings.Hook("showPatchNotesOnStartup", "xuiModManagerShowPatchNotesOnStartupSetting", value => showPatchNotesOnStartup = value, () => showPatchNotesOnStartup, (value) => value.ToString(), (str) =>
                {
                    bool result = bool.TryParse(str, out bool val);
                    return (val, result);
                }).SetAllowedValues(new bool[] { true, false }).SetWrap(true);

                settings.Hook("test", "test", value => tester = value, () => tester, (value) => value.ToString(), str =>
                {
                    bool result = int.TryParse(str, out int val);
                    return (val, result);
                }).SetMinimumMaximumAndIncrementValues(0, 100, 10).SetWrap(false).SetDisplayFormat(value => value.ToString() + "%");
            }
        }
    }
}
