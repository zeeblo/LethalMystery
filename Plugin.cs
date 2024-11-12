using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalMystery.GameMech;
using LethalMystery.Patches;
using LethalMystery.Players;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



namespace LethalMystery
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "dev.zeeblo.LethalMystery";
        private const string modName = "zeeblo.LethalMystery";
        private const string modVersion = "0.1.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? instance;
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);


        internal static SelectableLevel? currentLevel;
        internal static EnemyVent[]? currentLevelVents;
        internal static RoundManager? currentRound;
        internal static ConfigEntry<string>? PrefixSetting;
        internal static bool usingTerminal = false;
        internal static bool isHost;


        public static bool inMeeting = false;
        public static float defaultMeetingCountdown = 20f;
        public static float currentMeetingCountdown = defaultMeetingCountdown;
        public static float defaultMeetingCooldown = 10f;
        public static float MeetingCooldown = defaultMeetingCooldown;
        public static int defaultMeetingNum = 1;
        public static int MeetingNum = defaultMeetingNum;

        public static GameObject? sidebar;
        public static TextMeshProUGUI? sidebarHeaderText;
        public static TextMeshProUGUI? sidebarBodyText;
        public static Sprite? KnifeIcon;
        public static Sprite? LethalMysteryLogo;
        public static Sprite? LethalMysteryBanner;

        private void Awake()
        {

            PatchAllStuff();
            Roles.AppendRoles();

            PrefixSetting = instance?.Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            SpriteLoader();
            Keybinds.InitControls();
        }


        private void PatchAllStuff()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        public static void ResetVariables()
        {
            ButlerEnemyAIPatch.spawnedButlerForKnife = false;
            NutcrackerEnemyAIPatch.spawnedNutForWeapon = false;
            PlayerControllerBPatch.checkedForWeapon = false;
            CharacterDisplay.doneSpawningWeapons = false;
        }


        public static void RemoveEnvironment(bool value = true)
        {
            // Environment
            GameObject.Find("OutOfBoundsTerrain")?.gameObject?.SetActive(!value);
            Scene currentScene = SceneManager.GetSceneAt(1); // might not work on other moons. try getting the current scene

            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                if (obj.name == "Environment")
                {
                    obj.SetActive(!value);
                    break;
                }
            }
        }

        public static void DespawnEnemies()
        {
            Scene SampleScene = SceneManager.GetSceneAt(0);
            foreach (GameObject obj in SampleScene.GetRootGameObjects())
            {
                if (obj.name.Contains("Nutcracker"))
                {
                    Plugin.mls.LogInfo($"< Finding nuts \n-------");
                    Destroy(obj.gameObject);
                }
                if (obj.name.Contains("Butler"))
                {
                    Plugin.mls.LogInfo($"< Finding Butlers");
                    Destroy(obj.gameObject);
                }
            }
        }

        private static void SpriteLoader()
        {

            string MainDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
            string BundleDir = MainDir + "\\Assets\\Assetbundles\\items";

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            Texture2D KnifeTexture = myBundle.LoadAsset<Texture2D>("sprite_knife.png");
            Texture2D LogoTexture = myBundle.LoadAsset<Texture2D>("logo_a.png");
            Texture2D BannerTexture = myBundle.LoadAsset<Texture2D>("default banner.png");
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
        }


    }
}