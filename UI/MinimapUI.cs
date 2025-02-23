using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using LethalMystery.Utils;
using TMPro;
using LethalMystery.MainGame;
using UnityEngine.EventSystems;


namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class MinimapUI
    {

        public static GameObject minimap;
        public static GameObject minimapCam;
        public static GameObject border;
        public static GameObject mapIcon;
        public static GameObject markerDot;




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

        private static void CreateBorder()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            border = new GameObject("Minimap");
            border.transform.SetParent(parentUI.transform, false);
            Image rawImg = border.AddComponent<Image>();
            rawImg.color = new Color(0.996f, 0.095f, 0, 1f);

            border.layer = 5;
            border.transform.SetSiblingIndex(12);

            RectTransform rectBorder = border.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(450, 450);

            rectBorder.anchoredPosition = Vector2.zero;
        }

        public static void CreateMinimap()
        {
            CreateBorder();

            minimap = new GameObject("MinimapScreen");
            minimap.transform.SetParent(border.transform, false);

            minimapCam = Plugin.Instantiate(GameObject.Find("Systems/GameSystems/ItemSystems/MapCamera"));
            minimapCam.name = "MinimapCam";

            GameObject mapScript = new GameObject("MapScript");
            mapScript.transform.SetParent(minimap.transform);
            ManualCameraRenderer mcr = mapScript.AddComponent<ManualCameraRenderer>();
            SetRenderVars(mcr);


            RawImage rawImg = minimap.AddComponent<RawImage>();
            rawImg.texture = mcr.mapCamera.targetTexture;

            minimap.layer = 5; // UI Layer
            markerDot = Plugin.localPlayer.transform.Find("Misc/MapDot").gameObject;

            RectTransform rectMini = minimap.GetComponent<RectTransform>();
            rectMini.sizeDelta = new Vector2(448, 448);
            rectMini.anchoredPosition = Vector2.zero;
            Minimap.MinimapWaypoint waypoint = minimap.AddComponent<Minimap.MinimapWaypoint>();
            waypoint.minimapCamera = minimapCam.GetComponent<Camera>();
            waypoint.playerTransform = Plugin.localPlayer.transform;
            //waypoint.waypointPrefab = markerDot;
            Minimap.waypointPrefab = markerDot;
            waypoint.minimapRectTransform = minimap.GetComponent<RectTransform>();

            CreateName();
            SwitchPlayerButton();
        }


        private static void SetRenderVars(ManualCameraRenderer camRender)
        {
            ManualCameraRenderer ogManualCam = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorScript").GetComponent<ManualCameraRenderer>();

            Camera mapCam = minimapCam.GetComponent<Camera>();
            RenderTexture newRenderTexture = new RenderTexture(512, 512, 16);
            mapCam.targetTexture = newRenderTexture;

            camRender.cam = mapCam;
            camRender.cameraViews = ogManualCam.cameraViews;
            camRender.cameraViewIndex = ogManualCam.cameraViewIndex;
            camRender.currentCameraDisabled = false;
            camRender.mesh = ogManualCam.mesh;
            camRender.offScreenMat = ogManualCam.offScreenMat;
            camRender.onScreenMat = ogManualCam.onScreenMat;
            camRender.materialIndex = ogManualCam.materialIndex;
            camRender.overrideCameraForOtherUse = false;
            camRender.renderAtLowerFramerate = false;
            camRender.targetedPlayer = Plugin.localPlayer;
            camRender.radarTargets = ogManualCam.radarTargets;
            camRender.targetTransformIndex = ogManualCam.targetTransformIndex;
            camRender.mapCamera = mapCam;
            camRender.mapCameraLight = ogManualCam.mapCameraLight;
            camRender.mapCameraAnimator = ogManualCam.mapCameraAnimator;
            camRender.mapCameraStationaryUI = ogManualCam.mapCameraStationaryUI;
            camRender.shipArrowPointer = ogManualCam.shipArrowPointer;
            camRender.shipArrowUI = ogManualCam.shipArrowUI;
        }


        private static void CreateName()
        {
            GameObject playerName = new GameObject("playerName");
            playerName.transform.SetParent(minimap.transform, false);

            TextMeshProUGUI text = playerName.AddComponent<TextMeshProUGUI>();
            text.font = StartOfRound.Instance.mapScreenPlayerName.font;
            text.fontSize = StartOfRound.Instance.mapScreenPlayerName.fontSize;
            text.text = Plugin.localPlayer.playerUsername;

            RectTransform textRect = playerName.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(280, 80);
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = Vector2.zero;
        }


        private static void SwitchName()
        {

        }

        private static void SwitchPlayerButton()
        {
            GameObject switchBtn = new GameObject("switchBtn");
            switchBtn.transform.SetParent(minimap.transform, false);

            Button btn = switchBtn.AddComponent<Button>();
            Image img = switchBtn.AddComponent<Image>();

            Color normalColor = Color.white;
            Color hoverColor = Color.gray;
            img.color = normalColor;


            EventTrigger trigger = switchBtn.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { img.color = hoverColor; });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { img.color = normalColor; });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);


            btn.onClick.AddListener(() => Plugin.mls.LogInfo($"Viewing: {Plugin.localPlayer.playerUsername}"));

            RectTransform switchRect = switchBtn.GetComponent<RectTransform>();
            switchRect.sizeDelta = new Vector2(32, 62);
            switchRect.anchorMin = new Vector2(0, 0);
            switchRect.anchorMax = new Vector2(0, 0);
            switchRect.pivot = new Vector2(0, 0);
            switchRect.anchoredPosition = Vector2.zero;
        }



    }
}
