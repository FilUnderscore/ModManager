using HarmonyLib;
using System;
using System.Collections.Generic;
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

        public static ModSettings GetModSettings(global::Mod modInstance)
        {
            if (!IsModManagerLoaded())
            {
                Log.Warning($"[{modInstance.Name}] [Mod Manager API] Attempted to create mod settings while mod manager is not installed.");
                return new ModSettings(modInstance, null);
            }

            try
            {
                return new ModSettings(modInstance, CORE_ASSEMBLY.CreateInstance("CustomModManager.ModManagerModSettings", true, 0, null, new object[] { modInstance }, null, null));
            }
            catch
            {
                Log.Warning($"[{modInstance.Name}] [Mod Manager API] Failed to locate ModSettings instance in Mod Manager. Perhaps an out-of-date API version is being used?");

                return new ModSettings(modInstance, null);
            }
        }

        public class ModSettings
        {
            private readonly global::Mod modInstance;
            private readonly object instance;

            public ModSettings(global::Mod modInstance, object instance)
            {
                this.modInstance = modInstance;
                this.instance = instance;
            }

            public ModSetting<T> Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T, (string unformatted, string formatted)> toString, Func<string, (T, bool)> fromString)
            {
                try
                {
                    MethodInfo method = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSettings").GetMethods().Single(m => m.Name == "Hook" && m.IsGenericMethod && m.IsVirtual).MakeGenericMethod(typeof(T));
                    object settingInstance = method.Invoke(instance, new object[] { key, nameUnlocalized, setCallback, getCallback, toString, fromString });

                    return new ModSetting<T>(this, key, settingInstance);
                }
                catch
                {
                    Log.Warning($"[{modInstance.Name}] [Mod Manager API] Failed to create Mod Setting instance. Perhaps an out-of-date API version is being used?");
                }

                return new ModSetting<T>(this, key, null);
            }

            public ModSetting<string> Category(string key, string nameUnlocalized)
            {
                try
                {
                    MethodInfo method = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSettings").GetMethods().Single(m => m.Name == "Category");
                    object settingInstance = method.Invoke(instance, new object[] { key, nameUnlocalized });

                    return new ModSetting<string>(this, key, settingInstance);
                }
                catch
                {
                    Log.Warning($"[{modInstance.Name}] [Mod Manager API] Failed to create Mod Setting instance. Perhaps an out-of-date API version is being used?");
                }

                return new ModSetting<string>(this, key, null);
            }

            public ModSetting<string> Button(string key, string nameUnlocalized, Action clickCallback, Func<string> buttonText)
            {
                try
                {
                    MethodInfo method = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSettings").GetMethods().Single(m => m.Name == "Button");
                    object settingInstance = method.Invoke(instance, new object[] { key, nameUnlocalized, clickCallback, buttonText });

                    return new ModSetting<string>(this, key, settingInstance);
                }
                catch
                {
                    Log.Warning($"[{modInstance.Name}] [Mod Manager API] Failed to create Mod Setting instance. Perhaps an out-of-date API version is being used?");
                }

                return new ModSetting<string>(this, key, null);
            }

            public ModSetting<T> Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T, string> toString, Func<string, (T, bool)> fromString)
            {
                return Hook(key, nameUnlocalized, setCallback, getCallback, (value) =>
                {
                    string valueAsString = toString(value);
                    return (valueAsString, valueAsString);
                }, fromString);
            }

            public void CreateTab(string key, string nameUnlocalized)
            {
                try
                {
                    MethodInfo method = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSettings").GetMethods().Single(m => m.Name == "CreateTab" && m.IsVirtual);
                    method.Invoke(instance, new object[] { key, nameUnlocalized });
                }
                catch
                {
                    Log.Warning($"[{modInstance.Name}] [Mod Manager API] Failed to create Mod Setting tab. Perhaps an out-of-date API version is being used?");
                }
            }

            public class ModSetting<T>
            {
                private readonly ModSettings settingsInstance;
                private readonly string key;
                private readonly object instance;

                internal ModSetting(ModSettings settings, string key, object instance)
                {
                    this.settingsInstance = settings;
                    this.key = key;
                    this.instance = instance;
                }

                public ModSetting<T> SetAllowedValues(T[] allowedValues)
                {
                    try
                    {
                        TryInvokeMethod("SetAllowedValues", allowedValues);
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to set allowed values for mod setting {this.key}");
                    }

                    return this;
                }

                public ModSetting<T> SetTab(string tabKey)
                {
                    try
                    {
                        TryInvokeMethod("SetTab", tabKey);
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to set tab key {tabKey} for mod setting {this.key}");
                    }

                    return this;
                }

                public ModSetting<T> SetMinimumMaximumAndIncrementValues(T minimumValue, T maximumValue, T incrementValue)
                {
                    try
                    {
                        TryInvokeMethod("SetMinimumMaximumAndIncrementValues", minimumValue, maximumValue, incrementValue);
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to set minimum/maximum/increment values for mod setting {this.key}");
                    }

                    return this;
                }

                public ModSetting<T> SetWrap(bool wrap)
                {
                    try
                    {
                        TryInvokeMethod("SetWrap", wrap);
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to set wrap flag for mod setting {this.key}");
                    }

                    return this;
                }

                public void Update()
                {
                    try
                    {
                        TryInvokeMethod("Update", new object[] { });
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to update mod setting {this.key}");
                    }
                }

                public ModSetting<T> SetEnabled(Func<bool> enabled)
                {
                    try
                    {
                        TryInvokeMethod("SetEnabled", enabled);
                    }
                    catch
                    {
                        Log.Warning($"[{settingsInstance.modInstance.Name}] [Mod Manager API] [Mod Settings] Failed to set enabled selector for mod setting {this.key}");
                    }

                    return this;
                }

                private void TryInvokeMethod(string name, params object[] parameters)
                {
                    if (instance == null)
                        return;

                    MethodInfo setAllowedValuesMethod = CORE_ASSEMBLY.GetType("CustomModManager.API.IModSetting`1[[" + typeof(T).AssemblyQualifiedName + "]]").GetMethods().Single(m => m.Name == name && m.IsVirtual);
                    setAllowedValuesMethod.Invoke(instance, parameters);
                }
            }
        }

