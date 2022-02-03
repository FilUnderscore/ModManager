using HarmonyLib;

using CustomModManager.API;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
        private bool showPatchNotesOnStartup = true;
        private int testValue = 0;

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
                Log.Out("MM LOADED");
                ModManagerAPI.ModSettings settings = ModManagerAPI.GetModSettings(_modInstance);

                settings.Hook<int>("testValue", "xuiTestValue", newValue => testValue = newValue, () => testValue, (value) => value.ToString(), (str) =>
                {
                    bool result = int.TryParse(str, out int val);
                    return (val, result);
                });

                settings.Hook("showPatchNotesOnStartup", "xuiShowPatchNotesOnStartupSetting", value => showPatchNotesOnStartup = value, () => showPatchNotesOnStartup, (value) => value.ToString(), (str) =>
                {
                    bool result = bool.TryParse(str, out bool val);
                    return (val, result);
                }, new bool[] { true, false });
            
                for(int index = 0; index < 20; index++)
                {
                    settings.Hook("setting" + index, "thisIsASetting", (f) => { }, () => 0.0f, (val) => val.ToString(), (str) =>
                    {
                        bool result = float.TryParse(str, out float val);
                        return (val, result);
                    });
                }
            }
        }
    }
}
