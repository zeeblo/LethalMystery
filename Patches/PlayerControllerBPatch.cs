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
using System.Collections;



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
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                Commands.SpawnEnemyFunc($"{GameNetworkManager.Instance.localPlayerController.playerClientId} nutcrack");
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
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

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                Commands.SpawnGun($"{GameNetworkManager.Instance.localPlayerController}", toInventory: true);
                if (Commands.gunObject != null)
                {
                    // Get the private method "BeginGrabObject" then call it
                    MethodInfo GrabTest = typeof(PlayerControllerB).GetMethod("BeginGrabObject", BindingFlags.NonPublic | BindingFlags.Instance);
                    GrabTest.Invoke(__instance, null);


                    //ShotgunItem component = Commands.gunObject;
                    //__instance.ItemSlots[3] = component;
                    //HUDManager.Instance.itemSlotIcons[3].sprite = component.itemProperties.itemIcon;
                    //HUDManager.Instance.itemSlotIcons[3].enabled = true;
                    //component.parentObject = __instance.localItemHolder;
                }


            }

            if (Keyboard.current.digit5Key.wasPressedThisFrame && check)
            {
                Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.playerClientId}", toInventory: true);

                if (Commands.randomObject != null)
                {
                    MethodInfo GrabTest = typeof(PlayerControllerB).GetMethod("BeginGrabObject", BindingFlags.NonPublic | BindingFlags.Instance);
                    GrabTest.Invoke(__instance, null);
                    check = false;
                }

            }


            if (check && StartOfRound.Instance.shipHasLanded)
            {
                Scene currentScene = SceneManager.GetSceneAt(0);
                foreach (GameObject obj in currentScene.GetRootGameObjects())
                {
                    if (obj.name.Contains("ShotgunItem"))
                    {
                        Commands.randomObject = obj.GetComponent<GrabbableObject>();

                        MethodInfo GrabTest = typeof(PlayerControllerB).GetMethod("BeginGrabObject", BindingFlags.NonPublic | BindingFlags.Instance);
                        GrabTest.Invoke(__instance, null);

                        check = false;
                        break;
                    }
                }
            }


            /*
            if (check && StartOfRound.Instance.shipHasLanded)
            {
                Scene currentScene = SceneManager.GetSceneAt(0);
                foreach (GameObject obj in currentScene.GetRootGameObjects())
                {
                    if (obj.name.Contains("ShotgunItem"))
                    {
                        GrabbableObject component = obj.GetComponent<GrabbableObject>();

                        component.startFallingPosition = GameNetworkManager.Instance.localPlayerController.transform.position;
                        component.targetFloorPosition = component.GetItemFloorPosition(GameNetworkManager.Instance.localPlayerController.transform.position);
                        component.SetScrapValue(10); // Set Scrap Value
                        __instance.ItemSlots[3] = component;
                        HUDManager.Instance.itemSlotIcons[3].sprite = component.itemProperties.itemIcon;
                        HUDManager.Instance.itemSlotIcons[3].enabled = true;
                        component.parentObject = __instance.localItemHolder;
                        check = false;
                        break;
                    }
                }
            }
            */

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
        [HarmonyPrefix]
        private static bool BeginGrabObjectPatch(PlayerControllerB __instance)
        {
            if (Commands.randomObject != null)
            {
                Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("currentlyGrabbingObject").SetValue(Commands.randomObject);
                Plugin.mls.LogInfo($">>> currentlyGrab: {Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("currentlyGrabbingObject").GetValue()}");

                if (!GameNetworkManager.Instance.gameHasStarted && !Commands.randomObject.itemProperties.canBeGrabbedBeforeGameStart && StartOfRound.Instance.testRoom == null)
                {
                    return false;
                }
                Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("grabInvalidated").SetValue(false);
                if (Commands.randomObject == null || __instance.inSpecialInteractAnimation || Commands.randomObject.isHeld || Commands.randomObject.isPocketed)
                {
                    return false;
                }
                NetworkObject networkObject = Commands.randomObject.NetworkObject;
                if (networkObject == null || !networkObject.IsSpawned)
                {
                    return false;
                }
                Commands.randomObject.InteractItem();
                //MethodInfo GetFirstEmptyItemSlot = typeof(PlayerControllerB).GetMethod("FirstEmptyItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                //GetFirstEmptyItemSlot.Invoke(__instance, null);

                if (Commands.randomObject.grabbable)
                {
                    __instance.playerBodyAnimator.SetBool("GrabInvalidated", value: false);
                    __instance.playerBodyAnimator.SetBool("GrabValidated", value: false);
                    __instance.playerBodyAnimator.SetBool("cancelHolding", value: false);
                    __instance.playerBodyAnimator.ResetTrigger("Throw");

                    Plugin.mls.LogInfo(">>> 1");
                    MethodInfo SetSpecialGrabAnimationBool = typeof(PlayerControllerB).GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
                    SetSpecialGrabAnimationBool.Invoke(__instance, new object[] { true, Type.Missing });

                    Plugin.mls.LogInfo(">>> 1.1");
                    //__instance.SetSpecialGrabAnimationBool(setTrue: true);
                    __instance.isGrabbingObjectAnimation = true;
                    __instance.cursorIcon.enabled = false;
                    __instance.cursorTip.text = "";
                    __instance.twoHanded = Commands.randomObject.itemProperties.twoHanded;
                    __instance.carryWeight = 1;
                    if (Commands.randomObject.itemProperties.grabAnimationTime > 0f)
                    {
                        __instance.grabObjectAnimationTime = Commands.randomObject.itemProperties.grabAnimationTime;
                    }
                    else
                    {
                        __instance.grabObjectAnimationTime = 0.4f;
                    }
                    if (!__instance.isTestingPlayer)
                    {
                        Plugin.mls.LogInfo(">>> 2");
                        MethodInfo GrabObjectServerRpc = typeof(PlayerControllerB).GetMethod("GrabObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                        GrabObjectServerRpc.Invoke(__instance, new object[] { (NetworkObjectReference)networkObject });
                    }

                    Plugin.mls.LogInfo(">>> 3");
                    Coroutine grabObjectCoroutine = (Coroutine)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("grabObjectCoroutine").GetValue();
                    if (grabObjectCoroutine != null)
                    {
                        Plugin.mls.LogInfo(">>> 4");
                        __instance.StopCoroutine(grabObjectCoroutine);
                    }

                    Plugin.mls.LogInfo(">>> 5");

                    MethodInfo GetGrabObject = typeof(PlayerControllerB).GetMethod("GrabObject", BindingFlags.NonPublic | BindingFlags.Instance);
                    Plugin.mls.LogInfo(">>> 6");

                    IEnumerator GrabObject = (IEnumerator)GetGrabObject.Invoke(__instance, null);

                    // Start the coroutine using the result
                    Traverse.Create(GameNetworkManager.Instance.localPlayerController)
                            .Field("grabObjectCoroutine")
                            .SetValue(__instance.StartCoroutine(GrabObject));



                    Plugin.mls.LogInfo(">>> 7");
                    Commands.randomObject = null;
                }
                return false;
            }
            return true;
        }


    }
}
