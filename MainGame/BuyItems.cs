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
        private static void BuyPatch(Terminal __instance)
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
                ContainsShopItemWord(orderName);
            }
        }


        public static void SetItemPrices()
        {
            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                if (itemsAndPrices.ContainsKey(itm.itemName))
                {
                    itm.creditsWorth = itemsAndPrices[itm.itemName];
                    Plugin.mls.LogInfo($">>> Set price to: {itm.itemName}");
                }
            }
        }

        private static bool ContainsShopItemWord(string word)
        {
            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                if (itm.itemName.Replace("-", " ").ToLower().StartsWith(word.ToLower()))
                {
                    Vector3 pos = new Vector3(Plugin.localPlayer.transform.position.x, Plugin.localPlayer.transform.position.y + 2.3f, Plugin.localPlayer.transform.position.z);
                    GameObject boughtItem = UnityEngine.Object.Instantiate(itm.spawnPrefab, pos, Quaternion.identity);
                    boughtItem.GetComponent<NetworkObject>().Spawn(true);
                    return true;
                }
            }
            return false;
        }


    }
}
