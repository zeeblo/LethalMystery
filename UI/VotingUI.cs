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

        private static GameObject updatedPlayerList;

        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(QuickMenuManager __instance)
        {
            if (Keyboard.current.digit6Key.wasPressedThisFrame)
            {
                CreateVotingGUI(__instance);
            }

            if (Keyboard.current.digit9Key.wasPressedThisFrame)
            {
                List<int> playerVotes = new List<int>();
                foreach (KeyValuePair<string, string> v in Voting.allVotes.Value)
                {
                    int voteNum = StringAddons.ConvertToInt(v.Value);
                    playerVotes.Add(voteNum);
                }
                int skipVotes = StringAddons.ConvertToInt(Voting.skipVotes.Value);
                playerVotes.Add(skipVotes);


                int maxVote = playerVotes.Max(n => n);
                playerVotes.Remove(maxVote);

                foreach (int vote in playerVotes)
                {
                    if (vote == maxVote)
                    {
                        Plugin.mls.LogInfo(">>> Skipping Vote <<<");
                        return;
                    }
                }

                foreach (KeyValuePair<string, string> v in Voting.allVotes.Value)
                {
                    int voteNum = StringAddons.ConvertToInt(v.Value);
                    if (voteNum == maxVote)
                    {
                        int plrID = StringAddons.ConvertToInt(v.Key);
                        string PlayerName = StartOfRound.Instance.allPlayerScripts[plrID].playerUsername;
                        Plugin.mls.LogInfo($">>> Voted {PlayerName} <<<");
                        break;
                    }

                }
                

            }

        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
        [HarmonyPostfix]
        private static void UserLeft(ulong clientId)
        {
            //if (StringAddons.ConvertToBool(Plugin.inMeeting.Value) == false) return;

            UpdatePlayerList(clientId);

            Plugin.mls.LogInfo($"<<< localPlayerVote: {Voting.localPlayerVote}");
            if ($"{clientId-1}" == $"{Voting.localPlayerVote}")
            {
                Plugin.mls.LogInfo(">>> Player has been given back their vote.");
                Voting.hasVoted = false;
            }
        }


        private static GameObject CreateVotingGUI(QuickMenuManager __instance)
        {
            GameObject canvas = GameObject.Find("Systems/UI/Canvas/");
            GameObject VotingMenu = new GameObject("VotingMenu");
            GameObject plist = Plugin.Instantiate(__instance.playerListPanel);

            VotingMenu.layer = 5;
            VotingMenu.transform.SetParent(canvas.transform, false);
            VotingMenu.transform.SetSiblingIndex(13);

            plist.transform.SetParent(VotingMenu.transform, false);
            
            GetImageHeader(plist).GetComponent<TextMeshProUGUI>().text = "VOTE:";
            ShowVotesForPlayers(plist);
            VoteButton(plist);
            SkipButton(plist);
            CheckPlayerList();
            return VotingMenu;
        }

        private static GameObject GetImageHeader(GameObject plist)
        {
            return plist.transform.Find("Image/Header").gameObject;
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


            GameObject skipObj = Plugin.Instantiate(GetImageHeader(playerListSlot));
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
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {

                int index = i; // because apparently using just "i" doesn't work for events, it needs to be stored in a variable
                string playerBeingVoted = (i > 0) ? $"PlayerListSlot ({i})" : "PlayerListSlot";
                string path = (moreCompany == false) ? $"Image/{playerBeingVoted}/KickButton" : "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/KickButton";

                GameObject plistClone = playerListSlot.transform.Find("Image").gameObject;
                GameObject playerVoteBtn = playerListSlot.transform.Find(path).gameObject;
                playerVoteBtn.gameObject.name = "VoteButton";
                playerVoteBtn.SetActive(true);

                UnityEngine.UI.Image plrCheckSprite = playerVoteBtn.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button plrbutton = playerVoteBtn.GetComponent<UnityEngine.UI.Button>();
                plrCheckSprite.sprite = Plugin.CheckboxEmptyIcon;

                Plugin.mls.LogInfo($">>> Logging num: {i}");
                plrbutton.onClick.AddListener(() => Voting.VoteButtonClick(index, plrCheckSprite));

                // If the next playerlistslot is 0 then it's a dummy player script and it should be ignored.
                if (StartOfRound.Instance.allPlayerScripts[i + 1].actualClientId == 0) break;
            }

        }


        private static void ShowVotesForPlayers(GameObject playerListSlot)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
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


        /// <summary>
        /// Refresh playerlist when player leaves or dies
        /// </summary>
        private static void UpdatePlayerList(ulong playerID)
        {
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = $"{parentPath}/PlayerList(Clone)/Image/QuickmenuOverride(Clone)/Holder";

            GameObject playerVoteObj = GameObject.Find(path);
            foreach (GameObject player in GOTools.GetAllChildren(playerVoteObj))
            {
                TextMeshProUGUI pName = player.transform.Find("PlayerNameButton").transform.Find("PName").GetComponent<TextMeshProUGUI>();
                Plugin.mls.LogInfo($">>> pName: {pName.text}");

                // MoreCompany increases the actualClientID except the host.
                // Check if the host is being removed first or the other clients.
                if (pName.text.EndsWith($"#{playerID}") || pName.text.EndsWith($"#{playerID-1}"))
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
                    Plugin.mls.LogInfo($">>> PlayerIsDead: {plr.actualClientId}");
                    UpdatePlayerList(plr.actualClientId);
                    break;
                }
            }
        }

    }
}
