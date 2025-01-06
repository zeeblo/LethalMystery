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

        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(QuickMenuManager __instance)
        {
            if (Keyboard.current.digit6Key.wasPressedThisFrame)
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

            }

        }


        private static GameObject GetImageHeader(GameObject plist)
        {
            return plist.transform.Find("Image/Header").gameObject;
        }


        public static void VoteButton(GameObject playerListSlot)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string path = (moreCompany == false) ? "Image/PlayerListSlot/KickButton" : "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/KickButton";

            GameObject plistClone = playerListSlot.transform.Find("Image").gameObject;
            GameObject playerVoteBtn = playerListSlot.transform.Find(path).gameObject;
            playerVoteBtn.gameObject.name = "VoteButton";
            playerVoteBtn.SetActive(true);

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


            UnityEngine.UI.Image plrCheckSprite = playerVoteBtn.GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image skipCheckSprite = skipButton.GetComponent<UnityEngine.UI.Image>();
            plrCheckSprite.sprite = Plugin.CheckboxEmptyIcon;
            skipCheckSprite.sprite = Plugin.CheckboxEmptyIcon;

            UnityEngine.UI.Button plrbutton = playerVoteBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Button skipBtn = skipButton.GetComponent<UnityEngine.UI.Button>();

            int index = 0; // use player id
            plrbutton.onClick.AddListener(() => Voting.VoteButtonClick(index, plrCheckSprite));
            skipBtn.onClick.AddListener(() => Voting.SkipButtonClick(index, skipCheckSprite));



        }


        private static void ShowVotesForPlayers(GameObject playerListSlot)
        {
            string votes = "VOTES: 0";
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string path = (moreCompany == false) ? "Image/PlayerListSlot/VoiceVolumeSlider" : "Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/VoiceVolumeSlider";

            GameObject VolSlider = playerListSlot.transform.Find(path).gameObject;
            GameObject playerVoteObj = playerListSlot; // placeholder
            if (moreCompany == false)
            {
                playerVoteObj = playerListSlot.transform.Find($"{path}/Text (1)").gameObject;
            }
            else
            {
                playerVoteObj = playerListSlot.transform.Find("Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/Text (1)").gameObject;
            }


            VolSlider.SetActive(true);
            VolSlider.transform.Find("Image").gameObject.SetActive(false);
            VolSlider.transform.Find("Slider").gameObject.SetActive(false);

            playerVoteObj.name = "Votes";
            playerVoteObj.SetActive(true);
            playerVoteObj.gameObject.GetComponent<TextMeshProUGUI>().text = votes;


        }



        public static void UpdateSkipText()
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = (moreCompany == false) ? $"{parentPath}/PlayerList(Clone)/Image/SkipText" : $"{parentPath}/Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/SkipText";

            GameObject skipObj = GameObject.Find(path);
            TextMeshProUGUI skipText = skipObj.GetComponent<TextMeshProUGUI>();
            skipText.text = "SKIP: " + Voting.skipVotes.Value;
        }


        public static void UpdateVoteText(int index)
        {
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");
            string parentPath = "Systems/UI/Canvas/VotingMenu";
            string path = (moreCompany == false) ? $"{parentPath}/PlayerList(Clone)/Image/PlayerListSlot/VoiceVolumeSlider/Votes" : $"{parentPath}/Image/QuickmenuOverride(Clone)/Holder/PlayerListSlot(Clone)/Votes";

            GameObject playerVoteObj = GameObject.Find(path);
            TextMeshProUGUI playerVoteText = playerVoteObj.GetComponent<TextMeshProUGUI>();
            playerVoteText.text = "VOTES: " + Voting.allVotes.Value[$"{index}"];
        }

    }
}
