using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class Minimap
    {

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                //GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopRightCorner");
                GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
                Canvas canvas = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
                GameObject rawImageObject = new GameObject("Minimap");

                RawImage rawImage = rawImageObject.AddComponent<RawImage>();
                rawImageObject.transform.SetParent(parentUI.transform, false);

                ManualCameraRenderer manualCam = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorScript").GetComponent<ManualCameraRenderer>();

                rawImage.texture = manualCam.mapCamera.targetTexture;

                rawImageObject.layer = 5; // UI Layer
                rawImageObject.transform.SetSiblingIndex(3);

                RectTransform rectTransform = rawImageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 200);
                rectTransform.anchoredPosition = Vector2.zero;

                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);

                rectTransform.anchoredPosition = new Vector2(-10, -10);
            }
        }

    }
}
