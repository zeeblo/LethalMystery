using HarmonyLib;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;
using BepInEx.Configuration;
using BepInEx;
using System.Collections.Generic;
using System;
using System.IO;
using LethalMystery.MainGame;
using System.Linq;


namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class CustomLvl
    {
        public static List<SelectableLevel> customMoons = new List<SelectableLevel>();
        public static List<string> LMmoons = new List<string>();
        public static List<TerminalKeyword> customKeywords = new List<TerminalKeyword>();
        public static LNetworkVariable<string> mapName = LNetworkVariable<string>.Connect("mapName");
        public static GameObject CurrentInside;
        private static string default_maps = "";
        private static string vanilla_maps = "";
        private static string lll_maps = "";
        public static string localCurrentInside;
        public static string[] specialMapID = { "spawn_pos", "exit_pos", "scraps", "vents", "sabo" };

        private class NewSelectableLevel : SelectableLevel
        {

            public NewSelectableLevel(string planetname)
            {
                PlanetName = planetname;
            }

        }




        private class NewTerminalKeyword : TerminalKeyword
        {

            public NewTerminalKeyword(string word, bool isVerb, TerminalNode specialKeywordResult)
            {
                this.word = word;
                this.isVerb = isVerb;
                this.specialKeywordResult = specialKeywordResult;
            }

        }



        private class NewTerminalNode : TerminalNode
        {
            public NewTerminalNode(
                string displayText = "",
                string terminalEvent = "",
                bool clearPreviousText = true,
                bool isConfirmationNode = false,
                int itemCost = 0)
            {
                this.displayText = displayText;
                this.terminalEvent = terminalEvent;
                this.clearPreviousText = clearPreviousText;
                this.isConfirmationNode = isConfirmationNode;
                this.itemCost = itemCost;
            }
        }




        private static void AppendNewSelectableLevel()
        {
            customMoons.Add(new NewSelectableLevel("skeld"));
        }

        private static void AppendLMMoons()
        {
            LMmoons.Add("skeld");
            LMmoons.Add("office");
        }

        private static void AppendNewTerminalKeyword()
        {
            NewTerminalNode node = new NewTerminalNode(
                    displayText: "Interior will now be Skeld\n\n",
                    terminalEvent: "custom_map/skeld");

            NewTerminalNode node2 = new NewTerminalNode(
                    displayText: "Interior will now be Office\n\n",
                    terminalEvent: "custom_map/office");

            NewTerminalNode node3 = new NewTerminalNode(
                    displayText: "Ignore spaces in map names. Eg. \"My map\" should be typed \"mymap\".\n\nCustom Maps:\n\n\n\n",
                    terminalEvent: "cmd");

            NewTerminalNode node4 = new NewTerminalNode(
                    displayText: BuyItems.ShowAllItems() + "\n\n\n\n",
                    terminalEvent: "cmd");

            customKeywords.Add(new NewTerminalKeyword(
                word: "skeld",
                isVerb: false,
                specialKeywordResult: node));

            customKeywords.Add(new NewTerminalKeyword(
                word: "office",
                isVerb: false,
                specialKeywordResult: node2));

            customKeywords.Add(new NewTerminalKeyword(
                word: "moons",
                isVerb: false,
                specialKeywordResult: node3));

            customKeywords.Add(new NewTerminalKeyword(
                word: "store",
                isVerb: false,
                specialKeywordResult: node4));
        }


        private static void DefaultSetup()
        {
            CurrentInside = LMAssets.OfficeMap;
            StartOfRound.Instance.screenLevelDescription.text = $"Map: Office";
            localCurrentInside = "office";
        }



        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPrefix]
        private static bool MoonsInTerminal(Terminal __instance)
        {
            Plugin.terminal = __instance;
            customMoons.Clear();
            LMmoons.Clear();
            AppendLMMoons();
            DefaultSetup();
            RefreshCustomKeywords();

            return true;
        }


        public static void RefreshCustomKeywords()
        {
            customKeywords.Clear();
            AppendNewTerminalKeyword();
            Terminal term = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").gameObject.GetComponent<Terminal>();
            foreach (TerminalKeyword words in term.terminalNodes.allKeywords)
            {
                customKeywords.Add(words);
            }

            AddNewKeywords();

        }

        private static void AddNewKeywords()
        {
            Terminal term = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").gameObject.GetComponent<Terminal>();
            term.terminalNodes.allKeywords = customKeywords.ToArray();
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPostfix]
        private static void ShowMapsInTerminal()
        {
            DefaultMapsInTerminal();
            LLLMapsInTerminal();
            CombineMapsInTerminal();
            
        }


        private static void DefaultMapsInTerminal()
        {
            default_maps = "";
            vanilla_maps = "\nVanilla Maps:\n\n";
            foreach (string moon in LMmoons)
            {
                default_maps += $"\n* {StringAddons.Title(moon)}\n";
            }

            for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
            {
                string raw_name = StartOfRound.Instance.levels[i].name;
                if (!raw_name.ToLower().EndsWith("level") || StartOfRound.Instance.levels[i].levelID == 3 || StartOfRound.Instance.levels[i].levelID == 11) continue;

                string prefix = "Level";
                string name = raw_name.Substring(0, raw_name.Length - prefix.Length);
                vanilla_maps += $"\n* {name}\n";

            }

            default_maps += vanilla_maps;
        }


        private static void LLLMapsInTerminal()
        {
            lll_maps = "";
            // Player is using LethalLevelLoader, show custom maps
            /*
            if (Plugin.FoundThisMod("imabatby.lethallevelloader"))
            {
                foreach (LethalLevelLoader.ExtendedDungeonFlow extendedDungeonFlow in LethalLevelLoader.PatchedContent.ExtendedDungeonFlows)
                {
                    if ($"{extendedDungeonFlow.ContentType}".ToLower() == "custom")
                    {
                        lll_maps += $"\n* {extendedDungeonFlow.DungeonName}\n\n";

                        NewTerminalNode node = new NewTerminalNode(
                                displayText: $"Interior will now be {extendedDungeonFlow.DungeonName}\n\nIMPORTANT: Restart the game and type this dungeon name again (If you've done it already then just start the ship)\n\n",
                                terminalEvent: $"lll/{extendedDungeonFlow.DungeonName}");

                        string dungeon_name = extendedDungeonFlow.DungeonName.Replace(" ", "");

                        customKeywords.Add(new NewTerminalKeyword(
                            word: dungeon_name.ToLower(),
                            isVerb: false,
                            specialKeywordResult: node));
                    }
                }
                AddNewKeywords();
            }
            */
        }


        private static void CombineMapsInTerminal()
        {
            Terminal term = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").gameObject.GetComponent<Terminal>();
            foreach (TerminalKeyword twords in term.terminalNodes.allKeywords)
            {
                if (twords.word == "moons")
                {
                    string result = twords.specialKeywordResult.displayText.Substring(0, 83) + default_maps + lll_maps;
                    twords.specialKeywordResult.displayText = result + "\n\n";
                    break;
                }
            }

        }




        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyFinalizer]
        private static Exception ShowMapsInTerminalHandler(Exception __exception)
        {

            // Error was thrown because LethalLevelLoader mod was not loaded
            // use default custom maps instead.
            if (__exception != null)
            {
                if (__exception.GetType() == typeof(System.IO.FileNotFoundException))
                {
                    DefaultMapsInTerminal();
                    CombineMapsInTerminal();
                }
            }

            return null;
        }



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyPostfix]
        [HarmonyAfter("imabatby.lethallevelloader")]
        private static void TerminalCommand(TerminalNode node)
        {
            defaultInteriorCMD(node);
            LLLInteriorCMD(node);
        }



        private static void defaultInteriorCMD(TerminalNode node)
        {
            string cmd = node.terminalEvent.Split('/')[0];

            if (cmd.Contains("custom_map"))
            {
                ChangeMap(node.terminalEvent);
            }
        }


        private static void LLLInteriorCMD(TerminalNode node)
        {
            string cmd = node.terminalEvent.Split('/')[0];

            if (cmd.Contains("lll"))
            {
                UpdateLLLConfig(node.terminalEvent, 9999);
                ChangeMap(node.terminalEvent);
            }
        }



        /// <summary>
        /// (LethalLevelLoader Config Change)
        /// Set the typed interior to be guaranteed to spawn
        /// Set every other interior to 0
        /// </summary>
        private static void UpdateLLLConfig(string cmd, int weight = 0)
        {
            /*
           string mapName = cmd.Split("/")[1];
           string mapSection = "​​​​​​​​​Custom Dungeon:  " + mapName;
           string defaultFacility = "​​​​​​​Vanilla Dungeon:  Facility (Level1Flow)";
           Plugin.mls.LogInfo($">>> MapName: {mapName}");

           ConfigFile LLLConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "LethalLevelLoader.cfg"), false);
           ConfigEntry<bool> enableContentConfiguration = LLLConfigFile.Bind(mapSection, "Enable Content Configuration", false);
           ConfigEntry<string> manualLevelNames = LLLConfigFile.Bind(mapSection, "Dungeon Injection Settings - Manual Level Names List", "Experimentation: 0");

           ConfigEntry<bool> defaultFacility_enableContentConfiguration = LLLConfigFile.Bind(defaultFacility, "Enable Content Configuration", false);
           ConfigEntry<string> defaultFacility_manualLevelNames = LLLConfigFile.Bind(defaultFacility, "Dungeon Injection Settings - Manual Level Names List", "Experimentation: 0");



           foreach (LethalLevelLoader.ExtendedDungeonFlow extendedDungeonFlow in LethalLevelLoader.PatchedContent.ExtendedDungeonFlows)
           {
               if (extendedDungeonFlow.DungeonName == mapName) continue;

               string other_mapSection = "​​​​​​​​​Custom Dungeon:  " + extendedDungeonFlow.DungeonName;
               ConfigEntry<bool> other_enableContentConfiguration = LLLConfigFile.Bind(other_mapSection, "Enable Content Configuration", false);
               ConfigEntry<string> other_manualLevelNames = LLLConfigFile.Bind(other_mapSection, "Dungeon Injection Settings - Manual Level Names List", "Experimentation: 0");

               other_enableContentConfiguration.Value = true;
               other_manualLevelNames.Value = "Experimentation:0";
           }


           enableContentConfiguration.Value = true;
           manualLevelNames.Value = $"Experimentation:{weight}";

           defaultFacility_enableContentConfiguration.Value = true;
           defaultFacility_manualLevelNames.Value = "Experimentation:0";


           LLLConfigFile.Save();
           */
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyFinalizer]
        private static Exception TerminalCommandHandler(Exception __exception, TerminalNode node)
        {
            // Error was thrown because LethalLevelLoader mod was not loaded
            // use default custom maps instead.
            if (__exception != null)
            {
                if (__exception.GetType() == typeof(System.IO.FileNotFoundException))
                {
                    Plugin.mls.LogInfo(">>> LLL wasn't loaded so using doing default stuff");
                    defaultInteriorCMD(node);
                }
            }

            return null;
        }


        [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        private static bool FreeMoons(TerminalNode node)
        {
            if (!StartOfRound.Instance.inShipPhase) return true;
            if (node == null || node.buyRerouteToMoon == -1)
                return true;

            node.itemCost = 0;

            foreach (SelectableLevel lvl in StartOfRound.Instance.levels)
            {
                if (lvl.levelID == 3) continue;

                string name = lvl.PlanetName.Replace(" ", "-");
                string lll_name = lvl.PlanetName;
                if (node.displayText.Contains(name))
                {
                    node.itemCost = 0;
                }

                Plugin.mls.LogInfo($">>> reroute Num: {node.buyRerouteToMoon}");
                Plugin.mls.LogInfo($">>> displayText: {node.displayText}");
                Plugin.mls.LogInfo($">>> LLLplanetName: {lll_name}");
                if (node.buyRerouteToMoon > -1 && node.displayText.Contains(name))
                {
                    Plugin.mls.LogInfo($">>> Set to normal map: {name}");
                    ChangeMap($"normal/{name}");
                    break;
                }
                else if (node.buyRerouteToMoon > -1 && node.displayText.Contains(lll_name))
                {
                    Plugin.mls.LogInfo($">>> Set to normal map: {lll_name}");
                    ChangeMap($"normal/{lll_name}");
                    break;
                }
            }
            return true;
        }





        private static void ChangeMap(string map)
        {
            if (!Plugin.isHost)
            {
                HUDManager.Instance.DisplayTip("Oops", "Only the host can change maps");
                return;
            }
            if (!StartOfRound.Instance.inShipPhase)
            {
                HUDManager.Instance.DisplayTip("Oops", "Can only do this while in orbit.");
                return;
            }

            string mapName = map.Split('/')[1];
            localCurrentInside = mapName;

            Plugin.netHandler.currentMapReceive(map, 0u);
        }

    }
}
