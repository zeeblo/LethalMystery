using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;



namespace LethalMystery.GameMech
{

    internal class Tests
    {
        public static Vector3 modelPosition = new Vector3(80f, 20f, 20f);
        public static Vector3 groundPosition = new Vector3(80f, 18.8f, 20f);
        public static Vector3 cameraPosition = new Vector3(80f, 31.05f, 26f);
        public static Vector3 lightPosition = new Vector3(80f, 34f, 20f);
        public static GameObject? playerObj;
        public static GameObject? lght;
        public static Camera? introCamera;
        public static bool entered = false;
        public static bool entered6 = false;
        public static bool disableMovement = false;
        private static GameObject? HeaderImage;
        private static GameObject? BannerImage;
        private static GameObject? BGCanvas;


        [HarmonyPatch]
        internal class AdminCMDS
        {

            [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Update))]
            [HarmonyPostfix]
            private static void UpdatePatch()
            {
                /*
                HeaderImage = GameObject.Find("Canvas/MenuContainer/MainButtons/HeaderImage");
                if (HeaderImage != null && Plugin.LethalMysteryLogo != null && BGCanvas != null && BannerImage != null)
                {

                    BGCanvas.GetComponent<UnityEngine.Canvas>().worldCamera = GameObject.Find("UICamera").GetComponent<UnityEngine.Camera>();
                    BGCanvas.GetComponent<UnityEngine.Canvas>().renderMode = RenderMode.WorldSpace;
                    

                    BannerImage.transform.SetParent(BGCanvas.transform);
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.offsetMin = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.offsetMax = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.anchorMin = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.anchorMax = Vector2.one;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.localPosition = Vector3.zero;


                    BannerImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryBanner;
                    HeaderImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryLogo;
                }
                */
                HeaderImage = GameObject.Find("Canvas/MenuContainer/MainButtons/HeaderImage");
                if (HeaderImage != null && Plugin.LethalMysteryLogo != null && BannerImage != null)
                {

                    BannerImage.transform.SetParent(GameObject.Find("Canvas/MenuContainer/MainButtons/").transform);
                    BannerImage.transform.SetAsFirstSibling(); // GetComponent<UnityEngine.UI.Image>().rectTransform.SetAsFirstSibling();
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.offsetMin = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.offsetMax = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.anchorMin = Vector2.zero;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.anchorMax = Vector2.one;
                    BannerImage.GetComponent<UnityEngine.UI.Image>().rectTransform.localPosition = Vector3.zero;
                    BannerImage.transform.localScale = new Vector3(1.1125f, 0.9253f, 5.1671f);


                    BannerImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryBanner;
                    HeaderImage.GetComponent<UnityEngine.UI.Image>().sprite = Plugin.LethalMysteryLogo;
                }
            }

            [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
            [HarmonyPostfix]
            private static void StartPatch()
            {
                BannerImage = new GameObject("Banner");
                BGCanvas = new GameObject("BackgroundCanvas");
                BannerImage.AddComponent<UnityEngine.UI.Image>();
                BGCanvas.AddComponent<UnityEngine.Canvas>();
            }
        }




        [HarmonyPatch(typeof(HUDManager))]
        internal class AdminCMDS_2
        {
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
            [HarmonyPostfix]
            private static void Keys(HUDManager __instance)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame)
                {
                    __instance.drunknessFilter.weight = 15f;
                }
                if (Keyboard.current.digit2Key.wasPressedThisFrame)
                {
                    __instance.insanityScreenFilter.weight = 5f;
                }
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }

            }
        }
    }
}
