using System;
using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using LethalMystery.MainGame;
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
        public static GameObject playerSlot;
        public static GameObject voteList;
        //public static GameObject playerList;
        public static GameObject header;
        public static GameObject skipSection;
        public static GameObject SlotHolder;
        public static List<GameObject> allVoteUIObjects = new List<GameObject>();
        public static List<GameObject> slotObjects = new List<GameObject>();
        //public static Image voteBtnIcon;
        //public static Image skipBtnIcon;
        private static Sprite xButtonSprite;


        #region Patches


        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(QuickMenuManager __instance)
        {
            if (isCalled)
            {
                GameObject xButtonObj = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList/Image/PlayerListSlot/KickButton"); // Note: Check if MoreCompany is found
                xButtonSprite = xButtonObj.GetComponent<UnityEngine.UI.Image>().sprite;

                votingGUI = CreateVoteList();
                isCalled = false;

                UpdateVoteButtonSprite();
                EnableDisabledList();
                //StartOfRound.Instance.StartCoroutine(DoubleCheckList());
            }

            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value))
            {
                bool condition = LMConfig.defaultDiscussTime > 0 && StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0;
                float vote = StringAddons.ConvertToFloat(Meeting.voteTime.Value);
                float discuss = StringAddons.ConvertToFloat(Meeting.discussTime.Value);
                string value = (condition) ? $"DISCUSS: {(int)discuss}" : $"VOTE: {(int)vote}";
                UpdateTextHeader(value);

            }


        }





        #endregion Patches


        private static GameObject CreateVotingGUI(QuickMenuManager __instance)
        {
            GameObject canvas = GameObject.Find("Systems/UI/Canvas/");
            GameObject VotingMenu = new GameObject("VotingMenu");
            GameObject plist = Plugin.Instantiate(__instance.playerListPanel); //ClonedPlayerList(__instance.playerListPanel); // Plugin.Instantiate(__instance.playerListPanel);

            VotingMenu.layer = 5; // UI Layer
            VotingMenu.transform.SetParent(canvas.transform, false);
            VotingMenu.transform.SetSiblingIndex(12);

            plist.transform.SetParent(VotingMenu.transform, false);

            string rawHeaderText = (LMConfig.defaultDiscussTime == 0) ? "VOTE: " : "DISCUSS: ";
            float vote = StringAddons.ConvertToFloat(Meeting.voteTime.Value);
            float discuss = StringAddons.ConvertToFloat(Meeting.discussTime.Value);
            string HeaderText = (rawHeaderText.Equals("VOTE: ")) ? rawHeaderText + $"{(int)vote}" : rawHeaderText + $"{(int)discuss}";

            plist.transform.Find("Image/Header").gameObject.GetComponent<TextMeshProUGUI>().text = HeaderText;

            ShowVotesForPlayers(plist);
            VoteButton(plist);
            SkipButton(plist);
            CheckPlayerList();
            return VotingMenu;
        }



        private static void UpdateTextHeader(string text)
        {
            GameObject header = GameObject.Find("Systems/UI/Canvas/VotingMenu/PlayerList(Clone)/Image/Header");
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


        private static GameObject GetTextHeader()
        {
            return GameObject.Find("Systems/UI/Canvas/VotingMenu/PlayerList(Clone)/Image/Header");
        }



        private static GameObject GetPlayerListSlot()
        {
            return GameObject.Find("Systems/UI/Canvas/VotingMenu/PlayerList(Clone)");
        }




        public static List<int> GetPlayerIDs()
        {
            string parentPath = "Systems/UI/Canvas/QuickMenu";
            string path = $"{parentPath}/PlayerList(Clone)/Image/QuickmenuOverride(Clone)/Holder";
            List<int> playerIDS = new List<int>();

            GameObject playerVoteObj = GameObject.Find(path);
            foreach (GameObject players in GOTools.GetAllChildren(playerVoteObj))
            {
                TextMeshProUGUI pName = players.transform.Find("PlayerNameButton").transform.Find("PName").GetComponent<TextMeshProUGUI>();
                string actualID = pName.text.Split("#")[1];
                playerIDS.Add(StringAddons.ConvertToInt(actualID));
            }
            return playerIDS;
        }



        public static void SkipButton(GameObject playerListSlot)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string path = (moreCompany == false) ? $"Image/PlayerListSlot/VoteButton" : "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/VoteButton";

            GameObject plistClone = playerListSlot.transform.Find("Image").gameObject;
            GameObject playerVoteBtn = playerListSlot.transform.Find(path).gameObject;


            GameObject skipButton = Plugin.Instantiate(playerVoteBtn);
            RectTransform skipButtonRect = skipButton.GetComponent<RectTransform>();
            skipButton.gameObject.name = "SkipButton";
            skipButton.transform.SetParent(plistClone.transform, false);
            skipButtonRect.anchoredPosition = new Vector2(-100f, -155f);


            GameObject skipObj = Plugin.Instantiate(GetTextHeader());
            RectTransform skipTextRect = skipObj.GetComponent<RectTransform>();
            TextMeshProUGUI skipText = skipObj.GetComponent<TextMeshProUGUI>();
            skipObj.gameObject.name = "SkipText";
            skipText.fontSize = 17;
            skipText.text = "SKIP: " + Voting.skipVotes.Value;
            skipObj.transform.SetParent(plistClone.transform, false);
            skipTextRect.anchoredPosition = new Vector2(45f, -318f);

            UnityEngine.UI.Image skipCheckSprite = skipButton.GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Button skipBtn = skipButton.GetComponent<UnityEngine.UI.Button>();
            skipCheckSprite.sprite = LMAssets.CheckboxEmptyIcon;
            skipBtn.onClick.AddListener(() => Voting.SkipButtonClick(skipCheckSprite));

        }


        public static void VoteButton(GameObject playerListSlot)
        {
            GameObject raw_playerVoteBtn = playerListSlot.transform.Find("Image/QuickmenuOverride(Clone)/Holder").gameObject;
            Dictionary<int, GameObject> playerVoteBtn = new Dictionary<int, GameObject>();

            
            foreach (GameObject player in GOTools.GetAllChildren(raw_playerVoteBtn))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton/PName").GetComponent<TextMeshProUGUI>();
                string name = pName.text;
                int id = StringAddons.NameToID(name); // StringAddons.GetLastId(name); //
                Plugin.mls.LogInfo($">>>ClonedPlayerList: pName: {pName.text} | playerID: {id}");

                if (id == -1) continue;
                if (StartOfRound.Instance.allPlayerScripts[id].isPlayerDead) continue;
                if (!Voting.playersWhoGotVoted.Value.ContainsKey($"{id}")) continue;

                playerVoteBtn.Add(id, player);
            }
            

            foreach (KeyValuePair<int, GameObject> i in playerVoteBtn)
            {
                int index = i.Key; // because apparently using just "i" doesn't work for events, it needs to be stored in a variable

                GameObject VoteBtn = i.Value.transform.Find("KickButton").gameObject;
                VoteBtn.gameObject.name = "VoteButton";
                VoteBtn.SetActive(true);

                UnityEngine.UI.Image plrCheckSprite = VoteBtn.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button plrbutton = VoteBtn.GetComponent<UnityEngine.UI.Button>();
                plrCheckSprite.sprite = (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) ? xButtonSprite : LMAssets.CheckboxEmptyIcon;

                Plugin.mls.LogInfo($">>> Logging num: {index}");
                plrbutton.onClick.AddListener(() => Voting.VoteButtonClick(index, plrCheckSprite));

            }


        }

        /// <summary>
        /// During discuss time the buttons must appear as X.
        /// After they must appear as empty boxes
        /// </summary>
        private static void UpdateVoteButtonSprite()
        {
            GameObject playerVoteBtn = GetPlayerListSlot().transform.Find("Image/QuickmenuOverride(Clone)/Holder").gameObject;
            foreach (GameObject players in GOTools.GetAllChildren(playerVoteBtn))
            {
                // if (StartOfRound.Instance.allPlayerScripts[i].isPlayerDead) continue;
                Transform rawPlrObj = players.transform.Find("VoteButton");
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


        private static void ShowVotesForPlayers(GameObject playerListSlot)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (!Voting.playersWhoGotVoted.Value.ContainsKey($"{i}")) continue;

                string votes = "VOTES: " + Voting.playersWhoGotVoted.Value[$"{i}"];
                string playerBeingVoted = (i > 0) ? $"PlayerListSlot ({i})" : "PlayerListSlot";
                string path = (moreCompany == false) ? $"Image/{playerBeingVoted}/VoiceVolumeSlider" : "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/VoiceVolumeSlider";

                GameObject VolSlider = playerListSlot.transform.Find(path).gameObject;
                VolSlider.SetActive(true);
                VolSlider.transform.Find("Image").gameObject.SetActive(false);
                VolSlider.transform.Find("Slider").gameObject.SetActive(false);

                GameObject playerVoteObj = playerListSlot; // placeholder
                if (moreCompany == false)
                {
                    playerVoteObj = playerListSlot.transform.Find($"{path}/Text (1)").gameObject;
                }
                else
                {
                    playerVoteObj = playerListSlot.transform.Find("Image/QuickmenuOverride(Clone)/Holder").gameObject;
                    foreach (GameObject players in GOTools.GetAllChildren(playerVoteObj))
                    {
                        GameObject plrObj = players.transform.Find("Text (1)").gameObject;
                        GameObject plrVolume = players.transform.Find("PlayerVolumeSlider").gameObject;
                        plrVolume.SetActive(false);
                        plrObj.name = "Votes";
                        plrObj.SetActive(true);
                        plrObj.gameObject.GetComponent<TextMeshProUGUI>().text = votes;
                    }
                    break;
                }

                playerVoteObj.name = "Votes";
                playerVoteObj.SetActive(true);
                playerVoteObj.gameObject.GetComponent<TextMeshProUGUI>().text = votes;
            }
        }



        public static void UpdateSkipText()
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = (moreCompany == false) ? $"{parentPath}/PlayerList(Clone)/Image/SkipText" : $"{parentPath}/PlayerList(Clone)/Image/SkipText";

            GameObject skipObj = GameObject.Find(path);
            TextMeshProUGUI skipText = skipObj.GetComponent<TextMeshProUGUI>();
            skipText.text = "SKIP: " + Voting.skipVotes.Value;
        }


        public static void UpdateVoteText(int userID)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string playerBeingVoted = (userID > 0) ? $"PlayerListSlot ({userID})" : "PlayerListSlot";
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = (moreCompany == false) ? $"{parentPath}/PlayerList(Clone)/Image/{playerBeingVoted}/VoiceVolumeSlider/Votes" : $"{parentPath}/PlayerList(Clone)/Image/QuickmenuOverride(Clone)/Holder";

            if (moreCompany)
            {

                GameObject playerVoteObj = GameObject.Find(path);
                foreach (GameObject players in GOTools.GetAllChildren(playerVoteObj))
                {
                    TextMeshProUGUI pName = players.transform.Find("PlayerNameButton").transform.Find("PName").GetComponent<TextMeshProUGUI>();
                    foreach (PlayerControllerB sofplayer in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (pName.text == sofplayer.playerUsername && (int)sofplayer.playerClientId == userID)
                        {
                            Plugin.mls.LogInfo(">>> Found matching name");
                            GameObject plrObj = players.transform.Find("Votes").gameObject;
                            plrObj.gameObject.GetComponent<TextMeshProUGUI>().text = "VOTES: " + Voting.playersWhoGotVoted.Value[$"{userID}"];
                            break;
                        }
                    }

                }
            }
            else
            {
                GameObject playerVoteObj = GameObject.Find(path);
                TextMeshProUGUI playerVoteText = playerVoteObj.GetComponent<TextMeshProUGUI>();
                playerVoteText.text = "VOTES: " + Voting.playersWhoGotVoted.Value[$"{userID}"];
            }

        }


        /// <summary>
        /// Refresh playerlist when player leaves or dies
        /// </summary>
        public static void UpdatePlayerList(ulong playerID)
        {
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = $"{parentPath}/PlayerList(Clone)/Image/QuickmenuOverride(Clone)/Holder";

            GameObject playerVoteObj = GameObject.Find(path);
            foreach (GameObject player in GOTools.GetAllChildren(playerVoteObj))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton").transform.Find("PName").GetComponent<TextMeshProUGUI>();
                Plugin.mls.LogInfo($">>> pName: {pName.text} | playerID: {playerID}");

                foreach (PlayerControllerB sofplayer in StartOfRound.Instance.allPlayerScripts)
                {
                    if (pName.text == sofplayer.playerUsername && sofplayer.playerClientId == playerID)
                    {
                        Plugin.mls.LogInfo($"^^^ Found Match! (sofplayer: {sofplayer.playerUsername})");
                        Plugin.Destroy(player.gameObject);
                    }
                }
            }
        }


        /// <summary>
        /// Sometimes certain playerlistslots that shouldn't appear disabled
        /// are disabled, this method attempts to fix that
        /// </summary>
        private static void EnableDisabledList()
        {
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = $"{parentPath}/PlayerList(Clone)";

            GameObject PlayerList = GameObject.Find(path);
            if (PlayerList != null)
            {
                PlayerList.SetActive(true);
            }
        }

        /// <summary>
        /// Check if players are dead and remove them from the votelist
        /// </summary>
        private static void CheckPlayerList()
        {
            foreach (PlayerControllerB plr in StartOfRound.Instance.allPlayerScripts)
            {
                if (plr.isPlayerDead)
                {
                    UpdatePlayerList(plr.playerClientId);
                }
            }
        }



        /// <summary>
        /// If for some reason the UI isn't created properly, recreate it
        /// </summary>
        private static IEnumerator DoubleCheckList()
        {
            yield return new WaitForSeconds(0.8f);
            if (votingGUI != null)
            {
                CheckPlayerList();
            }
            else
            {
                isCalled = true;
            }
            
        }











        public class PlayerSlot : MonoBehaviour
        {
            public ulong playerID;
            public GameObject playerSlot;
            public GameObject votes;
        }


        public static GameObject CreateVoteList()
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

            voteList.AddComponent<Mask>();
            allVoteUIObjects.Add(voteList);

            Header();
            CreatePlayerList();
            CreateSkipSection();

            return voteList;
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

        public static void CreatePlayerList()
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
            allVoteUIObjects.Add(playerList);


        }

        public static RectTransform CreatePlayerSlotHolder(GameObject playerList)
        {
            SlotHolder = new GameObject("PlayerSlotHolder");
            SlotHolder.transform.SetParent(playerList.transform, false);
            Image rawImg = SlotHolder.AddComponent<Image>();
            //rawImg.color = new Color(0.4627f, 0, 0, 1);
            rawImg.color = new Color(1, 1, 1, 1);
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

        public static void CreatePlayerSlot(RectTransform slotRect, ulong playerID)
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
            CreateVoteButton(playerSlot);
            pslot.votes = CreateVoteText(playerSlot);

            slotObjects.Add(playerSlot);
        }

        public static GameObject NameTagBg(GameObject playerSlot)
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


        public static void NameTag(GameObject nametagBG, ulong playerID)
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



        public static GameObject CreateVoteText(GameObject playerSlot)
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



        private static void CreateVoteButton(GameObject playerSlot)
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
                voteBtnIcon.sprite = baseImg;
                voteBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);

            btn.onClick.AddListener(() => playerVoted(voteBtnIcon));

            RectTransform rect = voteBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);

            allVoteUIObjects.Add(voteBtn);
        }


        private static void playerVoted(Image voteBtnIcon)
        {
            voteBtnIcon.sprite = LMAssets.CheckboxEnabledIcon;
            Plugin.mls.LogInfo(">>>> Player voted!");
        }



        private static void CreateSkipSection()
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
                skipBtnIcon.sprite = baseImg;
                skipBtnIcon.color = new Color(0.651f, 0.2118f, 0.0039f, 1);
            });

            trigger.triggers.Add(pointerEnter);
            trigger.triggers.Add(pointerExit);

            btn.onClick.AddListener(() => playerSkipped());

            RectTransform rect = skipBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);

            allVoteUIObjects.Add(skipBtn);
        }


        private static void playerSkipped()
        {
            Plugin.mls.LogInfo(">>>> Player Skipped.");
        }

    }
}
