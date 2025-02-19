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

        /*
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.ParseWord))]
        [HarmonyPostfix]
        private static void ParseWord(Terminal __instance, string playerWord, int specificityRequired)
        {
            if (StartOfRound.Instance.inShipPhase) return;

            if (playerWord.Length < specificityRequired)
            {
                return;
            }
            TerminalKeyword terminalKeyword = null;
            for (int i = 0; i < __instance.terminalNodes.allKeywords.Length; i++)
            {
                bool hasGottenVerb = (bool)Traverse.Create(__instance).Field("hasGottenVerb").GetValue();
                if (__instance.terminalNodes.allKeywords[i].isVerb && hasGottenVerb)
                {
                    continue;
                }
                _ = __instance.terminalNodes.allKeywords[i].accessTerminalObjects;
                if (__instance.terminalNodes.allKeywords[i].word == playerWord)
                {
                    Plugin.mls.LogInfo($">>> playerWord: {playerWord}");
                    ContainsShopItemWord(playerWord);
                    return;
                    //return terminalNodes.allKeywords[i];
                }
                if (!(terminalKeyword == null))
                {
                    continue;
                }
                for (int num = playerWord.Length; num > specificityRequired; num--)
                {
                    if (__instance.terminalNodes.allKeywords[i].word.StartsWith(playerWord.Substring(0, num)))
                    {
                        terminalKeyword = __instance.terminalNodes.allKeywords[i];
                        Plugin.mls.LogInfo($">>> terminal word: {terminalKeyword.word} | playerWord: {playerWord}");
                        ContainsShopItemWord(terminalKeyword.word);
                        return;
                    }
                }
            }

        }
        */

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
        [HarmonyPostfix]
        private static void BuyPatch(TerminalNode node)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (node.displayText.ToLower().Contains("ordered"))
            {
                string raw_orderName = node.displayText.Split("[variableAmount] ")[1];
                string orderName = raw_orderName.Substring(0, raw_orderName.Length - 171);
                Plugin.mls.LogInfo($">>> orderName: {orderName}");
                ContainsShopItemWord(orderName);
            }
        }


        private static bool ContainsShopItemWord(string word)
        {
            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                Plugin.mls.LogInfo($">>> Item: {itm.itemName} | word: {word}");
                if (itm.itemName.Replace("-", " ").ToLower().StartsWith(word.ToLower()))
                {
                    Plugin.mls.LogInfo($">>> Buying: {itm.itemName} | ItemId: {itm.itemId} ");
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
