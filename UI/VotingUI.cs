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
            foreach (GameObject obj in GOTools.GetAllChildren(plist))
            {
                if (obj.gameObject.name.ToLower() == "image")
                {
                    foreach (GameObject value in GOTools.GetAllChildren(obj))
                    {
                        if (value.gameObject.name.ToLower() == "header")
                        {
                            return value;
                        }
                    }
                    break;
                }
            }
            return plist;
        }


        public static void VoteButton(GameObject parent)
        {
            // Note: Optimize / Make readable later

            List<GameObject> parentOBJ = GOTools.GetAllChildren(parent);
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            if (moreCompany == false)
            {
                foreach (GameObject plistClone in parentOBJ)
                {
                    if (plistClone.gameObject.name.ToLower() == "image")
                    {
                        foreach (GameObject img in GOTools.GetAllChildren(plistClone))
                        {
                            if (img.gameObject.name.ToLower() == "playerlistslot")
                            {
                                foreach (GameObject playerListSlot in GOTools.GetAllChildren(img))
                                {
                                    if (playerListSlot.gameObject.name.ToLower() == "kickbutton")
                                    {
                                        GameObject playerVoteBtn = playerListSlot;
                                        playerVoteBtn.SetActive(true);

                                        GameObject skipButton = Plugin.Instantiate(playerVoteBtn);
                                        RectTransform skipButtonRect = skipButton.GetComponent<RectTransform>();
                                        skipButton.transform.SetParent(plistClone.transform, false);
                                        skipButtonRect.anchoredPosition = new Vector2(-100f, -155f);

        
                                        GameObject skipText = Plugin.Instantiate(GetImageHeader(parent));
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
                                }

                            }
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject plistClone in parentOBJ)
                {
                    if (plistClone.gameObject.name.ToLower() == "image")
                    {
                        foreach (GameObject img in GOTools.GetAllChildren(plistClone))
                        {
                            if (img.gameObject.name.ToLower().Contains("quickmenuoverride"))
                            {
                                foreach (GameObject quickMenu in GOTools.GetAllChildren(img))
                                {
                                    if (quickMenu.gameObject.name.ToLower() == "holder")
                                    {
                                        foreach (GameObject holder in GOTools.GetAllChildren(quickMenu))
                                        {
                                            if (holder.gameObject.name.ToLower().Contains("playerlistslot"))
                                            {
                                                foreach (GameObject obj in GOTools.GetAllChildren(holder))
                                                {
                                                    if (obj.gameObject.name.ToLower() == "kickbutton")
                                                    {
                                                        GameObject playerVoteBtn = obj;
                                                        playerVoteBtn.SetActive(true);

                                                        GameObject skipButton = Plugin.Instantiate(playerVoteBtn);
                                                        RectTransform skipButtonRect = skipButton.GetComponent<RectTransform>();
                                                        skipButton.transform.SetParent(plistClone.transform, false);
                                                        skipButtonRect.anchoredPosition = new Vector2(-100f, -155f);


                                                        GameObject skipText = Plugin.Instantiate(GetImageHeader(parent));
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
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


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


        private static void ShowVotesForPlayers(GameObject parent)
        {
            // Note: Optimize / Make readable later

            List<GameObject> parentOBJ = GOTools.GetAllChildren(parent);
            string votes = "Votes: ";
            bool moreCompany = Plugin.FoundThisMod("me.swipez.melonloader.morecompany");

            if (moreCompany == false)
            {
                foreach (GameObject plistClone in parentOBJ)
                {
                    if (plistClone.gameObject.name.ToLower() == "image")
                    {
                        foreach (GameObject img in GOTools.GetAllChildren(plistClone))
                        {
                            if (img.gameObject.name.ToLower() == "playerlistslot")
                            {
                                foreach (GameObject playerListSlot in GOTools.GetAllChildren(img))
                                {
                                    if (playerListSlot.gameObject.name.ToLower() == "voicevolumeslider")
                                    {
                                        playerListSlot.SetActive(true);
                                        foreach (GameObject voiceSlider in GOTools.GetAllChildren(playerListSlot))
                                        {
                                            if (voiceSlider.gameObject.name.ToLower() == "image" || voiceSlider.gameObject.name.ToLower() == "slider")
                                            {
                                                voiceSlider.SetActive(false);
                                            }
                                            if (voiceSlider.gameObject.name.ToLower() == "text (1)")
                                            {
                                                voiceSlider.SetActive(true);
                                                voiceSlider.gameObject.GetComponent<TextMeshProUGUI>().text = votes;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject plistClone in parentOBJ)
                {
                    if (plistClone.gameObject.name.ToLower() == "image")
                    {
                        foreach (GameObject img in GOTools.GetAllChildren(plistClone))
                        {
                            if (img.gameObject.name.ToLower().Contains("quickmenuoverride"))
                            {
                                foreach (GameObject quickMenu in GOTools.GetAllChildren(img))
                                {
                                    if (quickMenu.gameObject.name.ToLower() == "holder")
                                    {
                                        foreach (GameObject holder in GOTools.GetAllChildren(quickMenu))
                                        {
                                            if (holder.gameObject.name.ToLower().Contains("playerlistslot"))
                                            {
                                                foreach (GameObject obj in GOTools.GetAllChildren(holder))
                                                {
                                                    if (obj.gameObject.name.ToLower() == "voicevolumeslider")
                                                    {
                                                        obj.SetActive(false);
                                                    }
                                                    if (obj.gameObject.name.ToLower() == "text (1)")
                                                    {
                                                        obj.GetComponent<TextMeshProUGUI>().text = votes;
                                                        obj.SetActive(true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }




    }
}
