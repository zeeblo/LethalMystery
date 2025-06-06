using System.Collections.Generic;
using HarmonyLib;
using LethalMystery.Maps;

namespace LethalMystery.MainGame
{

    [HarmonyPatch]
    internal class BuyItems
    {

        public static Dictionary<string, int> itemsAndPrices = new Dictionary<string, int>();



        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void AddItemPrices(Terminal __instance)
        {
            itemsAndPrices.Clear();
            foreach (Item itm in __instance.buyableItemsList)
            {
                switch (itm.itemName)
                {
                    case "Walkie-talkie":
                        itemsAndPrices.Add(itm.itemName, 5);
                        break;
                    case "Flashlight":
                        itemsAndPrices.Add(itm.itemName, 2);
                        break;
                    case "Shovel":
                        itemsAndPrices.Add(itm.itemName, 999);
                        break;
                    case "Pro-flashlight":
                        itemsAndPrices.Add(itm.itemName, 4);
                        break;
                    case "Stun grenade":
                        itemsAndPrices.Add(itm.itemName, 2);
                        break;
                    case "TZP-Inhalant":
                        itemsAndPrices.Add(itm.itemName, 3);
                        break;
                    case "Zap gun":
                        itemsAndPrices.Add(itm.itemName, 4);
                        break;
                    case "Belt bag":
                        itemsAndPrices.Add(itm.itemName, 5);
                        break;
                }
            }
            CustomLvl.RefreshCustomKeywords();
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyPostfix]
        private static void BuyPatch(Terminal __instance, TerminalNode node)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (node.displayText.ToLower().Contains("ordered"))
            {
                string raw_orderName = node.displayText.Split("[variableAmount] ")[1];
                int displayTextLen = raw_orderName.Length - 171;
                string orderName = raw_orderName.Substring(0, displayTextLen);
                SpawnShopItem(orderName, __instance.playerDefinedAmount);
            }
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SyncGroupCreditsServerRpc))]
        [HarmonyPrefix]
        private static bool SyncCreditsPatch1()
        {
            return false;
        }



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SyncGroupCreditsClientRpc))]
        [HarmonyPrefix]
        private static bool SyncCreditsPatch2()
        {
            return false;
        }



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SyncTerminalValuesServerRpc))]
        [HarmonyPrefix]
        private static bool SyncCreditsPatch3()
        {
            return false;
        }



        public static void SetItemPrices()
        {
            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                if (itemsAndPrices.ContainsKey(itm.itemName))
                {
                    itm.creditsWorth = itemsAndPrices[itm.itemName];
                }
            }
        }



        public static void HideItems()
        {
            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                if (!itemsAndPrices.ContainsKey(itm.itemName))
                {
                    itm.creditsWorth = 999;
                }
            }
        }

        public static string ShowAllItems()
        {
            string items = "";
            foreach (KeyValuePair<string, int> i in itemsAndPrices)
            {
                items += $"* {i.Key}  //  Price: ${i.Value}\n";
            }

            return items;
        }


        private static void SpawnShopItem(string word, int amt)
        {
            Plugin.netHandler.buyItemReceive($"{Plugin.localPlayer.playerClientId}/{word}/{amt}", Plugin.localPlayer.playerClientId);
        }


    }
}
