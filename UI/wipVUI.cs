using System.Collections.Generic;
using Discord;
using LethalMystery.MainGame;
using LethalMystery.Maps.Sabotages;
using LethalMystery.Players;
using LethalMystery.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LethalMystery.UI
{
    internal class wipVUI
    {

        public static GameObject voteIcon;
        public static GameObject playerSlot;
        public static GameObject voteList;
        public static GameObject playerList;
        public static GameObject nametagBG;
        public static Image voteBtnIcon;
        public static List<GameObject> allVoteUIObjects = new List<GameObject>();


        public static void DestroyUI()
        {
            foreach (GameObject obj in allVoteUIObjects)
            {
                if (obj != null)
                {
                    Plugin.Destroy(obj);
                }
            }
            allVoteUIObjects.Clear();
            Voting.ResetVars();
        }


        #region VoteIcon

        public static void CreateVoteIcon()
        {

            GameObject parentUI = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            voteIcon = new GameObject("voteIcon");
            voteIcon.transform.SetParent(parentUI.transform, false);

            voteIcon.layer = 5; // (UI Layer)
            voteIcon.transform.SetSiblingIndex(1);

            Image img = voteIcon.AddComponent<Image>();
            img.sprite = LMAssets.VoteIcon;

            RectTransform rectForm = voteIcon.GetComponent<RectTransform>();
            rectForm.sizeDelta = new Vector2(64, 64);

            rectForm.anchorMin = new Vector2(1, 1);
            rectForm.anchorMax = new Vector2(1, 1);
            rectForm.pivot = new Vector2(1, 1);
            rectForm.anchoredPosition = new Vector2(-80, 0);

            allVoteUIObjects.Add(voteIcon);

            ShowVoteIconKeybind();
        }



        private static void ShowVoteIconKeybind()
        {
            GameObject bgObj = new GameObject("background");
            bgObj.transform.SetParent(voteIcon.transform, false);
            bgObj.transform.SetSiblingIndex(0);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 1f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(32, 16);
            bgRect.anchorMin = new Vector2(1, 1);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.pivot = new Vector2(1.5f, 4.5f);
            bgRect.anchoredPosition = Vector2.zero;


            GameObject keybind = new GameObject("keybind");
            keybind.transform.SetParent(voteIcon.transform, false);
            keybind.transform.SetSiblingIndex(1);
            TextMeshProUGUI bindtxt = keybind.AddComponent<TextMeshProUGUI>();
            bindtxt.text = "[ " + $"{LMConfig.showVoteBind.Value.ToUpper()}" + " ]";
            bindtxt.fontSize = 9;
            bindtxt.fontWeight = FontWeight.Heavy;
            bindtxt.alignment = TextAlignmentOptions.Center;
            bindtxt.color = new Color(0.996f, 0.095f, 0, 1f);

            RectTransform bindRect = keybind.GetComponent<RectTransform>();
            bindRect.sizeDelta = new Vector2(280, 80);
            bindRect.pivot = new Vector2(0.5f, 0.9f);
            bindRect.anchoredPosition = Vector2.zero;
        }

        #endregion VoteIcon



        public static void SetupPlayerSlot()
        {
            CreateVoteList();
            CreatePlayerList();
            NameTagBg();
            NameTag();
            CreateVoteText();
            CreateVoteButton();
        }



        public static void CreateVoteList()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            voteList = new GameObject("VoteList");
            voteList.transform.SetParent(parentUI.transform, false);
            Image rawImg = voteList.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);

            voteList.layer = 5;
            voteList.transform.SetSiblingIndex(13); // before quickmenu

            RectTransform rect = voteList.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 300);
            rect.anchoredPosition = new Vector2(-140, 0);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);

            Outline outline = voteList.AddComponent<Outline>();
            outline.effectColor = new Color(0.996f, 0.095f, 0, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            Header();
            allVoteUIObjects.Add(voteList);
        }

        public static void CreatePlayerList()
        {
            playerList = new GameObject("PlayerList");
            playerList.transform.SetParent(voteList.transform, false);
            Image rawImg = playerList.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);
            playerList.layer = 5;

            RectTransform rect = playerList.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 230);

            ScrollRect scrlRect = playerList.AddComponent<ScrollRect>();
            scrlRect.horizontal = false;
            scrlRect.content = CreatePlayerSlot();

            playerList.AddComponent<Mask>();

            allVoteUIObjects.Add(playerList);
        }

        public static RectTransform CreatePlayerSlot()
        {
            playerSlot = new GameObject("playerSlot");
            playerSlot.transform.SetParent(playerList.transform, false);
            Image bgImage = playerSlot.AddComponent<Image>();
            bgImage.color = new Color(1, 1, 1, 0); // So I can visually see what the rect looks like when i need to

            playerSlot.layer = 5;

            RectTransform bgRect = playerSlot.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 30);
            bgRect.anchoredPosition = new Vector2(10, 0);
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            return bgRect;
        }

        public static void NameTagBg()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            nametagBG = new GameObject("nametag");
            nametagBG.transform.SetParent(playerSlot.transform, false);
            Image bgImage = nametagBG.AddComponent<Image>();
            bgImage.color = new Color(0.6509f, 0.2091f, 0.0031f, 1);

            nametagBG.layer = 5;

            RectTransform bgRect = nametagBG.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(166.38f, 23.19f);
            bgRect.anchoredPosition = Vector2.zero;

            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.pivot = new Vector2(0, 0.5f);
        }


        public static void NameTag()
        {
            GameObject username = new GameObject("username");
            username.transform.SetParent(nametagBG.transform, false);
            TextMeshProUGUI text = username.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Left;
            text.margin = new Vector3(8, 0, 0);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.text = "boop";

            username.layer = 5;

            RectTransform bgRect = username.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(180, 20);
            bgRect.anchoredPosition = Vector2.zero;
            
        }

        public static void Header()
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(voteList.transform, false);
            TextMeshProUGUI text = header.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 11f;
            //text.alignment = TextAlignmentOptions.Left;
            //text.margin = new Vector3(8, 0, 0);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.text = "DISCUSS: ";

            header.layer = 5;

            RectTransform bgRect = header.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(180, 20);
            bgRect.anchoredPosition = new Vector2(10, -15);
            bgRect.anchorMin = new Vector2(0, 1f);
            bgRect.anchorMax = new Vector2(0, 1f);
            bgRect.pivot = new Vector2(0, 1);
        }

        public static void CreateVoteText()
        {
            GameObject votesTxt = new GameObject("Votes");
            votesTxt.transform.SetParent(playerSlot.transform, false);
            TextMeshProUGUI text = votesTxt.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 8f;
            text.text = "VOTES: ";

            votesTxt.layer = 5;

            RectTransform bgRect = votesTxt.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0, 0);
            bgRect.pivot = new Vector2(0, 1);
        }





        
        private static void CreateVoteButton()
        {
            //if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false) return;

            GameObject voteBtn = new GameObject("VoteBtn");
            voteBtn.transform.SetParent(playerSlot.transform, false);

            Button btn = voteBtn.AddComponent<Button>();
            voteBtnIcon = voteBtn.AddComponent<Image>();

            Sprite baseImg = LMAssets.CheckboxEmptyIcon;
            Sprite imgSelect = LMAssets.CheckboxEnabledIcon;
            voteBtnIcon.sprite = baseImg;
            voteBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);

            EventTrigger trigger = voteBtn.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) =>
            {
                voteBtnIcon.color = Color.white;
            });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) =>
            {
                voteBtnIcon.sprite = baseImg;
                voteBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);

            btn.onClick.AddListener(() => playerVoted());

            RectTransform rect = voteBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);

            allVoteUIObjects.Add(voteBtn);
        }
        

        private static void playerVoted()
        {
            voteBtnIcon.sprite = LMAssets.CheckboxEnabledIcon;
            Plugin.mls.LogInfo(">>>> Player voted!");
        }

    }
}
