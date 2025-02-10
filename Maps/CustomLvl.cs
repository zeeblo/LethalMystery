using HarmonyLib;
//using LethalLevelLoader;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;


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

            customKeywords.Add(new NewTerminalKeyword(
                word: "skeld",
                isVerb: false,
                specialKeywordResult: node ));

            customKeywords.Add(new NewTerminalKeyword(
                word: "office",
                isVerb: false,
                specialKeywordResult: node2));
        }


        private static void DefaultSetup()
        {
            CurrentInside = LMAssets.SkeldMap;
            StartOfRound.Instance.screenLevelDescription.text = $"Map: SKELD";
        }



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        private static bool MoonsInTerminal(Terminal __instance)
        {
            customMoons.Clear();
            customKeywords.Clear();
            LMmoons.Clear();
            AppendLMMoons();
            //AppendNewSelectableLevel();
            AppendNewTerminalKeyword();
            DefaultSetup();

            foreach (TerminalKeyword words in __instance.terminalNodes.allKeywords)
            {
                customKeywords.Add(words);
            }

            //__instance.moonsCatalogueList = customMoons.ToArray();
            __instance.terminalNodes.allKeywords = customKeywords.ToArray();

            return true;
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPrefix]
        [HarmonyAfter("imabatby.lethallevelloader")]
        private static void StartPatch(Terminal __instance)
        {


            foreach (TerminalKeyword twords in __instance.terminalNodes.allKeywords)
            {
                if (twords.word == "moons")
                {
                    string raw_result = twords.specialKeywordResult.displayText.Substring(0, 219);
                    string result = raw_result;
                    foreach (string moon in LMmoons)
                    {
                        result +=  $"\n* {StringAddons.Title(moon)} [planetTime]\n";
                    }

                    twords.specialKeywordResult.displayText = result + "\n\n";
                    break;
                }
            }
            /*
            string lll_result = "";
            foreach (TerminalKeyword twords in __instance.terminalNodes.allKeywords)
            {
                if (twords.word == "moons")
                {
                    foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.ExtendedDungeonFlows)
                    {
                        if ($"{extendedDungeonFlow.ContentType}".ToLower() == "custom")
                        {
                            Plugin.mls.LogInfo($">>> {extendedDungeonFlow.DungeonName}");
                            string raw_result = twords.specialKeywordResult.displayText;
                            lll_result += raw_result + $"\n* {extendedDungeonFlow.DungeonName} [planetTime]\n\n";
                            

                            NewTerminalNode node = new NewTerminalNode(
                                    displayText: $"Interior will now be {extendedDungeonFlow.DungeonName}\n\n",
                                    terminalEvent: "lll");

                            customKeywords.Add(new NewTerminalKeyword(
                                word: extendedDungeonFlow.DungeonName.ToLower(),
                                isVerb: false,
                                specialKeywordResult: node));
                        }
                    }
                    twords.specialKeywordResult.displayText = lll_result;
                    break;
                }

            }
            */



        }



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyPrefix]
        private static bool TerminalCommand(TerminalNode node)
        {
            string cmd = node.terminalEvent.Split('/')[0];

            if (node.terminalEvent.Contains("custom_map"))
            {
                ChangeMap(node.terminalEvent.Split('/')[1]);
            }
            Plugin.mls.LogInfo($">>>chs: {node.name}");
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


            Plugin.netHandler.currentMapReceive(map, 0u);
        }

    }
}
