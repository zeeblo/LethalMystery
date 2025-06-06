using System.Collections;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Maps;
using LethalMystery.Utils;
using UnityEngine.InputSystem;
using UnityEngine;

namespace LethalMystery.Players.Abilities
{
    [HarmonyPatch]
    internal class Vent
    {
        public static bool isInVent = false;
        public static float currentRotationX = 180f;
        private static GameObject ventCamera;
        public static bool venting = false;



        public static void ResetVars()
        {
            venting = false;
        }


        public class LM_VentCam : MonoBehaviour
        {
            public string thisIndex;
            public string parent;

        }


        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPostfix]
        private static void CameraRotation(PlayerControllerB __instance)
        {
            if (isInVent)
            {
                if (ventCamera == null)
                {
                    ventCamera = GameObject.Find("LM_ventCamera");
                }
                string parent_name = ventCamera.GetComponent<LM_VentCam>().parent;
                bool limitRotate = (parent_name.StartsWith("ground")) ? true : false;

                float minRotation = 180 - 20;
                float maxRotation = 180 + 20;
                float mouseX = __instance.playerActions.Movement.Look.ReadValue<Vector2>().x * 0.008f * IngamePlayerSettings.Instance.settings.lookSensitivity;

                float newRotation = currentRotationX + mouseX;
                newRotation = (limitRotate) ? newRotation : Mathf.Clamp(newRotation, minRotation, maxRotation);

                float rotationFix = newRotation - currentRotationX;
                ventCamera.transform.Rotate(0f, rotationFix, 0f);

                currentRotationX = newRotation;

            }
        }



        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPostfix]
        private static void EnterVentPatch(InteractTrigger __instance)
        {

            if (__instance.currentCooldownValue > 0f)
            {
                EnterVent();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Interact_performed")]
        [HarmonyPostfix]
        private static void ExitVentPatch()
        {
            if (isInVent)
            {
                ExitVent();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPostfix]
        private static void MoveToSwitchVent()
        {
            if (isInVent)
            {
                float num = Plugin.localPlayer.playerActions.Movement.Move.ReadValue<Vector2>().x;
                if (num > 0f)
                {
                    StartOfRound.Instance.StartCoroutine(camPositionDelay(forward: true));
                }
                else if (num < 0f)
                {
                    StartOfRound.Instance.StartCoroutine(camPositionDelay(forward: false));
                }
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPrefix]
        private static bool ScrollToSwitchVent(InputAction.CallbackContext context)
        {
            if (isInVent)
            {
                float num = context.ReadValue<float>();
                if (num > 0f)
                {
                    StartOfRound.Instance.StartCoroutine(camPositionDelay(forward: true));
                }
                else
                {
                    StartOfRound.Instance.StartCoroutine(camPositionDelay(forward: false));
                }
                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(PlayerControllerB), "CanUseItem")]
        [HarmonyPrefix]
        private static bool CanUseItemPatch()
        {
            if (isInVent)
            {
                return false;
            }
            return true;
        }


        private static void CreateVentCamera()
        {
            if (CustomLvl.CurrentInside == null) return;
            Camera ventCamera = new GameObject("LM_ventCamera").AddComponent<Camera>();
            ventCamera.tag = "PhysicsProp";

            GameObject raw_vent = GOTools.GetObjectPlayerIsLookingAt();
            string ventName = raw_vent.name;
            string ventParentName = raw_vent.transform.parent.name;

            bool groundVent = ventParentName.StartsWith("ground");
            string location = (groundVent) ? ventName : $"{ventName}/point";
            GameObject vent = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{ventName}");
            GameObject ventPoint = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{location}");


            LM_VentCam camComp = ventCamera.transform.gameObject.AddComponent<LM_VentCam>();
            camComp.thisIndex = ventName;
            camComp.parent = ventParentName;

            Vector3 ventpos = new Vector3(ventPoint.transform.position.x, ventPoint.transform.position.y, ventPoint.transform.position.z);
            ventCamera.transform.position = ventpos;

            if (groundVent == false)
            {
                ventCamera.transform.LookAt(vent.transform);
                ventCamera.transform.Rotate(0, 180, 0);
            }

            ventCamera.cullingMask = GameNetworkManager.Instance.localPlayerController.gameplayCamera.cullingMask;

            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.renderMode = 0;
            canv.worldCamera = ventCamera;
            StartOfRound.Instance.SwitchCamera(ventCamera);
        }

        private static void RemoveVentCamera()
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("PhysicsProp"))
            {
                if (obj.name == "LM_ventCamera")
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }

            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            canv.renderMode = RenderMode.ScreenSpaceCamera;

            if (GameNetworkManager.Instance.localPlayerController != null)
            {
                StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
            }
        }

        private static void EnterVent()
        {
            if (!(GOTools.GetObjectPlayerIsLookingAt().name.ToLower().Contains("vent"))) return;
            if (GameObject.Find("LM_ventCamera") != null) return;
            if (CustomLvl.CurrentInside == null) return;
            Plugin.mls.LogInfo(">>> Entered vent");

            GameObject.Find("Systems/UI/Canvas/Panel/")?.SetActive(false);

            CreateVentCamera();

            Plugin.localPlayer.thisPlayerBody.transform.Rotate(0, 180, 0);
            Plugin.localPlayer.disableMoveInput = true;
            GOTools.HidePlayerModel();
            Plugin.netHandler.hidePlayerReceive($"{Plugin.localPlayer.playerClientId}/hide", Plugin.localPlayer.playerClientId);

            HUDManager.Instance.PlayerInfo.targetAlpha = 0;
            HUDManager.Instance.Tooltips.targetAlpha = 0;
            HUDManager.Instance.Inventory.targetAlpha = 0;

            
            SoundManager.Instance.PlaySoundAroundLocalPlayer(StartOfRound.Instance.damageSFX, 1);
            HUDManager.Instance.UIAudio.PlayOneShot(LMAssets.SFX_VentOpen);
            isInVent = true;
        }


        private static void ExitVent()
        {
            if (CustomLvl.CurrentInside == null) return;
            Plugin.mls.LogInfo(">>> Exited vent");
            LM_VentCam ventComp = GameObject.Find("LM_ventCamera").GetComponent<LM_VentCam>();
            string ventParentName = ventComp.parent;
            string ventName = ventComp.thisIndex;
            string location = (ventParentName.StartsWith("ground")) ? ventName : $"{ventName}/point";
            Vector3 raw_pos = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{location}").transform.position;
            Vector3 pos = new Vector3(raw_pos.x, raw_pos.y + 1f, raw_pos.z);

            Plugin.localPlayer.TeleportPlayer(pos);
            RemoveVentCamera();
            GameObject.Find("Systems/UI/Canvas/Panel/").SetActive(true);
            Plugin.localPlayer.disableMoveInput = false;
            GOTools.HidePlayerModel(false);
            Plugin.netHandler.hidePlayerReceive($"{Plugin.localPlayer.playerClientId}/show", Plugin.localPlayer.playerClientId);


            HUDManager.Instance.PlayerInfo.targetAlpha = 1;
            HUDManager.Instance.Tooltips.targetAlpha = 1;
            HUDManager.Instance.Inventory.targetAlpha = 1;


            HUDManager.Instance.UIAudio.PlayOneShot(LMAssets.SFX_VentLeave);
            isInVent = false;
        }



        private static void CameraPosition(bool forward = true)
        {
            if (ventCamera == null)
            {
                ventCamera = GameObject.Find("LM_ventCamera");
            }

            if (ventCamera == null) return;

            LM_VentCam ventComp = ventCamera.GetComponent<LM_VentCam>();
            int rawIndex = InsideMap.allVents[ventComp.parent].IndexOf(ventComp.thisIndex);

            int newIndex = (forward) ? rawIndex + 1 : rawIndex - 1;
            if (newIndex > InsideMap.allVents[ventComp.parent].Count - 1)
            {
                newIndex = 0;
            }
            else if (newIndex < 0)
            {
                newIndex = InsideMap.allVents[ventComp.parent].Count - 1;
            }

            ventComp.thisIndex = InsideMap.allVents[ventComp.parent][newIndex];

            SwitchPosition(ventComp.parent, ventComp.thisIndex);
        }

        private static IEnumerator camPositionDelay(bool forward)
        {
            if (venting == false)
            {
                venting = true;
                yield return new WaitForSeconds(0.25f);
                CameraPosition(forward);

                venting = false;
            }
        }

        private static void SwitchPosition(string ventParentName, string ventName)
        {
            if (CustomLvl.CurrentInside == null) return;
            string location = (ventParentName.StartsWith("ground")) ? ventName : $"{ventName}/point";
            GameObject vent = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{ventName}");
            GameObject ventPoint = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{location}");
            if (vent == null || ventPoint == null) return;


            Vector3 ventpos = new Vector3(ventPoint.transform.position.x, ventPoint.transform.position.y, ventPoint.transform.position.z);
            ventCamera.transform.position = ventpos;

            if (!ventParentName.StartsWith("ground"))
            {
                ventCamera.transform.LookAt(vent.transform);
                ventCamera.transform.Rotate(0, 180, 0);
            }

            SoundManager.Instance.PlaySoundAroundLocalPlayer(Plugin.enemyVent.ventCrawlSFX, 1);
            HUDManager.Instance.UIAudio.PlayOneShot(LMAssets.SFX_VentSwitch);
        }



    }
}
