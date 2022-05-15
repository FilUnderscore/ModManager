using HarmonyLib;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CustomModManager.UI;

namespace CustomModManager
{
    public class CustomModManager
    {
        private static readonly string modSettingsFilename = "mod-settings.xml";
        private const string modListFilename = "modlist.xml";

        static CustomModManager()
        {
            ModSettingsFromXml.Load();
        }

        internal static bool Save()
        {
            SaveModSettings();

            List<ModEntry> updatedMods = new List<ModEntry>();
            int updatedCount = 0;

            foreach(var mod in ModLoader.GetLoadedMods())
            {
                if (!mod.flag)
                    continue;

                updatedMods.Add(mod);
                mod.flag = false;
            }

            if (updatedMods.Count == 0)
                return false;

            string file = ModLoader.modPath + "/" + ModLoader.disabledModsFilename;
            List<string> lines = File.Exists(file) ? File.ReadAllLines(file).ToList() : new List<string>();

            foreach (var mod in updatedMods)
            {
                if (mod.WillBeEnabled().Value)
                    lines.RemoveAll(folderName => Path.GetFileName(mod.loadInfo.modPath) == folderName);
                else
                    lines.Add(Path.GetFileName(mod.loadInfo.modPath));

                if (mod.WillBeEnabled().Value != mod.IsLoaded())
                    updatedCount++;
            }

            File.WriteAllLines(file, lines);
            
            return updatedCount > 0;
        }

        private static void SaveModSettings()
        {
            ModSettingsFromXml.Save();
        }

        internal static string GetSettingsFileLocation()
        {
            return GameIO.GetApplicationPath() + "/" + modSettingsFilename;
        }

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(XUiC_MainMenu))]
            [HarmonyPatch(nameof(XUiC_MainMenu.Init))]
            class XUiC_MainMenuInitHook
            {
                static void Postfix(XUiC_MainMenu __instance)
                {
                    ((XUiC_SimpleButton)__instance.GetChildById("btnMods")).OnPressed += XUiC_MainMenuInitHook_OnPressed;
                }

                private static void XUiC_MainMenuInitHook_OnPressed(XUiController _sender, int _mouseButton)
                {
                    _sender.xui.playerUI.windowManager.Close(_sender.WindowGroup.ID);
                    _sender.xui.playerUI.windowManager.Open(XUiC_Mods.ID, true);
                }
            }

            [HarmonyPatch(typeof(XUiC_NewContinueGame))]
            [HarmonyPatch("BtnStart_OnPressed")]
            class XUiC_NewContinueGameBtnStart_OnPressedHook
            {
                private static readonly FieldInfo isContinueGameField = AccessTools.DeclaredField(typeof(XUiC_NewContinueGame), "isContinueGame");
                private static readonly MethodInfo startGameMethod = AccessTools.DeclaredMethod(typeof(XUiC_NewContinueGame), "startGame");
                private static readonly MethodInfo modCheckMethod = AccessTools.DeclaredMethod(typeof(CustomModManager.HarmonyPatches.XUiC_NewContinueGameBtnStart_OnPressedHook), "ModCheck");
                private static readonly MethodInfo closeMethod = AccessTools.DeclaredMethod(typeof(GUIWindowManager), "Close", new System.Type[] { typeof(string) });

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> list = new List<CodeInstruction>(instructions);

                    for(int i = 0; i < list.Count; i++)
                    {
                        if(list[i].Calls(startGameMethod))
                        {
                            list[i].operand = modCheckMethod;
                        }
                    }

                    for(int i = 0; i < list.Count; i++)
                    {
                        if(list[i].Calls(closeMethod))
                        {
                            list.RemoveRange(i - 6, 8);
                            break;
                        }
                    }

                    return list;
                }

                private static void ModCheck(XUiC_NewContinueGame instance)
                {
                    if (!(bool)isContinueGameField.GetValue(instance))
                    {
                        SaveModList();
                        StartGame(instance);
                        return;
                    }

                    XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(instance.xui, Localization.Get("xuiModsListChanged"), string.Format(Localization.Get("xuiGameModListChanged"), "mod list here"), Localization.Get("xuiYes"), Localization.Get("xuiNo"), () => 
                    {
                        SaveModList();
                        StartGame(instance);
                    }, () => { }, false);
                }

                private static void StartGame(XUiC_NewContinueGame instance)
                {
                    startGameMethod.Invoke(instance, null);
                    instance.xui.playerUI.windowManager.Close(instance.WindowGroup.ID);
                }

                private static void SaveModList()
                {
                    string gameName = GamePrefs.GetString(EnumGamePrefs.GameName);
                    string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);
                    string path = GameIO.GetSaveGameDir(gameWorld, gameName);


                }
            }

            [HarmonyPatch(typeof(XUiC_SavegamesList.ListEntry))]
            [HarmonyPatch("GetBindingValue")]
            class XUiC_SavegamesListListEntryGetBindingValueHook
            {
                static void Postfix(XUiC_SavegamesList.ListEntry __instance, ref bool __result, ref string _value, string _bindingName)
                {                    
                    switch(_bindingName)
                    {
                        case "modstooltip":
                            string worldName = __instance.worldName;
                            string saveName = __instance.saveName;
                            string path = GameIO.GetSaveGameDir(worldName, saveName);

                            _value = "This is a modded game.\n\nMods Used:\nImproved Hordes\nMod Manager\nModNumber3\nModNumber4";
                            __result = true;
                            break;
                        case "ismodded":
                            _value = true.ToString();
                            __result = true;
                            break;
                        case "moddedtext":
                            _value = "VANILLA";
                            __result = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            [HarmonyPatch(typeof(XUiC_SavegamesList.ListEntry))]
            [HarmonyPatch("GetNullBindingValues")]
            class XUiC_SavegamesListGetNullBindingValuesHook
            {
                static void Postfix(XUiC_SavegamesList.ListEntry __instance, ref bool __result, ref string _value, string _bindingName)
                {
                    switch(_bindingName)
                    {
                        case "modstooltip":
                            _value = "";
                            __result = true;
                            break;
                        case "ismodded":
                            _value = false.ToString();
                            __result = true;
                            break;
                        case "moddedtext":
                            _value = "";
                            __result = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            [HarmonyPatch(typeof(XUiC_LoadingScreen))]
            [HarmonyPatch("GetBindingValue")]
            class XUiC_LoadingScreenGetBindingValueHook
            {
                static void Postfix(XUiC_LoadingScreen __instance, ref bool __result, ref string _value, string _bindingName)
                {
                    switch(_bindingName)
                    {
                        case "modlisttext":
                            List<ModEntry> loadedMods = ModLoader.GetLoadedMods();

                            string loadedModsStr = loadedMods[0].info.Name.Value;

                            for(int i = 1; i < loadedMods.Count; i++)
                            {
                                loadedModsStr += " \u2022 " + loadedMods[i].info.Name.Value;
                            }

                            _value = loadedModsStr;
                            __result = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
