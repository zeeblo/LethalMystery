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
        public static GameObject border;
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
            bindtxt.text = "[ " + $"{LMConfig.showMapBind.Value.ToUpper()}" + " ]"; // Reminder: Change to VoteKeybind
            bindtxt.fontSize = 9;
            bindtxt.fontWeight = FontWeight.Heavy;
            bindtxt.alignment = TextAlignmentOptions.Center;
            bindtxt.color = new Color(0.996f, 0.095f, 0, 1f);

            RectTransform bindRect = keybind.GetComponent<RectTransform>();
            bindRect.sizeDelta = new Vector2(280, 80);
            bindRect.pivot = new Vector2(0.5f, 0.9f);
            bindRect.anchoredPosition = Vector2.zero;
        }




        private static void CreateBorder()
        {
            GameObject parentUI = GameObject.Find("Systems/UI/Canvas");
            border = new GameObject("VoteMenu");
            border.transform.SetParent(parentUI.transform, false);
            Image rawImg = border.AddComponent<Image>();
            rawImg.color = new Color(0.996f, 0.095f, 0, 1f);

            border.layer = 5;
            border.transform.SetSiblingIndex(13); // before quickmenu

            RectTransform rectBorder = border.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(450, 450);
            rectBorder.anchoredPosition = Vector2.zero;

            allVoteUIObjects.Add(border);
        }



    }
}
