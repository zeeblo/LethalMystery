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

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                CreateMinimap();
            }
        }


        private static void CreateMinimap()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            minimap = new GameObject("Minimap");

            RawImage rawImg = minimap.AddComponent<RawImage>();
            minimap.transform.SetParent(parentUI.transform, false);

            ManualCameraRenderer manualCam = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorScript").GetComponent<ManualCameraRenderer>();

            rawImg.texture = manualCam.mapCamera.targetTexture;

            minimap.layer = 5; // UI Layer
            minimap.transform.SetSiblingIndex(3);

            RectTransform rectMini = minimap.GetComponent<RectTransform>();
            rectMini.sizeDelta = new Vector2(200, 200);
            rectMini.anchoredPosition = Vector2.zero;

            rectMini.anchorMin = new Vector2(1, 1);
            rectMini.anchorMax = new Vector2(1, 1);
            rectMini.pivot = new Vector2(1, 1);

            rectMini.anchoredPosition = new Vector2(-10, -10);

            CreateBorder();
        }


        private static void CreateBorder()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            GameObject border = new GameObject("MinimapBorder");
            border.transform.SetParent(parentUI.transform, false);
            Image rawImg = border.AddComponent<Image>();

            border.layer = 5;
            border.transform.SetSiblingIndex(2);

            RectTransform rectBorder = border.GetComponent<RectTransform>();
            RectTransform rectMini = minimap.GetComponent<RectTransform>();

            rectBorder.sizeDelta = new Vector3(rectMini.sizeDelta.x + 8, rectMini.sizeDelta.y + 8);

            rectBorder.anchorMin = new Vector2(rectMini.anchorMin.x, rectMini.anchorMin.y);
            rectBorder.anchorMax = new Vector2(rectMini.anchorMax.x, rectMini.anchorMax.y);
            rectBorder.pivot = new Vector2(rectMini.pivot.x, rectMini.pivot.y);

            rectBorder.anchoredPosition = new Vector2(-10, -10); // new Vector2(rectMini.anchoredPosition.x, rectMini.anchoredPosition.y);

            minimap.transform.SetParent(border.transform, false);
            rectBorder.anchoredPosition = new Vector2(0, 0);
        }

    }
}
