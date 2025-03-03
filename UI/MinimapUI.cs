using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using LethalMystery.Utils;
using TMPro;
using LethalMystery.MainGame;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using LethalMystery.Players;
using LethalMystery.Maps.Sabotages;


namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class MinimapUI
    {

        public static GameObject minimap;
        public static GameObject minimapCam;
        public static GameObject border;
        public static GameObject mapIcon;
        public static Image lightIcon;
        public static TextMeshProUGUI playerNameTXT;
        public static List<GameObject> allMinimapObjects = new List<GameObject>();




        public static void DestroyUI()
        {
            foreach (GameObject obj in allMinimapObjects)
            {
                if (obj != null)
                {
                    Plugin.Destroy(obj);
                }
            }
            allMinimapObjects.Clear();
            Minimap.ResetVars();
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
            rectForm.anchoredPosition = new Vector2(0, 0);

            allMinimapObjects.Add(mapIcon);

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
            border.transform.SetSiblingIndex(14);

            RectTransform rectBorder = border.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(450, 450);
            rectBorder.anchoredPosition = Vector2.zero;

            allMinimapObjects.Add(border);
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

            RectTransform rectMini = minimap.GetComponent<RectTransform>();
            rectMini.sizeDelta = new Vector2(448, 448);
            rectMini.anchoredPosition = Vector2.zero;
            Minimap.MinimapWaypoint waypoint = minimap.AddComponent<Minimap.MinimapWaypoint>();
            waypoint.minimapCamera = minimapCam.GetComponent<Camera>();
            waypoint.minimapRectTransform = minimap.GetComponent<RectTransform>();
            Minimap.waypointPrefab = Plugin.localPlayer.transform.Find("Misc/MapDot").gameObject; // done twice

            allMinimapObjects.Add(minimap);
            allMinimapObjects.Add(minimapCam);

            CreateName();
            LeftButtonUI();
            RightButtonUI();
            CreateLightSabotage();
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

            playerNameTXT = playerName.AddComponent<TextMeshProUGUI>();
            playerNameTXT.font = StartOfRound.Instance.mapScreenPlayerName.font;
            playerNameTXT.fontSize = StartOfRound.Instance.mapScreenPlayerName.fontSize;
            playerNameTXT.text = Plugin.localPlayer.playerUsername;

            RectTransform textRect = playerName.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(280, 80);
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = Vector2.zero;
        }




        private static void LeftButtonUI()
        {
            GameObject LswitchBtn = new GameObject("LswitchBtn");
            LswitchBtn.transform.SetParent(minimap.transform, false);

            Button Lbtn = LswitchBtn.AddComponent<Button>();
            Image Limg = LswitchBtn.AddComponent<Image>();

            Sprite arrowImg = LMAssets.PlainArrow;
            Sprite arrowImgHover = LMAssets.PlainArrowHover;
            Limg.sprite = arrowImg;

            EventTrigger trigger = LswitchBtn.AddComponent<EventTrigger>();

            EventTrigger.Entry LpointerEnter = new EventTrigger.Entry();
            LpointerEnter.eventID = EventTriggerType.PointerEnter;
            LpointerEnter.callback.AddListener((data) => { Limg.sprite = arrowImgHover; });

            EventTrigger.Entry LpointerExit = new EventTrigger.Entry();
            LpointerExit.eventID = EventTriggerType.PointerExit;
            LpointerExit.callback.AddListener((data) => { Limg.sprite = arrowImg; });

            trigger.triggers.Add(LpointerEnter);
            trigger.triggers.Add(LpointerExit);

            Lbtn.onClick.AddListener(() => SwitchPlayerButton(forward: false));

            RectTransform LswitchRect = LswitchBtn.GetComponent<RectTransform>();
            LswitchRect.sizeDelta = new Vector2(32, 32);
            LswitchRect.anchorMin = new Vector2(0, 0);
            LswitchRect.anchorMax = new Vector2(0, 0);
            LswitchRect.pivot = new Vector2(1, 0);
            LswitchRect.anchoredPosition = new Vector2(8f, 2f);
            LswitchRect.rotation = Quaternion.Euler(0, 0, -90f);

        }


        private static void RightButtonUI()
        {
            GameObject RswitchBtn = new GameObject("RswitchBtn");
            RswitchBtn.transform.SetParent(minimap.transform, false);

            Button Rbtn = RswitchBtn.AddComponent<Button>();
            Image Rimg = RswitchBtn.AddComponent<Image>();

            Sprite arrowImg = LMAssets.PlainArrow;
            Sprite arrowImgHover = LMAssets.PlainArrowHover;
            Rimg.sprite = arrowImg;

            EventTrigger trigger = RswitchBtn.AddComponent<EventTrigger>();

            EventTrigger.Entry RpointerEnter = new EventTrigger.Entry();
            RpointerEnter.eventID = EventTriggerType.PointerEnter;
            RpointerEnter.callback.AddListener((data) => { Rimg.sprite = arrowImgHover; });

            EventTrigger.Entry RpointerExit = new EventTrigger.Entry();
            RpointerExit.eventID = EventTriggerType.PointerExit;
            RpointerExit.callback.AddListener((data) => { Rimg.sprite = arrowImg; });

            trigger.triggers.Add(RpointerEnter);
            trigger.triggers.Add(RpointerExit);

            Rbtn.onClick.AddListener(() => SwitchPlayerButton(forward: true));

            RectTransform RswitchRect = RswitchBtn.GetComponent<RectTransform>();
            RswitchRect.sizeDelta = new Vector2(32, 32);
            RswitchRect.anchorMin = new Vector2(0, 0);
            RswitchRect.anchorMax = new Vector2(0, 0);
            RswitchRect.pivot = new Vector2(0, 1);
            RswitchRect.anchoredPosition = new Vector2(80f, 0f);
            RswitchRect.rotation = Quaternion.Euler(0, 0, 90f);
        }

        private static void SwitchPlayerButton(bool forward)
        {
            if (Meeting.inMeeting.Value == "false") return;
            if (Minimap.allPlayerPoints.Count <= 0) return;

            List<string> playerList = new List<string>();
            foreach (KeyValuePair<string, string> plr in Minimap.allPlayerPoints)
            {
                playerList.Add(plr.Key);
            }

            int rawIndex = playerList.IndexOf(Minimap.currentPointUserID);
            int newIndex = (forward) ? rawIndex + 1 : rawIndex - 1;
            if (newIndex > playerList.Count - 1)
            {
                newIndex = 0;
            }
            else if (newIndex < 0)
            {
                newIndex = playerList.Count - 1;
            }

            Minimap.currentPointUserID = playerList[newIndex];

            Int32.TryParse(Minimap.currentPointUserID, out int plrid);
            playerNameTXT.text = StartOfRound.Instance.allPlayerScripts[plrid].playerUsername;
        }




        private static void CreateLightSabotage()
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Type == Roles.RoleType.employee) return;

            GameObject LightSwitch = new GameObject("LightSwitchBtn");
            LightSwitch.transform.SetParent(minimap.transform, false);

            Button btn = LightSwitch.AddComponent<Button>();
            lightIcon = LightSwitch.AddComponent<Image>();

            Sprite baseImg = LMAssets.LightSwitch;
            Sprite imgHover = LMAssets.LightSwitchHover;
            Sprite imgSelect = LMAssets.LightSwitchSelect;
            lightIcon.sprite = baseImg;

            EventTrigger trigger = LightSwitch.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) =>
            {
                lightIcon.sprite = (!Sabotage.generatorFixed || Sabotage.fogTimerStarted) ? imgSelect : imgHover;
            });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) =>
            {
                lightIcon.sprite = (!Sabotage.generatorFixed || Sabotage.fogTimerStarted) ? imgSelect : baseImg;
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);

            btn.onClick.AddListener(() => Generator.BreakGenerator());

            RectTransform rect = LightSwitch.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32, 32);
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-3, -8);

            allMinimapObjects.Add(LightSwitch);
        }

    }
}
