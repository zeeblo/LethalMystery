using HarmonyLib;
using UnityEngine;

namespace LethalMystery.UI
{

    [HarmonyPatch]
    internal class MainMenu
    {

        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
        [HarmonyPostfix]
        private static void StartPatch()
        {
            CreateLMMainMenu();
        }


        private static void CreateLMMainMenu()
        {
            UnityEngine.UI.Image BannerImage = new GameObject("Banner").AddComponent<UnityEngine.UI.Image>();
            GameObject HeaderImage = GameObject.Find("Canvas/MenuContainer/MainButtons/HeaderImage");
            GameObject LoadingImage = GameObject.Find("Canvas/MenuContainer/LoadingScreen/Image");

            if (HeaderImage != null && Plugin.LethalMysteryLogo != null && BannerImage != null)
            {

                BannerImage.transform.SetParent(GameObject.Find("Canvas/MenuContainer/MainButtons/").transform);
                BannerImage.transform.SetAsFirstSibling();
                BannerImage.rectTransform.offsetMin = Vector2.zero;
                BannerImage.rectTransform.offsetMax = Vector2.zero;
                BannerImage.rectTransform.anchorMin = Vector2.zero;
                BannerImage.rectTransform.anchorMax = Vector2.one;
                BannerImage.rectTransform.localPosition = Vector3.zero;
                BannerImage.transform.localScale = new Vector3(1.1125f, 0.9253f, 5.1671f);

                BannerImage.sprite = Plugin.LethalMysteryBanner;
                HeaderImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryLogo;
                LoadingImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryLogo;
            }


        }
    }
}
