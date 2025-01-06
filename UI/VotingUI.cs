using HarmonyLib;
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
            playerVoteBtn.SetActive(true);

            GameObject skipButton = Plugin.Instantiate(playerVoteBtn);
            RectTransform skipButtonRect = skipButton.GetComponent<RectTransform>();
            skipButton.transform.SetParent(plistClone.transform, false);
            skipButtonRect.anchoredPosition = new Vector2(-100f, -155f);


            GameObject skipText = Plugin.Instantiate(GetImageHeader(playerListSlot));
            RectTransform skipTextRect = skipText.GetComponent<RectTransform>();
            TextMeshProUGUI skipTextTXT = skipText.GetComponent<TextMeshProUGUI>();
            skipTextTXT.fontSize = 17;
            skipTextTXT.text = "SKIP";
            skipText.transform.SetParent(plistClone.transform, false);
            skipTextRect.anchoredPosition = new Vector2(45f, -318f);


            UnityEngine.UI.Image plrCheckSprite = playerVoteBtn.GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image skipCheckSprite = skipButton.GetComponent<UnityEngine.UI.Image>();
            plrCheckSprite.sprite = Plugin.CheckboxEmptyIcon;
            skipCheckSprite.sprite = Plugin.CheckboxEmptyIcon;

            UnityEngine.UI.Button plrbutton = playerVoteBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Button skipBtn = skipButton.GetComponent<UnityEngine.UI.Button>();

            int index = 0; // use player id
            plrbutton.onClick.AddListener(() => VoteButtonClick(index, plrCheckSprite));
            skipBtn.onClick.AddListener(() => SkipButtonClick(index, skipCheckSprite));



        }


        private static void SkipButtonClick(int index, UnityEngine.UI.Image check)
        {
            Plugin.mls.LogInfo($"Skip Button {index} clicked.");
            check.sprite = Plugin.CheckboxEnabledIcon;
        }

        private static void VoteButtonClick(int index, UnityEngine.UI.Image check)
        {
            Plugin.mls.LogInfo($"Vote Button {index} clicked.");
            check.sprite = Plugin.CheckboxEnabledIcon;
        }


        private static void ShowVotesForPlayers(GameObject playerListSlot)
        {
            string votes = "Votes: ";
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

            playerVoteObj.SetActive(true);
            playerVoteObj.gameObject.GetComponent<TextMeshProUGUI>().text = votes;


        }




    }
}
