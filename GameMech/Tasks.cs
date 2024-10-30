using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalMystery.GameMech
{
    internal class Tasks
    {
        public static bool checkingForItems = false;

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class Items
        {

            /// <summary>
            /// Iterates through user's inventory and removes every item
            /// except for shotguns and knives
            /// </summary>
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
            [HarmonyPostfix]
            private static void TaskUpdate(PlayerControllerB __instance)
            {
                // Note that DisplayNewScrapFound may be called when a user drops an item as well
                if (checkingForItems && StartOfRound.Instance.shipHasLanded)
                {
                    Plugin.mls.LogInfo($">>> (1) Length of ItemSlots: {__instance.ItemSlots.Length}");

                    for (int i = 0; i < __instance.ItemSlots.Length; i++)
                    {
                        Plugin.mls.LogInfo($">>> {i}");

                        if (__instance.ItemSlots[i] != null)
                        {
                            Plugin.mls.LogInfo($"name: {__instance.ItemSlots[i].name}");
                            if (!(__instance.ItemSlots[i].name.ToLower().Contains("shotgun")) && !(__instance.ItemSlots[i].name.ToLower().Contains("knife")))
                            {
                                Plugin.mls.LogInfo($">>> Items in inv: {__instance.ItemSlots[i]}");
                                __instance.DestroyItemInSlotAndSync(i);
                                HUDManager.Instance.itemSlotIcons[i].enabled = false;
                                __instance.carryWeight = 1f;
                            }
                        }
                    }
                    StartOfRound.Instance.StartCoroutine(SetCheck(3));
                    //checkingForItems = false;
                }

            }

            private static IEnumerator SetCheck(float amount)
            {
                //Note: When player is in hangarship, return false for DropHeldItem or smthing like that
                yield return new WaitForSeconds(amount);
                checkingForItems = false;
            }


            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
            [HarmonyPrefix]
            private static bool DiscardHeldObjectPatch(PlayerControllerB __instance)
            {
                if (__instance.isInHangarShipRoom && StartOfRound.Instance.shipHasLanded)
                {
                    return false;
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(HUDManager))]
        internal class Assignment
        {

            /// <summary>
            /// Prevents Guns and Knives from popping up when entering the ship
            /// </summary>
            [HarmonyPatch(typeof(HUDManager), "DisplayNewScrapFound")]
            [HarmonyPrefix]
            private static bool DontDisplayWeapons(HUDManager __instance)
            {
                for (int i = 0; i < __instance.itemsToBeDisplayed.Count(); i++ )
                {
                    if (__instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("shotgun") || __instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("knife"))
                    {
                        __instance.itemsToBeDisplayed.Remove(__instance.itemsToBeDisplayed[i]);
                    }
                }

                //Note: This is will always set checkingForItems to true at the start.
                checkingForItems = true;

                
                return true;
            }

        }
    }
}
