using System.Collections.Generic;
using LethalMystery.MainGame;
using LethalMystery.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalMystery.UI
{
    internal class wipVUI
    {

        public static GameObject voteIcon;
        public static GameObject playerList;
        public static GameObject nametagBG;
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




        public static void NameTagBg()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            nametagBG = new GameObject("nametag");
            nametagBG.transform.SetParent(parentUI.transform, false);
            Image bgImage = nametagBG.AddComponent<Image>();
            bgImage.color = new Color(0.6509f, 0.2091f, 0.0031f, 1);

            nametagBG.layer = 5;
            nametagBG.transform.SetSiblingIndex(8);

            RectTransform bgRect = nametagBG.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(166.38f, 23.19f);
            bgRect.anchoredPosition = Vector2.zero;
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



        public static void CreatePlayerList()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            playerList = new GameObject("PlayerList");
            playerList.transform.SetParent(parentUI.transform, false);
            Image rawImg = playerList.AddComponent<Image>();
            rawImg.color = new Color(0.4627f, 0, 0, 1);

            playerList.layer = 5;
            playerList.transform.SetSiblingIndex(13); // before quickmenu

            RectTransform rectBorder = playerList.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(150, 250);
            rectBorder.anchoredPosition = new Vector2(-140, 0);

            Outline outline = playerList.AddComponent<Outline>();
            outline.effectColor = new Color(0.996f, 0.095f, 0, 1f);
            outline.effectDistance = new Vector2(2f, 2f);


            allVoteUIObjects.Add(playerList);
        }



    }
}
