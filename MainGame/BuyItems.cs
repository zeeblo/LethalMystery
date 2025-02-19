using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LethalMystery.MainGame
{

    [HarmonyPatch]
    internal class BuyItems
    {

        public static Dictionary<string, int> itemsAndPrices = new Dictionary<string, int>();



        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
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
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyPostfix]
        private static void BuyPatch(TerminalNode node)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (node.displayText.ToLower().Contains("ordered"))
            {
                string raw_orderName = node.displayText.Split("[variableAmount] ")[1];
                int displayTextLen = raw_orderName.Length - 171;
                string orderName = raw_orderName.Substring(0, displayTextLen);
                SpawnShopItem(orderName);
            }
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


        private static void SpawnShopItem(string word)
        {
            Plugin.netHandler.buyItemReceive($"{Plugin.localPlayer.playerClientId}/{word}", Plugin.localPlayer.playerClientId);
        }


    }
}
