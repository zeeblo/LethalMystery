using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class Minimap
    {

        public static GameObject minimap;
        public static GameObject border;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void SetupPatch()
        {
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                CreateBorder();
                CreateMinimap();
            }
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(ManualCameraRenderer __instance)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (__instance.cam == null) return;
            //__instance.screenEnabledOnLocalClient = true;
            Traverse.Create(__instance).Field("screenEnabledOnLocalClient").SetValue(true);
            __instance.cam.enabled = true;
            //__instance.shipArrowUI.SetActive(value: true);
        }



        private static void CreateMinimap()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            minimap = new GameObject("Minimap");
            minimap.transform.SetParent(border.transform, false);

            RawImage rawImg = minimap.AddComponent<RawImage>();
            ManualCameraRenderer manualCam = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorScript").GetComponent<ManualCameraRenderer>();
            rawImg.texture = manualCam.mapCamera.targetTexture;

            minimap.layer = 5; // UI Layer
            //minimap.transform.SetSiblingIndex(3);

            RectTransform rectMini = minimap.GetComponent<RectTransform>();
            rectMini.sizeDelta = new Vector2(190, 190);

            rectMini.anchorMin = new Vector2(1, 1);
            rectMini.anchorMax = new Vector2(1, 1);
            rectMini.pivot = new Vector2(1, 1);

            rectMini.anchoredPosition = Vector2.zero;
        }


        private static void CreateBorder()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            border = new GameObject("MinimapBorder");
            border.transform.SetParent(parentUI.transform, false);
            Image rawImg = border.AddComponent<Image>();

            border.layer = 5;
            border.transform.SetSiblingIndex(2);

            RectTransform rectBorder = border.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(200, 200); // new Vector3(rectMini.sizeDelta.x + 8, rectMini.sizeDelta.y + 8);

            rectBorder.anchorMin = new Vector2(1, 1); // new Vector2(rectMini.anchorMin.x, rectMini.anchorMin.y);
            rectBorder.anchorMax = new Vector2(1, 1); // new Vector2(rectMini.anchorMax.x, rectMini.anchorMax.y);
            rectBorder.pivot = new Vector2(1, 1); // new Vector2(rectMini.pivot.x, rectMini.pivot.y);

            rectBorder.anchoredPosition = new Vector2(-10, -10); // new Vector2(rectMini.anchoredPosition.x, rectMini.anchoredPosition.y);
        }

    }
}
