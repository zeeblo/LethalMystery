using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using LethalMystery.MainGame;
using LethalMystery.Players;
using LethalMystery.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class VotingUI
    {
        public static bool isCalled = false;
        public static bool inVoteTime = false;
        public static GameObject votingGUI;
        public static GameObject voteIcon;
        public static GameObject skipSection;
        public static GameObject SlotHolder;
        public static List<GameObject> slotObjects = new List<GameObject>();
        private static Sprite xButtonSprite;


        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(QuickMenuManager __instance)
        {
            if (isCalled)
            {
                CreateVoteIcon();
                GameObject xButtonObj = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList/Image/PlayerListSlot/KickButton"); // Note: Check if MoreCompany is found
                xButtonSprite = xButtonObj.GetComponent<UnityEngine.UI.Image>().sprite;

                votingGUI = CreateVoteList();
                isCalled = false;

            }

            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) && votingGUI != null)
            {
                bool condition = LMConfig.defaultDiscussTime > 0 && StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0;
                float vote = StringAddons.ConvertToFloat(Meeting.voteTime.Value);
                float discuss = StringAddons.ConvertToFloat(Meeting.discussTime.Value);
                string value = (condition) ? $"DISCUSS: {(int)discuss}" : $"VOTE: {(int)vote}";
                UpdateTextHeader(value);

            }


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






        public class PlayerSlot : MonoBehaviour
        {
            public ulong playerID;
            public GameObject playerSlot;
            public GameObject votes;
        }


        private static GameObject CreateVoteList()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            GameObject voteList = new GameObject("VoteList");
            voteList.transform.SetParent(parentUI.transform, false);
            Image rawImg = voteList.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);

            voteList.layer = 5;
            voteList.transform.SetSiblingIndex(12); // before quickmenu

            RectTransform rect = voteList.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 300);
            rect.anchoredPosition = new Vector2(-140, 0);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);

            Outline outline = voteList.AddComponent<Outline>();
            outline.effectColor = new Color(0.996f, 0.095f, 0, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            voteList.AddComponent<Mask>();

            slotObjects.Clear();

            Header(voteList);
            CreatePlayerList(voteList);
            CreateSkipSection(voteList);
            Controls.UnlockCursor(true);

            return voteList;
        }

        private static void Header(GameObject voteList)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(voteList.transform, false);
            TextMeshProUGUI text = header.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 11f;
            //text.alignment = TextAlignmentOptions.Left;
            //text.margin = new Vector3(8, 0, 0);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.text = "[loading]: ";

            header.layer = 5;

            RectTransform bgRect = header.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(180, 20);
            bgRect.anchoredPosition = new Vector2(10, -15);
            bgRect.anchorMin = new Vector2(0, 1f);
            bgRect.anchorMax = new Vector2(0, 1f);
            bgRect.pivot = new Vector2(0, 1);
        }

        private static void CreatePlayerList(GameObject voteList)
        {
            GameObject playerList = new GameObject("PlayerList");
            playerList.transform.SetParent(voteList.transform, false);
            Image rawImg = playerList.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);
            playerList.layer = 5;

            RectTransform rect = playerList.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 230);

            ScrollRect scrlRect = playerList.AddComponent<ScrollRect>();
            scrlRect.horizontal = false;
            scrlRect.content = CreatePlayerSlotHolder(playerList);

            playerList.AddComponent<Mask>();
        }

        private static RectTransform CreatePlayerSlotHolder(GameObject playerList)
        {
            SlotHolder = new GameObject("PlayerSlotHolder");
            SlotHolder.transform.SetParent(playerList.transform, false);
            Image rawImg = SlotHolder.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);
            //rawImg.color = new Color(1, 1, 1, 1);
            SlotHolder.layer = 5;

            RectTransform rect = SlotHolder.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 30);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);


            GridLayoutGroup layout = SlotHolder.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(245, 50);
            //layout.spacing = new Vector2(0, 1);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 1;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.isPlayerDead) continue;
                if (!Voting.playersWhoGotVoted.Value.ContainsKey($"{player.playerClientId}")) continue;
                CreatePlayerSlot(slotRect: rect, playerID: player.playerClientId);

            }

            return rect;
        }

        private static void CreatePlayerSlot(RectTransform slotRect, ulong playerID)
        {
            GameObject playerSlot = new GameObject("playerSlot");
            playerSlot.transform.SetParent(SlotHolder.transform, false);
            Image bgImage = playerSlot.AddComponent<Image>();
            bgImage.color = new Color(1, 1, 1, 0); // So I can visually see what the rect looks like when i need to

            playerSlot.layer = 5;

            RectTransform bgRect = playerSlot.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 30);
            bgRect.anchoredPosition = new Vector2(10, 0);
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);

            // Change Y size of parent
            float slot_x = slotRect.sizeDelta.x;
            float slot_y = slotRect.sizeDelta.y + 55;
            slotRect.sizeDelta = new Vector2(slot_x, slot_y);

            PlayerSlot pslot = playerSlot.AddComponent<PlayerSlot>();
            pslot.playerID = playerID;
            pslot.playerSlot = playerSlot;

            NameTag(NameTagBg(playerSlot), playerID);
            CreateVoteButton(playerSlot, playerID);
            pslot.votes = CreateVoteText(playerSlot);

            slotObjects.Add(playerSlot);
        }

        private static GameObject NameTagBg(GameObject playerSlot)
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            GameObject nametagBG = new GameObject("nametag");
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
            return nametagBG;
        }


        private static void NameTag(GameObject nametagBG, ulong playerID)
        {
            GameObject username = new GameObject("username");
            username.transform.SetParent(nametagBG.transform, false);
            TextMeshProUGUI text = username.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Left;
            text.margin = new Vector3(8, 0, 0);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.text = $"#{playerID} {StartOfRound.Instance.allPlayerScripts[(int)playerID].playerUsername}";

            username.layer = 5;

            RectTransform bgRect = username.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(180, 20);
            bgRect.anchoredPosition = Vector2.zero;

        }



        private static GameObject CreateVoteText(GameObject playerSlot)
        {
            GameObject votesTxt = new GameObject("Votes");
            votesTxt.transform.SetParent(playerSlot.transform, false);
            TextMeshProUGUI text = votesTxt.AddComponent<TextMeshProUGUI>();
            text.color = new Color(1, 0.5897f, 0, 1);
            text.fontSize = 12f;
            text.fontStyle = FontStyles.Bold;
            text.text = "0";

            votesTxt.layer = 5;

            RectTransform bgRect = votesTxt.GetComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(125, -15);
            bgRect.anchorMin = new Vector2(1, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.pivot = new Vector2(1, 0.5f);
            return votesTxt;
        }



        private static void CreateVoteButton(GameObject playerSlot, ulong playerID)
        {
            //if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false) return;

            GameObject voteBtn = new GameObject("VoteBtn");
            voteBtn.transform.SetParent(playerSlot.transform, false);

            Button btn = voteBtn.AddComponent<Button>();
            Image voteBtnIcon = voteBtn.AddComponent<Image>();

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
                voteBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);


            int index = (int)playerID;
            btn.onClick.AddListener(() => Voting.VoteButtonClick(index, voteBtnIcon));

            RectTransform rect = voteBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);

        }



        private static void CreateSkipSection(GameObject voteList)
        {
            skipSection = new GameObject("SkipSection");
            Image img = skipSection.AddComponent<Image>();
            skipSection.transform.SetParent(voteList.transform, false);

            img.color = new Color(1, 1, 1, 0);

            RectTransform rect = skipSection.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(voteList.GetComponent<RectTransform>().sizeDelta.x, 30);
            rect.anchoredPosition = new Vector2(0, 30f);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 1);

            CreateSkipText();
        }

        private static void CreateSkipText()
        {
            GameObject skiptxt = new GameObject("SkipText");
            TextMeshProUGUI txt = skiptxt.AddComponent<TextMeshProUGUI>();
            skiptxt.transform.SetParent(skipSection.transform, false);

            txt.color = new Color(1, 0.5897f, 0, 1);
            txt.fontSize = 12f;
            txt.fontStyle = FontStyles.Bold;
            txt.text = "SKIP: ";

            RectTransform rect = skiptxt.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(35, 0);
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.9f);

            CreateSkipButton();
        }


        private static void CreateSkipButton()
        {
            GameObject skipBtn = new GameObject("SkipBtn");
            skipBtn.transform.SetParent(skipSection.transform, false);

            Button btn = skipBtn.AddComponent<Button>();
            Image skipBtnIcon = skipBtn.AddComponent<Image>();

            Sprite baseImg = LMAssets.CheckboxEmptyIcon;
            Sprite imgSelect = LMAssets.CheckboxEnabledIcon;
            skipBtnIcon.sprite = baseImg;
            skipBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);

            EventTrigger trigger = skipBtn.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) =>
            {
                skipBtnIcon.color = Color.white;
            });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) =>
            {
                skipBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);

            btn.onClick.AddListener(() => Voting.SkipButtonClick(skipBtnIcon));

            RectTransform rect = skipBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);

        }



        private static void UpdateTextHeader(string text)
        {
            GameObject header = votingGUI.transform.Find("Header").gameObject;
            if (header)
            {
                header.gameObject.GetComponent<TextMeshProUGUI>().text = text;
            }

            if (text.ToLower().Contains("vote") && inVoteTime == false)
            {
                UpdateVoteButtonSprite();
                inVoteTime = true;
            }
        }

        /// <summary>
        /// During discuss time the buttons must appear as X.
        /// After they must appear as empty boxes
        /// </summary>
        private static void UpdateVoteButtonSprite()
        {

            foreach (GameObject playerslot in slotObjects)
            {
                Transform rawPlrObj = playerslot.transform.Find("VoteBtn");
                if (rawPlrObj == null) continue;

                GameObject plrObj = rawPlrObj.gameObject;
                if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0)
                {
                    plrObj.GetComponent<UnityEngine.UI.Image>().sprite = xButtonSprite;
                }
                else
                {
                    plrObj.GetComponent<UnityEngine.UI.Image>().sprite = LMAssets.CheckboxEmptyIcon;
                }

            }

        }



        public static void UpdateSkipText()
        {
            GameObject skipObj = skipSection.transform.Find("SkipText").gameObject;
            TextMeshProUGUI skipText = skipObj.GetComponent<TextMeshProUGUI>();
            skipText.text = "SKIP: " + Voting.skipVotes.Value;
        }


        public static void UpdateVoteText(int userID)
        {
            foreach (KeyValuePair<string, string> id in Voting.playersWhoGotVoted.Value)
            {
                foreach (GameObject slot in slotObjects)
                {
                    PlayerSlot comp = slot.GetComponent<PlayerSlot>();
                    TextMeshProUGUI votes = comp.votes.GetComponent<TextMeshProUGUI>();
                    int pid = (int)comp.playerID;

                    if (pid == StringAddons.ConvertToInt(id.Value))
                    {
                        votes.text = "VOTES: " + id.Value;
                        break;
                    }
                }
            }

        }


        /// <summary>
        /// Refresh playerlist when player leaves or dies
        /// </summary>
        public static void UpdatePlayerList(ulong playerID)
        {
            foreach (GameObject player in slotObjects)
            {
                ulong pid = player.GetComponent<PlayerSlot>().playerID;

                if (pid == playerID)
                {
                    Plugin.Destroy(player);
                    break;
                }
            }
        }



    }
}
