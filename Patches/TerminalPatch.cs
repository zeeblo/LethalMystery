using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void Postfix(Terminal __instance)
        {

            /*
            Item[] allItems = __instance.buyableItemsList;
            foreach (Item itm in allItems)
            {
                Plugin.mls.LogInfo(">>>Item Name ES: " + itm.itemName);
                itm.creditsWorth = 0;
            }
            */

        }
    }
}
