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
        public static bool droppedItem = false;

        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class Items
        {

            /// <summary>
            /// Iterates through user's inventory and removes currentlyHeld item
            /// </summary>
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
            [HarmonyPostfix]
            private static void TaskUpdate(PlayerControllerB __instance)
            {
                if (checkingForItems && StartOfRound.Instance.shipHasLanded)
                {
                    for (int i = 0; i < __instance.ItemSlots.Length; i++)
                    {
                        if (__instance.ItemSlots[i] != null)
                        {
                            if (!(__instance.ItemSlots[i].name.ToLower().Contains("shotgun")) && !(__instance.ItemSlots[i].name.ToLower().Contains("knife")))
                            {
                                __instance.DestroyItemInSlotAndSync(i);
                                HUDManager.Instance.itemSlotIcons[i].enabled = false;
                                __instance.carryWeight = 1f;
                                checkingForItems = false;
                                break;
                            }
                        }
                    }

                }

            }


            /// <summary>
            /// Prevent user from dropping items in ship and collects it
            /// </summary>
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
            [HarmonyPrefix]
            private static bool CollectItem(PlayerControllerB __instance)
            {
                if ( (__instance.isInHangarShipRoom && StartOfRound.Instance.shipHasLanded && __instance.currentlyHeldObjectServer != null)
                    && !(__instance.currentlyHeldObjectServer.itemProperties.itemName.ToLower().Contains("shotgun"))
                    && !(__instance.currentlyHeldObjectServer.itemProperties.itemName.ToLower().Contains("knife")))
                {
                    HUDManager.Instance.AddNewScrapFoundToDisplay(__instance.currentlyHeldObjectServer);
                    droppedItem = true;
                    return false;
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(HUDManager))]
        internal class Assignment
        {


            /// <summary>
            /// If user attempted to drop an item then call DisplayNewScrapFound
            /// </summary>
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
            [HarmonyPostfix]
            private static void CheckCollectItem()
            {
                if (droppedItem)
                {
                    HUDManager.Instance.DisplayNewScrapFound();
                    droppedItem = false;
                }
            }


            /// <summary>
            /// Removes Guns and Knives from displaying when in the ship
            /// </summary>
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AddNewScrapFoundToDisplay))]
            [HarmonyPostfix]
            private static void DontDisplayWeapons(HUDManager __instance)
            {
                for (int i = 0; i < __instance.itemsToBeDisplayed.Count(); i++)
                {
                    if (__instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("shotgun") || __instance.itemsToBeDisplayed[i].itemProperties.itemName.ToLower().Contains("knife"))
                    {
                        __instance.itemsToBeDisplayed.Remove(__instance.itemsToBeDisplayed[i]);
                    }
                }
            }


            /// <summary>
            /// if DisplayNewScrapFound is called then remove currently selected item
            /// </summary>
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.DisplayNewScrapFound))]
            [HarmonyPrefix]
            private static bool DisplayItems(HUDManager __instance)
            {
                checkingForItems = true;

                return true;
            }

        }
    }
}
