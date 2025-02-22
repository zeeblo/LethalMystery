using UnityEngine.EventSystems;
using UnityEngine;
using LethalMystery.UI;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Minimap
    {

        public static Vector3 lastPlayerPos = Vector3.zero;


        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.Update))]
        [HarmonyPostfix]
        private static void MCRUpdatePatch(ManualCameraRenderer __instance)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (__instance.cam == null) return;
            if (MinimapUI.minimapCam == null) return;

            Traverse.Create(__instance).Field("screenEnabledOnLocalClient").SetValue(!StringAddons.ConvertToBool(Meeting.inMeeting.Value));
            __instance.cam.enabled = !StringAddons.ConvertToBool(Meeting.inMeeting.Value);
        }



        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ScrollMouse_performed))]
        [HarmonyPostfix]
        private static void ZoomPatch(InputAction.CallbackContext context)
        {
            if (MinimapUI.border == null || MinimapUI.minimapCam == null) return;
            if (MinimapUI.border.activeSelf)
            {
                float num = context.ReadValue<float>();
                if (num < 0f)
                {
                    // zoom out
                    // GameObject mapCam = GameObject.Find("Systems/GameSystems/ItemSystems/MapCamera");
                    //GameObject mapCam = GameObject.Find("MinimapCam");
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
            }
        }


    
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (MinimapUI.minimapCam == null) return;
            if (Plugin.localPlayer.isInHangarShipRoom == false || Meeting.inMeeting.Value == "true")
            {
                HidePlayerDots();
            }
            else
            {
                HidePlayerDots(false);
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



        public class MinimapWaypoint: MonoBehaviour, IPointerClickHandler
        {

            public Camera minimapCamera;
            public Transform playerTransform;
            public GameObject waypointPrefab;
            public RectTransform minimapRectTransform;

            private GameObject currentWaypoint;



            public void OnPointerClick(PointerEventData eventData)
            {
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
                worldPos.y = playerTransform.position.y;

                return worldPos;
            }



            private void PlaceWaypoint(Vector3 worldPosition)
            {

                if (currentWaypoint != null)
                {
                    Destroy(currentWaypoint);
                }

                currentWaypoint = Instantiate(waypointPrefab, worldPosition, Quaternion.identity);
                currentWaypoint.SetActive(true);

            }


            public void ClearWaypoint()
            {
                if (currentWaypoint != null)
                {
                    Destroy(currentWaypoint);
                    currentWaypoint = null;
                }
            }
        }

    }
}