#if MM_API_EXTENSIONS
        public static class Extensions
        {
            public static void AddStartupMessage(string title, string message)
            {
                StartupMessageBox.startupMessages.Add(new StartupMessageBox.StartupMessage(title, message));
            }

            private static class StartupMessageBox
            {
                public class StartupMessage
                {
                    public readonly string Title;
                    public readonly string Message;

                    public StartupMessage(string title, string message)
                    {
                        this.Title = title;
                        this.Message = message;
                    }
                }

                public static System.Collections.Generic.List<StartupMessage> startupMessages = new System.Collections.Generic.List<StartupMessage>();

                [HarmonyLib.HarmonyPatch(typeof(XUiC_MainMenu))]
                [HarmonyLib.HarmonyPatch("Open")]
                class XUiC_MainMenu_Open_Extension
                {
                    static bool mainMenuLoaded = false;

                    static void Postfix(XUi _xuiInstance)
                    {
                        if (mainMenuLoaded)
                            return;

                        mainMenuLoaded = true;

                        if (startupMessages.Count > 0)
                        {
                            ConstructNextMessageAction(_xuiInstance, 0).Invoke();
                        }
                    }

                    private static Action ConstructNextMessageAction(XUi _xuiInstance, int currentReadCount)
                    {
                        return () =>
                        {
                            StartupMessage message = startupMessages[currentReadCount];

                            XUiC_MessageBoxWindowGroup.ShowMessageBox(_xuiInstance, message.Title, message.Message, () =>
                            {
                                if (++currentReadCount < startupMessages.Count)
                                {
                                    ConstructNextMessageAction(_xuiInstance, currentReadCount);
                                }
                            }, ++currentReadCount >= startupMessages.Count);
                        };
                    }
                }
            }

            public static void AddLoadingScreenTip(string title, string message)
            {
                LoadingScreenTips.tips.Add(new LoadingScreenTips.LoadingScreenTip(title, message));
            }

            private static class LoadingScreenTips
            {
                private static readonly FieldInfo tipsField = AccessTools.DeclaredField(typeof(XUiC_LoadingScreen), "tips");
                private static readonly FieldInfo currentTipIndexField = AccessTools.DeclaredField(typeof(XUiC_LoadingScreen), "currentTipIndex");

                public static readonly List<LoadingScreenTip> tips = new List<LoadingScreenTip>();

                private static List<string> GetTips()
                {
                    return (List<string>)tipsField.GetValue(null);
                }

                public class LoadingScreenTip
                {
                    public readonly string Title;
                    public readonly string Message;

                    public LoadingScreenTip(string title, string message)
                    {
                        this.Title = title;
                        this.Message = message;
                    }
                }

                [HarmonyLib.HarmonyPatch(typeof(XUiC_LoadingScreen))]
                [HarmonyLib.HarmonyPatch("OnOpen")]
                class XUiC_LoadingScreen_OnOpen_Extension
                {
                    static void Prefix(XUiC_LoadingScreen __instance)
                    {
                        for(int i = 0; i < tips.Count; i++)
                            GetTips().Add("mm-loadingscreen-tip" + i);
                    }
                }

                [HarmonyLib.HarmonyPatch(typeof(XUiC_LoadingScreen))]
                [HarmonyLib.HarmonyPatch("OnClose")]
                class XUiC_LoadingScreen_OnClose_Extension
                {
                    static void Prefix(XUiC_LoadingScreen __instance)
                    {
                        for(int i = 0; i < tips.Count; i++)
                            GetTips().RemoveAt(GetTips().Count - 1);
                    }
                }

                [HarmonyLib.HarmonyPatch(typeof(XUiC_LoadingScreen))]
                [HarmonyLib.HarmonyPatch("GetBindingValue")]
                class XUiC_LoadingScreen_GetBindingValue_Extension
                {
                    static bool Prefix(XUiC_LoadingScreen __instance, ref string _value, string _bindingName, ref bool __result)
                    {
                        int currentTipIndex = (int)currentTipIndexField.GetValue(__instance);

                        if (currentTipIndex >= 0)
                        {
                            string currentTip = GetTips()[currentTipIndex];

                            if (currentTip.StartsWith("mm-loadingscreen-tip"))
                            {
                                if (int.TryParse(currentTip.Substring(20), out int index))
                                {
                                    switch (_bindingName)
                                    {
                                        case "title":
                                            _value = tips[index].Title;
                                            break;
                                        case "text":
                                            _value = tips[index].Message;
                                            break;
                                    }

                                    return false;
                                }
                            }
                        }

                        return true;
                    }
                }
            }
        }
#endif
    }
}