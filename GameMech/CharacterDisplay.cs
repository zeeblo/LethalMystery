using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using LethalMystery.Players;
using Unity.Netcode;

namespace LethalMystery.GameMech
{
    [HarmonyPatch]
    public class CharacterDisplay
    {

        public static Vector3 modelPosition = new Vector3(-80f, 20f, 20f);
        public static Vector3 groundPosition = new Vector3(-80f, 18.8f, 20f);
        public static Vector3 cameraPosition = new Vector3(-80f, 31.05f, 26f);
        public static Vector3 lightPosition = new Vector3(-80f, 34f, 20f);
        public static GameObject? lght;
        public static Camera? introCamera;
        public static GameObject? sphere;
        public static bool inIntro = false;
        public static bool disableMovement = false;


        public static void CreateSphere()
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = groundPosition;
            sphere.transform.localScale = new Vector3(20, 20, 20);
        }

        public static void ShowSphere(bool value)
        {
            if (sphere != null)
            {
                sphere.SetActive(value);
            }
            else
            {
                CreateSphere();
            }
        }

        public static void EnableMovement(bool value)
        {
            if (value)
            {
                MethodInfo OnEnable = typeof(PlayerControllerB).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
                OnEnable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                GameNetworkManager.Instance.localPlayerController.disableMoveInput = false;
            }
            else
            {
                MethodInfo OnDisable = typeof(PlayerControllerB).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
                OnDisable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                GameNetworkManager.Instance.localPlayerController.disableMoveInput = true;
            }

        }

        /// <summary>
        /// If user is holding a weapon, switch to the next slot.
        /// (This doesn't specifically check if it's a weapon but that's how it's used)
        /// </summary>
        public static void SwitchToNextItem()
        {
            MethodInfo GetFirstEmptyItemSlot = typeof(PlayerControllerB).GetMethod("FirstEmptyItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            int FirstEmptyItemSlot = (int)GetFirstEmptyItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, null);

            MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { FirstEmptyItemSlot, Type.Missing });
        }

        public static void LookAtCamera()
        {
            GameNetworkManager.Instance.localPlayerController.thisPlayerBody.rotation = Quaternion.identity;
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles = new Vector3(0, 0, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles.z);
        }

        public static void ToggleView(bool value)
        {
            GameObject.Find("Systems/UI/Canvas/Panel/")?.SetActive(value); // makes screen black
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.gameObject?.SetActive(value);
            GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.enabled = value;
            GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/")?.SetActive(value);
            GameObject.Find("PlayersContainer/Player/Misc/MapDot")?.SetActive(value);
        }

        public static void ResetAnimation()
        {
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Walking", value: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sprinting", value: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sideways", value: false);
            GameNetworkManager.Instance.localPlayerController.Crouch(crouch: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("FallNoJump", value: false);
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            GameNetworkManager.Instance.localPlayerController.StopPerformingEmoteServerRpc();
        }


        public static void EnvironmentLight(bool light)
        {
            GameObject.Find("Systems/Rendering/VolumeMain")?.SetActive(light);
            GameObject.Find("Environment/OutOfBoundsTrigger1")?.SetActive(light);
            GameObject.Find("StoryLogs")?.SetActive(light);
        }

        private static void CreateLights()
        {
            lght = GameObject.Find("Area Light (5)").gameObject;
            Quaternion rot = Quaternion.Euler(90, 0, 0);
            UnityEngine.Object.Instantiate(lght, lightPosition, rot);
        }

        private static void ShowLights(bool value)
        {
            if (lght != null)
            {
                lght.SetActive(value);
            }
            else
            {
                CreateLights();
            }

        }


        public static void IntroCamValues()
        {
            if (introCamera != null)
            {
                introCamera.transform.position = cameraPosition;
                introCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);

                Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
                Plugin.mls.LogInfo($"Default Screen: {canv.renderMode}");
                canv.renderMode = 0;
                canv.worldCamera = introCamera;
            }
        }

        private static void CreateCamera()
        {
            introCamera = new GameObject("IntroCamera").AddComponent<Camera>();
            introCamera.transform.position = cameraPosition;
            introCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);

            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.renderMode = 0;
            canv.worldCamera = introCamera;
            StartOfRound.Instance.SwitchCamera(introCamera);
        }

        public static void IntroCamera()
        {
            if (introCamera != null)
            {
                IntroCamValues();
                StartOfRound.Instance.SwitchCamera(introCamera);
            }
            else
            {
                CreateCamera();
            }
        }

        private static void DisableIntroCamera()
        {
            introCamera?.gameObject.SetActive(false);
            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            canv.renderMode = RenderMode.ScreenSpaceCamera;

            StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
        }



        public static IEnumerator IntroDisplay()
        {
            ShowSphere(true);
            EnableMovement(false);
            LookAtCamera();
            ResetAnimation();
            yield return new WaitForSeconds(1.5f);
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(modelPosition);


            Plugin.RemoveEnvironment(true);
            EnvironmentLight(false);
            ShowLights(true);

            
            yield return new WaitForSeconds(2f);
            ToggleView(false);
            IntroCamera();
            SwitchToNextItem();
            

            yield return new WaitForSeconds(8f);

            ShowSphere(false);
            EnableMovement(true);
            LookAtCamera();
            ToggleView(true);

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);

            Plugin.RemoveEnvironment(false);
            EnvironmentLight(true);
            ShowLights(false);


            DisableIntroCamera();
            if (Roles.CurrentRole != null)
            {
                Roles.ShowRole(Roles.CurrentRole);
            }
            
            inIntro = false;

        }


        [HarmonyPatch(typeof(PlayerControllerB), "PlayerLookInput")]
        [HarmonyPrefix]
        private static bool PlayerLookPatch()
        {
            if (inIntro)
            {
                return false;
            }
            return true;
        }


    }
}
