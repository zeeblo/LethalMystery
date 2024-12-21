using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using System.Collections;
using System.Reflection;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using LethalMystery.Patches;

namespace LethalMystery.GameMech
{

    [HarmonyPatch]
    internal class AutoGiveItem
    {
        public static bool checkedForWeapon = false;
        public static bool doneSpawningWeapons = false;

        #region Patches
        /// <summary>
        /// Checks if a role item exists in the scene and gives it to the user once the ship lands
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void CheckForWeapon(PlayerControllerB __instance)
        {
            if (doneSpawningWeapons && checkedForWeapon == false && Roles.CurrentRole != null)
            {
                if (Roles.CurrentRole.GetItem() == "")
                {
                    checkedForWeapon = true;
                    Plugin.mls.LogInfo(">>> No weapon for this role");
                }
                else
                {
                   
                    Scene currentScene = SceneManager.GetSceneAt(0);
                    foreach (GameObject obj in currentScene.GetRootGameObjects())
                    {
                        //Plugin.mls.LogInfo($">>> Item: {obj.name}");
                        GrabbableObject holdableItem = obj.GetComponent<GrabbableObject>();
                        if (obj.name.ToLower().Contains(Roles.CurrentRole.GetItem()) && holdableItem != null && holdableItem.playerHeldBy == null)
                        {
                            Plugin.mls.LogInfo(">>> in CheckForWeapon");
                            Commands.randomObject = obj.GetComponent<GrabbableObject>();
                            Commands.randomObject.itemProperties.itemIcon = Roles.CurrentRole.GetIcon(Commands.randomObject.itemProperties.itemIcon);
                            Commands.randomObject.itemProperties.weight = 1f;
                            MethodInfo GrabTest = typeof(PlayerControllerB).GetMethod("BeginGrabObject", BindingFlags.NonPublic | BindingFlags.Instance);
                            GrabTest.Invoke(__instance, null);
                            checkedForWeapon = true;
                            break;
                        }
                    }

                }

                StartOfRound.Instance.StartCoroutine(DespawnTimer());
            }
        }

        private static IEnumerator DespawnTimer()
        {
            yield return new WaitForSeconds(3);
            Plugin.DespawnEnemies();
        }

        
        [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
        [HarmonyPrefix]
        private static bool BeginGrabObjectPatch(PlayerControllerB __instance)
        {
            if (Commands.randomObject != null)
            {
                Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("currentlyGrabbingObject").SetValue(Commands.randomObject);
                //Plugin.mls.LogInfo($">>> currentlyGrab: {Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("currentlyGrabbingObject").GetValue()}");

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

                    MethodInfo SetSpecialGrabAnimationBool = typeof(PlayerControllerB).GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
                    SetSpecialGrabAnimationBool.Invoke(__instance, new object[] { true, Type.Missing });

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
                        MethodInfo GrabObjectServerRpc = typeof(PlayerControllerB).GetMethod("GrabObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                        GrabObjectServerRpc.Invoke(__instance, new object[] { (NetworkObjectReference)networkObject });
                    }

                    Coroutine grabObjectCoroutine = (Coroutine)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("grabObjectCoroutine").GetValue();
                    if (grabObjectCoroutine != null)
                    {
                        __instance.StopCoroutine(grabObjectCoroutine);
                    }

                    MethodInfo GetGrabObject = typeof(PlayerControllerB).GetMethod("GrabObject", BindingFlags.NonPublic | BindingFlags.Instance);

                    IEnumerator GrabObject = (IEnumerator)GetGrabObject.Invoke(__instance, null);

                    Traverse.Create(GameNetworkManager.Instance.localPlayerController)
                            .Field("grabObjectCoroutine")
                            .SetValue(__instance.StartCoroutine(GrabObject));


                    Commands.randomObject = null;
                }
                return false;
            }
            return true;
        }
        




        #endregion Patches




        public static void ResetVariables()
        {
            ButlerEnemyAIPatch.spawnedButlerForKnife = false;
            NutcrackerEnemyAIPatch.spawnedNutForWeapon = false;
            checkedForWeapon = false;
            doneSpawningWeapons = false;
        }

    }
}
