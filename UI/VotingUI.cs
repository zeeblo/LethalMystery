using Discord;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class VotingUI
    {
        public static bool isCalled = false;
        public static GameObject votingGUI;
        private static Sprite xButtonSprite;
        public static bool inVoteTime = false;


        #region Patches


        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(QuickMenuManager __instance)
        {
            if (isCalled)
            {
                GameObject xButtonObj = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList/Image/PlayerListSlot/KickButton");
                xButtonSprite = xButtonObj.GetComponent<UnityEngine.UI.Image>().sprite;

                votingGUI = CreateVotingGUI(__instance);
                isCalled = false;

                UpdateVoteButtonSprite();
            }

            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value))
            {
                bool condition = LMConfig.defaultDiscussTime > 0 && StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0;
                float vote = StringAddons.ConvertToFloat(Meeting.currentMeetingCountdown.Value);
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
            VotingMenu.transform.SetSiblingIndex(13);

            plist.transform.SetParent(VotingMenu.transform, false);

            string rawHeaderText = (LMConfig.defaultDiscussTime == 0) ? "VOTE: " : "DISCUSS: ";
            float vote = StringAddons.ConvertToFloat(Meeting.currentMeetingCountdown.Value);
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
            skipCheckSprite.sprite = Plugin.CheckboxEmptyIcon;
            skipBtn.onClick.AddListener(() => Voting.SkipButtonClick(skipCheckSprite));

        }


        public static void VoteButton(GameObject playerListSlot)
        {
            /*
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (!Voting.allVotes.Value.ContainsKey($"{i}")) continue;
            }
            */
            GameObject raw_playerVoteBtn = playerListSlot.transform.Find("Image/QuickmenuOverride(Clone)/Holder").gameObject;
            Dictionary<int, GameObject> playerVoteBtn = new Dictionary<int, GameObject>();

            foreach (GameObject player in GOTools.GetAllChildren(raw_playerVoteBtn))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton/PName").GetComponent<TextMeshProUGUI>();
                string raw_id = pName.text.Split('#')[1];
                int id = StringAddons.ConvertToInt(raw_id);
                Plugin.mls.LogInfo($">>>ClonedPlayerList: pName: {pName.text} | playerID: {raw_id}");

                if (StartOfRound.Instance.allPlayerScripts[id].isPlayerDead) continue;
                if (!Voting.allVotes.Value.ContainsKey($"{id}")) continue;

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
                plrCheckSprite.sprite = (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) ? xButtonSprite : Plugin.CheckboxEmptyIcon;

                Plugin.mls.LogInfo($">>> Logging num: {index}");
                plrbutton.onClick.AddListener(() => Voting.VoteButtonClick(index, plrCheckSprite));

            }
            /*
            foreach (KeyValuePair<string, string> i in Voting.allVotes.Value)
            {
                int index = StringAddons.ConvertToInt(i.Key); // because apparently using just "i" doesn't work for events, it needs to be stored in a variable
                string path = "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/KickButton";

                //GameObject plistClone = playerListSlot.transform.Find("Image").gameObject;
                GameObject playerVoteBtn = playerListSlot.transform.Find(path).gameObject;
                playerVoteBtn.gameObject.name = "VoteButton";
                playerVoteBtn.SetActive(true);

                UnityEngine.UI.Image plrCheckSprite = playerVoteBtn.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button plrbutton = playerVoteBtn.GetComponent<UnityEngine.UI.Button>();
                plrCheckSprite.sprite = (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) ? xButtonSprite : Plugin.CheckboxEmptyIcon;

                Plugin.mls.LogInfo($">>> Logging num: {index}");
                plrbutton.onClick.AddListener(() => Voting.VoteButtonClick(index, plrCheckSprite));

            }
            */


        }


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
                    plrObj.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.CheckboxEmptyIcon;
                }

            }


        }


        private static void ShowVotesForPlayers(GameObject playerListSlot)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (!Voting.allVotes.Value.ContainsKey($"{i}")) continue;

                string votes = "VOTES: " + Voting.allVotes.Value[$"{i}"];
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
                    if (pName.text.EndsWith($"#{userID}"))
                    {
                        GameObject plrObj = players.transform.Find("Votes").gameObject;
                        plrObj.gameObject.GetComponent<TextMeshProUGUI>().text = "VOTES: " + Voting.allVotes.Value[$"{userID}"];
                        break;
                    }

                }
            }
            else
            {
                GameObject playerVoteObj = GameObject.Find(path);
                TextMeshProUGUI playerVoteText = playerVoteObj.GetComponent<TextMeshProUGUI>();
                playerVoteText.text = "VOTES: " + Voting.allVotes.Value[$"{userID}"];
            }

        }


        public static GameObject ClonedPlayerList(GameObject plist)
        {
            string path = "Image/QuickmenuOverride(Clone)/Holder";

            GameObject raw_votelist = Plugin.Instantiate(plist);
            GameObject votelist = raw_votelist.transform.Find(path).gameObject;

            foreach (GameObject player in GOTools.GetAllChildren(votelist))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton/PName").GetComponent<TextMeshProUGUI>();
                string raw_id = pName.text.Split('#')[1];
                int id = StringAddons.ConvertToInt(raw_id);
                Plugin.mls.LogInfo($">>>ClonedPlayerList: pName: {pName.text} | playerID: {raw_id}");

                if (StartOfRound.Instance.allPlayerScripts[id].isPlayerDead)
                {
                    player.gameObject.SetActive(false);
                }
            }

            foreach (GameObject player in GOTools.GetAllChildren(votelist))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton").transform.Find("PName").GetComponent<TextMeshProUGUI>();
                string raw_id = pName.text.Split('#')[1];
                int id = StringAddons.ConvertToInt(raw_id);
                Plugin.mls.LogInfo($">>>A444ClonedPlayerList: pName: {pName.text} | playerID: {raw_id}");
            }

            return raw_votelist;
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

                if (pName.text.EndsWith($"#{playerID}"))
                {
                    Plugin.Destroy(player.gameObject);
                    break;
                }

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
                    break;
                }
            }
        }

    }
}
