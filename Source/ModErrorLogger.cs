using HarmonyLib;
using CustomModManager.UI;
using System.Collections.Generic;

namespace CustomModManager
{
    public class ModErrorLogger
    {
        private static HashSet<string> ignored = new HashSet<string>();

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(ModEventAbs<object>))]
            [HarmonyPatch("LogError")]
            class ModEventAbsLogError
            {
                static bool Prefix(object __instance, System.Exception _e, object _currentMod)
                {
                    string eventName = (string)__instance.GetType().GetField("eventName").GetValue(__instance);
                    string mod = (string)_currentMod.GetType().GetProperty("ModName").GetValue(_currentMod);

                    string message = string.Format(Localization.Get("xuiGameModErrorDesc") + Localization.Get("xuiGameModErrorDescStackTrace"), mod, eventName, _e.Message);
                    string copiedMessage = string.Format(Localization.Get("xuiGameModErrorDesc") + "Stacktrace:\n" + _e.StackTrace, mod, eventName, _e.Message);

                    if (!ignored.Contains(copiedMessage))
                    {
                        XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(((GUIWindowManager)UnityEngine.Object.FindObjectOfType(typeof(GUIWindowManager))).playerUI.xui, Localization.Get("xuiGameModError"), message, Localization.Get("xuiGameModErrorIgnore"), Localization.Get("xuiOk"), () =>
                        {
                            ignored.Add(copiedMessage);
                        }, () => { }, false, false, "Stacktrace:\n" + _e.StackTrace, copiedMessage);
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(GUIWindowConsole))]
            [HarmonyPatch("openConsole")]
            class GUIWindowConsoleOpenConsoleHook
            {
                static bool Prefix(string _logString)
                {
                    if (GameManager.Instance.World == null)
                        XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(((GUIWindowManager)UnityEngine.Object.FindObjectOfType(typeof(GUIWindowManager))).playerUI.xui, Localization.Get("xuiGameModError"), "An error has occurred: " + _logString, "Ignore", "OK", () => { }, () => { }, false, false);

                    return false;
                }
            }
        }
    }
}