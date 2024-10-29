using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalMystery.GameMech
{
    internal class Tasks
    {

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class Items
        {

        }

        [HarmonyPatch(typeof(HUDManager))]
        internal class Assignment
        {
            [HarmonyPatch(typeof(HUDManager), "DisplayNewScrapFound")]
            [HarmonyPrefix]
            private static bool DisplayNewScrapFoundPatch(HUDManager __instance)
            {

                for (int i = 0; i < __instance.itemsToBeDisplayed.Count(); i++ )
                {
                    Plugin.mls.LogInfo($">>> ItemName: {__instance.itemsToBeDisplayed[i].itemProperties.itemName}");
                    if (__instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("shotgun") || __instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("knife"))
                    {
                        Plugin.mls.LogInfo($">>> made it through if-st");
                        __instance.itemsToBeDisplayed.Remove(__instance.itemsToBeDisplayed[i]);
                    }
                }


                return true;
            }

        }
    }
}
