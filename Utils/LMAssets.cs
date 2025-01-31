using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalMystery.Utils
{
    internal class LMAssets
    {

        public static Sprite? KnifeIcon;
        public static Sprite? LethalMysteryLogo;
        public static Sprite? LethalMysteryBanner;
        public static Sprite? CheckboxEmptyIcon;
        public static Sprite? CheckboxEnabledIcon;
        public static GameObject SkeldMap;



        public static void LoadAllAssets()
        {
            SpriteLoader();
            MapLoader();
        }


        private static void SpriteLoader()
        {
            string BundleDir = Plugin.MainDir + "\\Assets\\Assetbundles\\items";

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            Texture2D KnifeTexture = myBundle.LoadAsset<Texture2D>("sprite_knife.png");
            Texture2D LogoTexture = myBundle.LoadAsset<Texture2D>("logo_a.png");
            Texture2D BannerTexture = myBundle.LoadAsset<Texture2D>("default_banner.jpg");
            Texture2D CheckboxEmptyTexture = myBundle.LoadAsset<Texture2D>("btn_empty.png");
            Texture2D CheckboxEnabledTexture = myBundle.LoadAsset<Texture2D>("btn_check.png");
            KnifeIcon = Sprite.Create(
                KnifeTexture,
                new Rect(0, 0, KnifeTexture.width, KnifeTexture.height),
                new Vector2(0, 0)
            );
            LethalMysteryLogo = Sprite.Create(
                LogoTexture,
                new Rect(0, 0, LogoTexture.width, LogoTexture.height),
                new Vector2(0, 0)
            );
            LethalMysteryBanner = Sprite.Create(
                BannerTexture,
                new Rect(0, 0, BannerTexture.width, BannerTexture.height),
                new Vector2(0, 0)
            );
            CheckboxEmptyIcon = Sprite.Create(
                CheckboxEmptyTexture,
                new Rect(0, 0, CheckboxEmptyTexture.width, CheckboxEmptyTexture.height),
                new Vector2(0, 0)
            );
            CheckboxEnabledIcon = Sprite.Create(
                CheckboxEnabledTexture,
                new Rect(0, 0, CheckboxEnabledTexture.width, CheckboxEnabledTexture.height),
                new Vector2(0, 0)
            );
        }



        private static void MapLoader()
        {
            string BundleDir = Plugin.MainDir + "\\Assets\\Assetbundles\\lm_maps";

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            SkeldMap = myBundle.LoadAsset<GameObject>("Skeld.prefab");

        }
    }
}
