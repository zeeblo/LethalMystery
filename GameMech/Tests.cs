using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Steamworks.InventoryItem;
using static UnityEngine.Rendering.DebugUI;


namespace LethalMystery.GameMech
{

    internal class Tests
    {
        public static Vector3 modelPosition = new Vector3(80f, 20f, 20f);
        public static Vector3 groundPosition = new Vector3(80f, 18.8f, 20f);
        public static Vector3 cameraPosition = new Vector3(80f, 31.05f, 26f);
        public static Vector3 lightPosition = new Vector3(80f, 34f, 20f);
        public static GameObject? playerObj;
        public static GameObject? lght;
        public static Camera? introCamera;
        public static bool entered = false;
        public static bool entered6 = false;
        public static bool disableMovement = false;

        [HarmonyPatch]
        internal class AdminCMDS
        {
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
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }

            }
        }
    }
}
