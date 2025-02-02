using HarmonyLib;
using LethalMystery.Utils;


namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class CustomLvl
    {
        public static List<SelectableLevel> customMoons = new List<SelectableLevel>();
        public static List<TerminalKeyword> customKeywords = new List<TerminalKeyword>();
        public static List<TerminalNode> customNodes = new List<TerminalNode>();

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
                bool clearPreviousText = true,
                int maxCharactersToType = 25,
                int buyItemIndex = -1,
                int buyVehicleIndex = -1,
                bool isConfirmationNode = false,
                int buyRerouteToMoon = -1,
                int displayPlanetInfo = -1,
                int shipUnlockableID = -1,
                int itemCost = 0,
                bool buyUnlockable = false,
                bool returnFromStorage = false)
            {
                this.displayText = displayText;
                this.clearPreviousText = clearPreviousText;
                this.maxCharactersToType = maxCharactersToType;
                this.buyItemIndex = buyItemIndex;
                this.buyVehicleIndex = buyVehicleIndex;
                this.isConfirmationNode = isConfirmationNode;
                this.buyRerouteToMoon = buyRerouteToMoon;
                this.displayPlanetInfo = displayPlanetInfo;
                this.shipUnlockableID = shipUnlockableID;
                this.itemCost = itemCost;
                this.buyUnlockable = buyUnlockable;
                this.returnFromStorage = returnFromStorage;
            }
        }




        private static void AppendNewSelectableLevel()
        {
            customMoons.Add(new NewSelectableLevel("skeld"));
        }

        private static void AppendNewTerminalKeyword()
        {
            customKeywords.Add(new NewTerminalKeyword("skeld", false, new NewTerminalNode("Interior will now be Skeld\n\n")));
        }

        private static void AppendNewTerminalNode()
        {
            customNodes.Add(new NewTerminalNode());
        }






        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        private static bool MoonsInTerminal(Terminal __instance)
        {
            customMoons.Clear();
            customKeywords.Clear();
            customNodes.Clear();
            AppendNewSelectableLevel();
            AppendNewTerminalKeyword();
            AppendNewTerminalNode();
            

            foreach (TerminalKeyword words in __instance.terminalNodes.allKeywords)
            {
                customKeywords.Add(words);
            }

            /*
            foreach (TerminalNode node in __instance.terminalNodes.specialNodes)
            {
                customKeywords.Add(words);
            }
            */
            __instance.moonsCatalogueList = customMoons.ToArray();
            __instance.terminalNodes.allKeywords = customKeywords.ToArray();

            return true;
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPrefix]
        private static void StartPatch(Terminal __instance)
        {


            foreach (TerminalKeyword twords in __instance.terminalNodes.allKeywords)
            {
                if (twords.word == "moons")
                {
                    string raw_result = twords.specialKeywordResult.displayText.Substring(0, 219);
                    string result = "";
                    foreach (SelectableLevel moon in __instance.moonsCatalogueList)
                    {
                        result += raw_result + $"\n* {StringAddons.Title(moon.PlanetName)} [planetTime]\n\n";
                    }

                    twords.specialKeywordResult.displayText = result;
                    break;
                }
            }

        }

    }
}
