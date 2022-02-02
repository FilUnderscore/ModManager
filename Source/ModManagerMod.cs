using HarmonyLib;

using CustomModManager.API;

namespace CustomModManager
{
    public class ModManagerMod : IModApi
    {
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

                settings.Hook<int>("testValue", "xuiTestValue", "xuiTestValue2", newValue => testValue = newValue, () => testValue, (value) => value.ToString(), (str) =>
                {
                    bool result = int.TryParse(str, out int val);
                    return (val, result);
                });
            }
        }
    }
}
