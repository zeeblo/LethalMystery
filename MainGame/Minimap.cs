using UnityEngine.EventSystems;
using UnityEngine;
using LethalMystery.UI;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;



namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Minimap
    {

        public static Vector3 lastPlayerPos = Vector3.zero;
        public static Dictionary<string, string> allPlayerPoints = new Dictionary<string, string>();
        public static GameObject currentWaypoint;
        public static GameObject waypointPrefab;
        public static string currentPointUserID = "";


        public static void ResetVars()
        {
            allPlayerPoints.Clear();
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.Update))]
        [HarmonyPostfix]
        private static void MCRUpdatePatch(ManualCameraRenderer __instance)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (__instance.cam == null) return;
            if (MinimapUI.minimapCam == null) return;
            if (__instance.cam.name.Contains("MinimapCam"))
            {
                __instance.cam.enabled = true;

                if (Meeting.inMeeting.Value == "true")
                {
                    __instance.targetedPlayer = null;
                }
                else
                {
                    __instance.targetedPlayer = Plugin.localPlayer;
                    //lastPlayerPos = Plugin.localPlayer.transform.position;
                }
            }

        }


        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.MapCameraFocusOnPosition))]
        [HarmonyPrefix]
        private static bool SpectatePlayerPoint(ManualCameraRenderer __instance)
        {
            if (__instance.cam.name.Contains("MinimapCam") && Meeting.inMeeting.Value == "true")
            {
                //__instance.mapCamera.nearClipPlane = -2.47f;

                if (allPlayerPoints == null) return false;
                if (!allPlayerPoints.ContainsKey($"{Plugin.localPlayer.playerClientId}"))
                {
                    __instance.mapCamera.transform.position = lastPlayerPos;
                    currentPointUserID = $"{Plugin.localPlayer.playerClientId}";
                }
                else
                {
                    __instance.mapCamera.transform.position = StringAddons.ConvertToVector3(allPlayerPoints[currentPointUserID]);
                }
                

                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ScrollMouse_performed))]
        [HarmonyPrefix]
        private static bool ZoomPatch(InputAction.CallbackContext context)
        {
            if (MinimapUI.border == null || MinimapUI.minimapCam == null) return true;
            if (MinimapUI.border.activeSelf)
            {
                float num = context.ReadValue<float>();
                if (num < 0f)
                {
                    // zoom out
                    Camera cam = MinimapUI.minimapCam.GetComponent<Camera>();
                    float raw_size = cam.orthographicSize += 50;
                    float size = Mathf.Clamp(raw_size, 19.7f, 999f);
                    cam.orthographicSize = size;
                }
                else
                {
                    // zoom in
                    Camera cam = MinimapUI.minimapCam.GetComponent<Camera>();
                    float raw_size = cam.orthographicSize -= 50;
                    float size = Mathf.Clamp(raw_size, 19.7f, 999f);
                    cam.orthographicSize = size;
                }
                return false;
            }
            return true;
        }


    
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (MinimapUI.minimapCam == null) return;
            if (Plugin.localPlayer.isInHangarShipRoom && Meeting.inMeeting.Value == "false")
            {
                HidePlayerDots(false);
            }
            else
            {
                HidePlayerDots();
            }
        }



        public static void HidePlayerDots(bool value = true)
        {
            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                if (user != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    if (user.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
                    {
                        user.transform.Find("Misc/MapDot").gameObject.SetActive(!value);
                    }
                }

            }
        }



        public static Vector3 GetPlayerPoint(string playerID)
        {
            Dictionary<string, string> localPlayerPos = new Dictionary<string, string>();
            localPlayerPos = allPlayerPoints;
            foreach (KeyValuePair<string, string> plrPos in allPlayerPoints)
            {
                if (playerID == plrPos.Key)
                {
                    return StringAddons.ConvertToVector3(plrPos.Value);
                }
            }
            return Vector3.zero;
        }


        public static void ClearAllPoints()
        {
            Scene SampleScene = SceneManager.GetSceneAt(0);
            foreach (GameObject obj in SampleScene.GetRootGameObjects())
            {
                if (obj.name.ToLower().Contains("waypoint_"))
                {
                    Plugin.Destroy(obj.gameObject);
                }
            }

            if (Minimap.currentWaypoint != null)
            {
                Plugin.Destroy(Minimap.currentWaypoint);
            }

        }


        public class MinimapWaypoint: MonoBehaviour, IPointerClickHandler
        {

            public Camera minimapCamera;
            //public Transform playerTransform;
            //public GameObject waypointPrefab;
            public RectTransform minimapRectTransform;



            public void OnPointerClick(PointerEventData eventData)
            {
                if (Meeting.inMeeting.Value == "false" || Plugin.localPlayer.isPlayerDead) return;
                Plugin.mls.LogInfo(">>> Clack Clack CLick CLick");

                // Check if click was on the minimap
                if (RectTransformUtility.RectangleContainsScreenPoint(minimapRectTransform, eventData.position, eventData.pressEventCamera))
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        minimapRectTransform,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint);

                    Vector2 normalizedPos = new Vector2(
                        (localPoint.x + minimapRectTransform.rect.width * 0.5f) / minimapRectTransform.rect.width,
                        (localPoint.y + minimapRectTransform.rect.height * 0.5f) / minimapRectTransform.rect.height
                    );


                    Vector3 worldPos = CalculateWorldPosition(normalizedPos);
                    PlaceWaypoint(worldPos);

                    Plugin.mls.LogInfo($"Placing waypoint at: {worldPos}, Camera at: {minimapCamera.transform.position}, Click pos normalized: {normalizedPos}");
                }
            }


            private Vector3 CalculateWorldPosition(Vector2 normalizedPos)
            {
                Vector3 viewportPoint = new Vector3(normalizedPos.x, normalizedPos.y, minimapCamera.nearClipPlane);
                Vector3 worldPos = minimapCamera.ViewportToWorldPoint(viewportPoint);

                float playerYPos = GetPlayerYPos();
                worldPos.y = playerYPos;

                return worldPos;
            }

            private float GetPlayerYPos()
            {
                Int32.TryParse(currentPointUserID, out int playerID);
                float playerYPos = lastPlayerPos.y;

                if (allPlayerPoints.ContainsKey(currentPointUserID))
                {
                    playerYPos = StringAddons.ConvertToVector3(allPlayerPoints[currentPointUserID]).y;
                }

                return playerYPos;
            }



            private void PlaceWaypoint(Vector3 worldPosition)
            {

                if (Minimap.currentWaypoint != null)
                {
                    Plugin.Destroy(Minimap.currentWaypoint);
                }


                Minimap.currentWaypoint = Plugin.Instantiate(Minimap.waypointPrefab, worldPosition, Quaternion.identity);
                Minimap.currentWaypoint.SetActive(true);

                Plugin.netHandler.setWaypointReceive($"{Plugin.localPlayer.playerClientId}/{worldPosition}", Plugin.localPlayer.playerClientId);
            }



        }

    }
}
