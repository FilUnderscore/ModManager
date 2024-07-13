using HarmonyLib;

namespace CustomModManager.Mod
{
    internal sealed class ModLoaderPatches
    {
        [HarmonyPatch(typeof(global::Mod))]
        [HarmonyPatch(nameof(global::Mod.InitModCode))]
        private sealed class Mod_InitModCode_Patch
        {
            private static bool Prefix(global::Mod __instance)
            {
                Mod modInstance = ModLoader.Instance.GetModFromInstance(__instance);
                modInstance.preloaded = false;

                bool enabled = ModLoader.Instance.IsModEnabled(__instance);
                modInstance.initialized = enabled;

                return enabled;
            }
        }

        [HarmonyPatch(typeof(global::ModManager))]
        [HarmonyPatch(nameof(global::ModManager.ModLoaded))]
        private sealed class ModManager_ModLoaded_Patch
        {
            private static void Postfix(string _modName, ref bool __result)
            {
                if(__result == false)
                {
                    return;
                }

                __result = ModLoader.Instance.IsModEnabled(ModManager.loadedMods.dict[_modName]);
            }
        }
    }
}
