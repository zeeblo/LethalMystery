using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LethalMystery.GameMech
{

    internal class Tests
    {
        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class AdminCMDS
        {
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
            [HarmonyPostfix]
            private static void Keys(PlayerControllerB __instance)
            {
                // should also only run if isInHangarShipRoom is true
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
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

                }
            }
        }



        [HarmonyPatch(typeof(HUDManager))]
        internal class AdminCMDS_2
        {
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
            [HarmonyPostfix]
            private static void Keys(HUDManager __instance)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame)
                {
                    __instance.drunknessFilter.weight = 15f;
                }
                if (Keyboard.current.digit2Key.wasPressedThisFrame)
                {
                    __instance.insanityScreenFilter.weight = 5f;
                }
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit5Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }

            }
        }
    }
}
