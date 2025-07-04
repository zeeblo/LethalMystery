﻿using System;
using System.Collections.Generic;
using System.IO;
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
        public static Sprite? CheckboxDisabledIcon;
        public static Sprite? MapIcon;
        public static Sprite? VoteIcon;
        public static Sprite? MapIconHover;
        public static Sprite? PlainArrow;
        public static Sprite? PlainArrowHover;
        public static Sprite? LightSwitch;
        public static Sprite? LightSwitchHover;
        public static Sprite? LightSwitchSelect;
        public static AudioClip SFX_VentOpen;
        public static AudioClip SFX_VentLeave;
        public static AudioClip SFX_VentSwitch;
        public static GameObject SkeldMap;
        public static GameObject OfficeMap;



        public static void LoadAllAssets()
        {
            ItemsLoader();
            DefaultMapLoader();
        }




        private static void ItemsLoader()
        {
            string BundleDir = Plugin.GetAssetPath("items");

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            Texture2D KnifeTexture = myBundle.LoadAsset<Texture2D>("sprite_knife.png");
            Texture2D LogoTexture = myBundle.LoadAsset<Texture2D>("logo_a.png");
            Texture2D BannerTexture = myBundle.LoadAsset<Texture2D>("default_banner.jpg");
            Texture2D CheckboxEmptyTexture = myBundle.LoadAsset<Texture2D>("btn_empty.png");
            Texture2D CheckboxEnabledTexture = myBundle.LoadAsset<Texture2D>("btn_check.png");
            Texture2D CheckboxDisabledTexture = myBundle.LoadAsset<Texture2D>("btn_X.png");
            Texture2D MapIconTexture = myBundle.LoadAsset<Texture2D>("mapIcon.png");
            Texture2D MapIconHoverTexture = myBundle.LoadAsset<Texture2D>("mapIcon_hover.png");
            Texture2D VoteIconTexture = myBundle.LoadAsset<Texture2D>("voteIcon.png");
            Texture2D PlainArrowTexture = myBundle.LoadAsset<Texture2D>("plain-arrow.png");
            Texture2D PlainArrowHoverTexture = myBundle.LoadAsset<Texture2D>("plain-arrow_hover.png");
            Texture2D LightSwitchTexture = myBundle.LoadAsset<Texture2D>("light-bulb.png");
            Texture2D LightSwitchHoverTexture = myBundle.LoadAsset<Texture2D>("light-bulb_hover.png");
            Texture2D LightSwitchSelectTexture = myBundle.LoadAsset<Texture2D>("light-bulb_selected.png");
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
            CheckboxDisabledIcon = Sprite.Create(
                CheckboxDisabledTexture,
                new Rect(0, 0, CheckboxDisabledTexture.width, CheckboxDisabledTexture.height),
                new Vector2(0, 0)
            );
            MapIcon = Sprite.Create(
                MapIconTexture,
                new Rect(0, 0, MapIconTexture.width, MapIconTexture.height),
                new Vector2(0, 0)
            );
            VoteIcon = Sprite.Create(
                VoteIconTexture,
                new Rect(0, 0, VoteIconTexture.width, VoteIconTexture.height),
                new Vector2(0, 0)
            );
            MapIconHover = Sprite.Create(
                MapIconHoverTexture,
                new Rect(0, 0, MapIconHoverTexture.width, MapIconHoverTexture.height),
                new Vector2(0, 0)
            );
            PlainArrow = Sprite.Create(
                PlainArrowTexture,
                new Rect(0, 0, PlainArrowTexture.width, PlainArrowTexture.height),
                new Vector2(0, 0)
            );
            PlainArrowHover = Sprite.Create(
                PlainArrowHoverTexture,
                new Rect(0, 0, PlainArrowHoverTexture.width, PlainArrowHoverTexture.height),
                new Vector2(0, 0)
            );
            LightSwitch = Sprite.Create(
                LightSwitchTexture,
                new Rect(0, 0, LightSwitchTexture.width, LightSwitchTexture.height),
                new Vector2(0, 0)
            );
            LightSwitchHover = Sprite.Create(
                LightSwitchHoverTexture,
                new Rect(0, 0, LightSwitchHoverTexture.width, LightSwitchHoverTexture.height),
                new Vector2(0, 0)
            );
            LightSwitchSelect = Sprite.Create(
                LightSwitchSelectTexture,
                new Rect(0, 0, LightSwitchSelectTexture.width, LightSwitchSelectTexture.height),
                new Vector2(0, 0)
            );

            AudioLoader(myBundle);
        }

        private static void AudioLoader(AssetBundle bundle)
        {
            SFX_VentOpen = bundle.LoadAsset<AudioClip>("vent_open.wav");
            SFX_VentLeave = bundle.LoadAsset<AudioClip>("vent_leave.wav");
            SFX_VentSwitch = bundle.LoadAsset<AudioClip>("vent_switch.MP3");   
        }


        private static void DefaultMapLoader()
        {
            string BundleDir = Plugin.GetAssetPath("lm_maps");

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            SkeldMap = myBundle.LoadAsset<GameObject>("Skeld.prefab");
            OfficeMap = myBundle.LoadAsset<GameObject>("Office.prefab");
        }
    }
}
