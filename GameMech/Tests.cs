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
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
            [HarmonyPostfix]
            private static void Keys(PlayerControllerB __instance)
            {

                if (Keyboard.current.digit5Key.wasPressedThisFrame && entered == false)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = groundPosition;
                    sphere.transform.localScale = new Vector3(20, 20, 20);
                    Plugin.mls.LogInfo(">> loading intro");

                    // disable movement
                    MethodInfo OnDisable = typeof(PlayerControllerB).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
                    OnDisable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                    GameNetworkManager.Instance.localPlayerController.disableMoveInput = true;

                    // switch to no item
                    MethodInfo GetFirstEmptyItemSlot = typeof(PlayerControllerB).GetMethod("FirstEmptyItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                    int FirstEmptyItemSlot = (int)GetFirstEmptyItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, null);

                    MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
                    SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { FirstEmptyItemSlot, Type.Missing });

                    // look at camera
                    GameNetworkManager.Instance.localPlayerController.thisPlayerBody.rotation = Quaternion.identity;
                    GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles = new Vector3(0, 0, __instance.gameplayCamera.transform.localEulerAngles.z);

                    //
                    //__instance.isInHangarShipRoom = false;
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(modelPosition);


                    // deactivate default camera & do some hud stuff
                    GameObject.Find("Systems/UI/Canvas/Panel/").SetActive(false); // makes screen black
                    __instance.gameplayCamera.transform.gameObject.SetActive(false);
                    GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.enabled = false;
                    GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/").SetActive(false);
                    GameObject.Find("PlayersContainer/Player/Misc/MapDot").SetActive(false);



                    // reset animations
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Walking", value: false);
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sprinting", value: false);
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sideways", value: false);
                    GameNetworkManager.Instance.localPlayerController.Crouch(crouch: false);
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("FallNoJump", value: false);
                    GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
                    GameNetworkManager.Instance.localPlayerController.StopPerformingEmoteServerRpc();



                    Plugin.RemoveEnvironment(false);
                    //StartOfRound.Instance.starSphereObject.SetActive(value: true);

                    // darken bg
                    GameObject.Find("Systems/Rendering/VolumeMain")?.SetActive(false);
                    GameObject.Find("Environment/OutOfBoundsTrigger1")?.SetActive(false);
                    GameObject.Find("StoryLogs")?.SetActive(false);

                    CreateLights();


                    // spawn custom camera
                    StartOfRound.Instance.StartCoroutine(CreateCamera());

                    entered = true;
                }


                if (Keyboard.current.digit6Key.wasPressedThisFrame && entered)
                {

                    //  movement
                    MethodInfo OnEnable = typeof(PlayerControllerB).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
                    OnEnable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                    GameNetworkManager.Instance.localPlayerController.disableMoveInput = false;

                    // look at camera
                    //GameNetworkManager.Instance.localPlayerController.thisPlayerBody.rotation = Quaternion.identity;
                    //GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles = new Vector3(0, 0, __instance.gameplayCamera.transform.localEulerAngles.z);

                    //
                    //__instance.isInHangarShipRoom = false;
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);


                    // activate default camera & do some hud stuff
                    GameObject.Find("Systems/UI/Canvas/Panel/").SetActive(true); // makes screen black
                    GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.gameObject.SetActive(true);
                    GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.enabled = true;
                    GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/").SetActive(true);
                    GameObject.Find("PlayersContainer/Player/Misc/MapDot").SetActive(true);




                    Plugin.RemoveEnvironment(true);
                    //StartOfRound.Instance.starSphereObject.SetActive(value: true);

                    // darken bg
                    GameObject.Find("Systems/Rendering/VolumeMain")?.SetActive(true);
                    GameObject.Find("Environment/OutOfBoundsTrigger1")?.SetActive(true);
                    GameObject.Find("StoryLogs")?.SetActive(true);

                    DisableLights();


                    // spawn custom camera
                    StartOfRound.Instance.StartCoroutine(DisableCamera(__instance));

                    entered = false;
                }




            }



            private static IEnumerator CreateCamera()
            {
                yield return new WaitForSeconds(2f);
                introCamera = new GameObject("IntroCamera").AddComponent<Camera>();
                introCamera.transform.position = cameraPosition;
                introCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);

                Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
                Plugin.mls.LogInfo($"Default Screen: {canv.renderMode}");
                canv.renderMode = 0;
                canv.worldCamera = introCamera;

                StartOfRound.Instance.SwitchCamera(introCamera);
            }


            private static IEnumerator DisableCamera(PlayerControllerB __instance)
            {
                yield return new WaitForSeconds(2f);

                introCamera?.gameObject.SetActive(false);
                Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
                
                Plugin.mls.LogInfo($"Current Camera: {canv.worldCamera}");

                canv.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
                canv.renderMode = RenderMode.ScreenSpaceCamera;

                //StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);

                Plugin.mls.LogInfo($"After Camera: {canv.worldCamera}");
            }


            private static void CreateLights()
            {
                lght = GameObject.Find("Area Light (5)").gameObject;
                Quaternion rot = Quaternion.Euler(90, 0, 0);
                UnityEngine.Object.Instantiate(lght, lightPosition, rot);
            }

            private static void DisableLights()
            {
                lght?.SetActive(false);
            }


            [HarmonyPatch(typeof(PlayerControllerB), "PlayerLookInput")]
            [HarmonyPrefix]
            private static bool PlayerLookPatch()
            {
                if (entered)
                {
                    return false;
                }
                return true;
            }


            /*
            [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Update))]
            [HarmonyPostfix]
            private static void UpdatePatch(TimeOfDay __instance)
            {
                if (entered)
                {
                    __instance.sunDirect.enabled = false;
                    HDAdditionalLightData indirectLightData = (HDAdditionalLightData)Traverse.Create(__instance).Field("indirectLightData").GetValue();
                    Traverse.Create(__instance).Field("indirectLightData.lightDimmer").SetValue(Mathf.Lerp(indirectLightData.lightDimmer, 0f, 50f * Time.deltaTime));


                    // enable:
                    //  sunIndirect.enabled = true;
	                //   indirectLightData.lightDimmer = Mathf.Lerp(indirectLightData.lightDimmer, 1f, 5f * Time.deltaTime);
                    
                }

            }

            */
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
