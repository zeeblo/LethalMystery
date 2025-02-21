using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using LethalMystery.Utils;
using TMPro;

namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class Minimap
    {

        public static GameObject minimap;
        public static GameObject border;
        public static GameObject mapIcon;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void SetupPatch()
        {
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                //CreateMapIcon();
               // CreateBorder();
               // CreateMinimap();
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



        public static void CreateMapIcon()
        {

            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            mapIcon = new GameObject("minimapIcon");
            mapIcon.transform.SetParent(parentUI.transform, false);

            mapIcon.layer = 5;
            mapIcon.transform.SetSiblingIndex(1);

            Image img = mapIcon.AddComponent<Image>();
            img.sprite = LMAssets.MapIcon;

            RectTransform rectForm = mapIcon.GetComponent<RectTransform>();
            rectForm.sizeDelta = new Vector2(64, 64);

            rectForm.anchorMin = new Vector2(1, 1);
            rectForm.anchorMax = new Vector2(1, 1);
            rectForm.pivot = new Vector2(1, 1);
            rectForm.anchoredPosition = new Vector2(-10, 14);

            CreateMapIconKeybind();
        }

        private static void CreateMapIconKeybind()
        {
            GameObject bgObj = new GameObject("background");
            bgObj.transform.SetParent(mapIcon.transform, false);
            bgObj.transform.SetSiblingIndex(0);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 1f); // new Color(13f, 0, 0, 0.2f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(32, 16);
            bgRect.anchorMin = new Vector2(1, 1);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.pivot = new Vector2(1.5f, 4.5f);
            bgRect.anchoredPosition = Vector2.zero;
            

            GameObject keybind = new GameObject("keybind");
            keybind.transform.SetParent(mapIcon.transform, false);
            keybind.transform.SetSiblingIndex(1);
            TextMeshProUGUI bindtxt = keybind.AddComponent<TextMeshProUGUI>();
            bindtxt.text = "[ " + $"{LMConfig.showMapBind.Value.ToUpper()}" + " ]";
            bindtxt.fontSize = 9;
            bindtxt.fontWeight = FontWeight.Heavy;
            bindtxt.alignment = TextAlignmentOptions.Center;
            bindtxt.color = new Color(0.996f, 0.095f, 0, 1f); // new Color(0.996f, 0.274f, 0, 0.2f);

            RectTransform bindRect = keybind.GetComponent<RectTransform>();
            bindRect.sizeDelta = new Vector2(280, 80);
            bindRect.pivot = new Vector2(0.5f, 0.9f);
            bindRect.anchoredPosition = Vector2.zero;
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
