using BepInEx.Bootstrap;
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
        private static GameObject playerListPanel;

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
                playerListPanel = plist;

                foreach (GameObject obj in GOTools.GetAllChildren(plist))
                {
                    if (obj.gameObject.name.ToLower() == "image")
                    {
                        foreach (GameObject value in GOTools.GetAllChildren(obj))
                        {
                            if (value.gameObject.name.ToLower() == "header")
                            {
                                value.GetComponent<TextMeshProUGUI>().text = "VOTE:";
                                break;
                            }
                        }
                        break;
                    }
                }

                ShowVotesForPlayers(plist);
                VoteButton(plist);

            }

        }




        public static void VoteButton(GameObject parent)
        {
            // Note: Optimize / Make readable later

            List<GameObject> parentOBJ = GOTools.GetAllChildren(parent);
            string votes = "Votes: ";
            bool moreCompany = false;
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                string modGuid = "me.swipez.melonloader.morecompany";
                if (metadata.GUID.Equals(modGuid))
                {
                    Plugin.mls.LogInfo($"Found {modGuid}");
                    moreCompany = true;
                    break;
                }
            }

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
                                                    if (obj.gameObject.name.ToLower() == "kickbutton")
                                                    {
                                                        obj.SetActive(true);
                                                        
                                                        UnityEngine.UI.Button button = obj.GetComponent<UnityEngine.UI.Button>();

                                                        int index = 0; // use player id
                                                        button.onClick.AddListener(() => VoteButtonClick(index));
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


        private static void VoteButtonClick(int index)
        {
            Plugin.mls.LogInfo($"blah blah {index} clicked.");
        }


        private static void ShowVotesForPlayers(GameObject parent)
        {
            // Note: Optimize / Make readable later

            List<GameObject> parentOBJ = GOTools.GetAllChildren(parent);
            string votes = "Votes: ";
            bool moreCompany = false;
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                string modGuid = "me.swipez.melonloader.morecompany";
                if (metadata.GUID.Equals(modGuid))
                {
                    Plugin.mls.LogInfo($"Found {modGuid}");
                    moreCompany = true;
                    break;
                }
            }

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
