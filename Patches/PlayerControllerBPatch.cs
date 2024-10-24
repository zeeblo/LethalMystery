using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Reflection;
using UnityEngine.UIElements;
using System.Xml.Linq;
using System.ComponentModel;



namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        
        private static bool check = true;

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(PlayerControllerB __instance)
        {
            //__instance.grabDistance = 400;

            if (Keyboard.current.digit8Key.wasPressedThisFrame)
            {

                Commands.SpawnEnemyFunc($"{GameNetworkManager.Instance.localPlayerController.playerClientId} nutcrack");

                /*
                // Get the private method "BeginGrabObject" then call it
                MethodInfo GrabTest = typeof(PlayerControllerB).GetMethod("BeginGrabObject", BindingFlags.NonPublic | BindingFlags.Instance);

                if (GrabTest != null)
                {
                    // Invoke the private method on the instance
                    GrabTest.Invoke(__instance, null);
                }
                else
                {
                    Plugin.mls.LogInfo("BeginGrabObject was not found");
                }
                */


            }

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                Plugin.DespawnNutcracker();
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.playerClientId}", toInventory: true);
                if (Commands.randomObject != null)
                {
                    GrabbableObject component = Commands.randomObject;
                    __instance.ItemSlots[3] = component;
                    HUDManager.Instance.itemSlotIcons[3].sprite = component.itemProperties.itemIcon;
                    HUDManager.Instance.itemSlotIcons[3].enabled = true;
                    component.parentObject = __instance.localItemHolder;
                }
                

            }

            /*
            if (check && StartOfRound.Instance.shipHasLanded)
            {
                Scene currentScene = SceneManager.GetSceneAt(0);
                foreach (GameObject obj in currentScene.GetRootGameObjects())
                {
                    Plugin.mls.LogInfo($"<> Object name: {obj.name} ");
                    if (obj.name.Contains("ShotgunItem"))
                    {
                        Plugin.mls.LogInfo(">>> TP shotgun on player");
                        obj.gameObject.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;
                        //obj.gameObject.transform.localScale = new Vector3(40f, 40f, 40f);
                        check = false;
                        break;
                    }
                }
            }
            */
        }


        [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
        [HarmonyPostfix]
        private static void BeginGrabObjectPatch(PlayerControllerB __instance)
        {
            // Check if item name being grabbed is shotgun
            // If it is, stop spawning and destroying nutcrackers
        }


    }
}
