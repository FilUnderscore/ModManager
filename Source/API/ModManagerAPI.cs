using System;
using System.Linq;
using System.Reflection;

namespace CustomModManager.API
{
    public static class ModManagerAPI
    {
        private static string CORE_ASSEMBLY_ID = "ModManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        
        private static bool initialized = false;
        private static Assembly CORE_ASSEMBLY;

        static ModManagerAPI()
        {
            TryDetectAssembly();
        }

        private static void TryDetectAssembly()
        {
            if (initialized)
                return;

            CORE_ASSEMBLY = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName == CORE_ASSEMBLY_ID);

            initialized = true;
        }

        public static bool IsModManagerLoaded()
        {
            return CORE_ASSEMBLY != null;
        }

        public static ModSettings GetModSettings(Mod modInstance)
        {
            if(!IsModManagerLoaded())
            {
                Log.Warning($"[{modInstance.ModInfo.Name.Value}] [Mod Manager API] Attempted to create mod settings while mod manager is not installed.");
                return null;
            }

            try
            {
                return new ModSettings(modInstance, CORE_ASSEMBLY.CreateInstance("CustomModManager.ModManagerModSettings", true, 0, null, new object[] { modInstance }, null, null));
            }
            catch
            {
                Log.Warning($"[{modInstance.ModInfo.Name.Value}] [Mod Manager API] Failed to locate ModSettings instance in Mod Manager. Perhaps an out-of-date API version is being used?");

                return null;
            }
        }

        public class ModSettings
        {
            private readonly Mod modInstance;
            private readonly object instance;

            public ModSettings(Mod modInstance, object instance)
            {
                this.modInstance = modInstance;
                this.instance = instance;
            }

            public void Hook<T>(string key, string nameUnlocalized, string tooltip, Action<T> setCallback, Func<T> getCallback, Func<T, string> toString, Func<string, (T, bool)> fromString, T[] allowedValues = null)
            {
                try
                {
                    MethodInfo method = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSettings").GetMethods().Single(m => m.Name == "Hook" && m.IsGenericMethod && m.IsVirtual).MakeGenericMethod(typeof(T));
                    method.Invoke(instance, new object[] { key, nameUnlocalized, tooltip, setCallback, getCallback, toString, fromString, allowedValues });
                }
                catch
                {
                    Log.Warning($"[{modInstance.ModInfo.Name.Value}] [Mod Manager API] Failed to hook variables into ModSettings instance. Perhaps an out-of-date API version is being used?");
                }
            }
        }
    }
}
