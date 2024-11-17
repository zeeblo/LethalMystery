using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalMystery.GameMech
{

    [HarmonyPatch]
    internal class Tasks
    {
        public static bool checkingForItems = false;
        public static bool droppedItem = false;
        public static int currentQuota = 0;
        public static int maxQuota = 120;

        #region Patches
        /// <summary>
        /// Iterates through user's inventory and removes currentlyHeld item
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void TaskUpdate(PlayerControllerB __instance)
        {
            if (checkingForItems && StartOfRound.Instance.shipHasLanded)
            {
                if (__instance.ItemSlots[__instance.currentItemSlot] != null)
                {
                    if (!(__instance.ItemSlots[__instance.currentItemSlot].name.ToLower().Contains("shotgun")) && !(__instance.ItemSlots[__instance.currentItemSlot].name.ToLower().Contains("knife")))
                    {
                        __instance.DestroyItemInSlotAndSync(__instance.currentItemSlot);
                        HUDManager.Instance.itemSlotIcons[__instance.currentItemSlot].enabled = false;
                        __instance.carryWeight = 1f;
                        checkingForItems = false;
                        currentQuota += 10;
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
            if ((__instance.isInHangarShipRoom && StartOfRound.Instance.shipHasLanded && __instance.currentlyHeldObjectServer != null)
                && !(__instance.currentlyHeldObjectServer.itemProperties.itemName.ToLower().Contains("shotgun"))
                && !(__instance.currentlyHeldObjectServer.itemProperties.itemName.ToLower().Contains("knife")))
            {
                HUDManager.Instance.AddNewScrapFoundToDisplay(__instance.currentlyHeldObjectServer);
                droppedItem = true;
                return false;
            }
            return true;
        }





        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            StartOfRound.Instance.profitQuotaMonitorText.text = $"PROFIT QUOTA:\n${currentQuota} / ${maxQuota}";
        }


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
        private static bool DisplayItems()
        {
            checkingForItems = true;

            return true;
        }

        #endregion Patches




        public static void ResetVariables()
        {
            checkingForItems = false;
            droppedItem = false;
            currentQuota = 0;
            maxQuota = 120;
        }

    }
}
