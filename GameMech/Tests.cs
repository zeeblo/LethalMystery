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
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("jar", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit5Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
            }
        }
    }
}
