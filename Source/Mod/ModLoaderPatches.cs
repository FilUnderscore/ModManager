using HarmonyLib;

namespace CustomModManager.Mod
{
    internal sealed class ModLoaderPatches
    {
        // Flag to prevent mod DLLs loading after Mod Manager does to prevent disabled mods from loading automatically.
        public static bool CAN_INIT_MOD_CODE = false;

        [HarmonyPatch(typeof(global::Mod))]
        [HarmonyPatch(nameof(global::Mod.InitModCode))]
        private sealed class Mod_InitModCode_Patch
        {
            private static int PRE_INIT_COUNT = 1;

            private static bool Prefix()
            {
                if (!CAN_INIT_MOD_CODE)
                    PRE_INIT_COUNT++;

                return CAN_INIT_MOD_CODE;
            }

            private static int MOD_COUNT
            {
                get
                {
                    return ModLoader.Instance.GetMods(false).Count;
                }
            }

            private static void Postfix()
            {
                if (CAN_INIT_MOD_CODE)
                    return;

                int modCount = MOD_COUNT;
                CAN_INIT_MOD_CODE = PRE_INIT_COUNT == modCount;
                
                if(CAN_INIT_MOD_CODE)
                {
                    foreach(var mod in ModLoader.Instance.GetMods(false))
                    {
                        Log.Out("Mod " + mod.Info.Name + " enabled " + ModLoader.Instance.IsModEnabled(mod));

                        if(ModLoader.Instance.IsModEnabled(mod))
                            mod.Load();
                    }
                }
            }
        }
    }
}
