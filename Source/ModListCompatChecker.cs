using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using CustomModManager.UI;

namespace CustomModManager
{
    public sealed class ModListCompatChecker
    {
        private const string modListFilename = "modlist.xml";
        private static readonly Dictionary<string, List<string>> modLists = new Dictionary<string, List<string>>();

        private enum EModListState
        {
            VANILLA,
            MODDED,
            UNKNOWN
        }

        private static EModListState GetModList(string gameWorld, string gameName, out List<string> list)
        {
            string key = gameWorld + "/" + gameName;
            bool contains = modLists.ContainsKey(key);

            if(contains)
            {
                list = modLists[key];
                
                if(list.Count == 0)
                {
                    return EModListState.VANILLA;
                }
                else
                {
                    return EModListState.MODDED;
                }
            }
            else
            {
                list = new List<string>();
                return EModListState.UNKNOWN;
            }
        }

        private static EModListState GetCurrentModList(out List<string> list)
        {
            List<string> modList = new List<string>();
            ModLoader.GetActiveMods().ForEach(mod =>
            {
                if (mod.instance.ApiInstance is ModManagerMod)
                    return;

                modList.Add(mod.info.Name.Value);
            });

            list = modList;

            return list.Count > 0 ? EModListState.MODDED : EModListState.VANILLA;
        }

        class HarmonyPatches
        {
            [HarmonyPatch(typeof(XUiC_NewContinueGame))]
            [HarmonyPatch("BtnStart_OnPressed")]
            class XUiC_NewContinueGameBtnStart_OnPressedHook
            {
                private static readonly FieldInfo isContinueGameField = AccessTools.DeclaredField(typeof(XUiC_NewContinueGame), "isContinueGame");
                private static readonly MethodInfo startGameMethod = AccessTools.DeclaredMethod(typeof(XUiC_NewContinueGame), "startGame");
                private static readonly MethodInfo modCheckMethod = AccessTools.DeclaredMethod(typeof(XUiC_NewContinueGameBtnStart_OnPressedHook), "ModCheck");
                private static readonly MethodInfo closeMethod = AccessTools.DeclaredMethod(typeof(GUIWindowManager), "Close", new System.Type[] { typeof(string) });

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> list = new List<CodeInstruction>(instructions);

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Calls(startGameMethod))
                        {
                            list[i].operand = modCheckMethod;
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Calls(closeMethod))
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

                    string gameName = GamePrefs.GetString(EnumGamePrefs.GameName);
                    string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);

                    EModListState state = GetModList(gameWorld, gameName, out List<string> modList);
                    EModListState currentState = GetCurrentModList(out List<string> currentModList);

                    if(state != EModListState.MODDED)
                    {
                        if(state != currentState)
                        {
                            SaveModList();
                        }

                        StartGame(instance);
                    }
                    else
                    {
                        if(currentModList.All(modList.Contains) && currentModList.Count == modList.Count)
                        {
                            StartGame(instance);
                        }
                        else
                        {
                            XUiC_ModsErrorMessageBoxWindowGroup.ShowMessageBox(instance.xui, Localization.Get("xuiModsListChanged"), string.Format(Localization.Get("xuiGameModListChanged"), modList.StringFromList()), Localization.Get("xuiComboYesNoOn"), Localization.Get("xuiComboYesNoOff"), () =>
                            {
                                SaveModList();
                                StartGame(instance);
                            }, () => { }, false);
                        }
                    }
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

                    if(!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement modListRoot = xmlDoc.AddXmlElement("mod_list");

                    if (modLists.ContainsKey(gameWorld + "/" + gameName))
                        modLists[gameWorld + "/" + gameName].Clear();
                    else
                        modLists.Add(gameWorld + "/" + gameName, new List<string>());

                    foreach (var modEntry in ModLoader.GetActiveMods())
                    {
                        if (modEntry.instance.ApiInstance is ModManagerMod)
                            continue;

                        XmlElement modElement = modListRoot.AddXmlElement("mod");
                        modElement.SetAttribute("name", modEntry.info.Name.Value);

                        modListRoot.AppendChild(modElement);

                        modLists[gameWorld + "/" + gameName].Add(modEntry.info.Name.Value);
                    }

                    xmlDoc.Save(path + "/" + modListFilename);
                }
            }

            [HarmonyPatch(typeof(XUiC_SavegamesList.ListEntry))]
            [HarmonyPatch(MethodType.Constructor)]
            [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(DateTime), typeof(WorldState) })]
            class XUiC_SavegamesListListEntryHook
            {
                static void Postfix(XUiC_SavegamesList.ListEntry __instance)
                {
                    string worldName = __instance.worldName;
                    string saveName = __instance.saveName;
                    string path = GameIO.GetSaveGameDir(worldName, saveName);

                    string filePath = path + "/" + modListFilename;

                    if (!File.Exists(filePath))
                        return;

                    if (!modLists.ContainsKey(worldName + "/" + saveName))
                        modLists.Add(worldName + "/" + saveName, new List<string>());
                    else
                        return;

                    XmlDocument document = new XmlDocument();
                    document.Load(filePath);

                    foreach (XmlNode node in document.DocumentElement.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            XmlElement element = (XmlElement)node;

                            if (element.Name.Equals("mod"))
                            {
                                if (!element.HasAttribute("name"))
                                    continue;

                                string mod = element.GetAttribute("name");

                                modLists[worldName + "/" + saveName].Add(mod);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(XUiC_SavegamesList.ListEntry))]
            [HarmonyPatch("GetBindingValue")]
            class XUiC_SavegamesListListEntryGetBindingValueHook
            {
                static void Postfix(XUiC_SavegamesList.ListEntry __instance, ref bool __result, ref string _value, string _bindingName)
                {
                    switch (_bindingName)
                    {
                        case "modstooltip":
                        case "moddedtext":
                            string worldName = __instance.worldName;
                            string saveName = __instance.saveName;

                            EModListState state = GetModList(worldName, saveName, out List<string> modList);

                            if (_bindingName == "modstooltip")
                            {
                                switch(state)
                                {
                                    case EModListState.VANILLA:
                                        _value = Localization.Get("xuiModsListStateVanilla");
                                        break;
                                    case EModListState.MODDED:
                                        _value = string.Format(Localization.Get("xuiModsListStateModded"), modList.StringFromList("\n"));
                                        break;
                                    case EModListState.UNKNOWN:
                                        _value = Localization.Get("xuiModsListStateUnknown");
                                        break;
                                }
                            }
                            else if (_bindingName == "moddedtext")
                            {
                                switch(state)
                                {
                                    case EModListState.VANILLA:
                                        _value = "V";
                                        break;
                                    case EModListState.MODDED:
                                        _value = "M";
                                        break;
                                    default:
                                        _value = "U";
                                        break;
                                }
                            }

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
                    switch (_bindingName)
                    {
                        case "modstooltip":
                            _value = "";
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
        }
    }
}
