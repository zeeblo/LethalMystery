using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Network;
using LethalMystery.Players;
using LethalNetworkAPI;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BepInEx.BepInDependency;



namespace LethalMystery
{

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID, DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "LethalMystery.zeeblo.dev";
        private const string modName = "zeeblo.LethalMystery";
        private const string modVersion = "0.1.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? instance;
        public static string MainDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        public static ConfigEntry<string>? PrefixSetting;
        public static ConfigEntry<string>? shapeshiftBind;
        public static List<ConfigEntry<string>> AllHotkeys = new List<ConfigEntry<string>>();


        internal static SelectableLevel? currentLevel;
        internal static EnemyVent[]? currentLevelVents;
        internal static RoundManager? currentRound;

        internal static bool isHost;
        public static GameObject shipInstance;


        //public static bool inMeeting = false;
        public static float defaultMeetingCountdown = 20f;
        //public static float currentMeetingCountdown = defaultMeetingCountdown;
        public static float defaultMeetingCooldown = 10f;
        //public static float MeetingCooldown = defaultMeetingCooldown;
        public static int defaultMeetingNum = 1;
        public static int MeetingNum = defaultMeetingNum;
        //public static bool inGracePeriod = false;
        public static float defaultGracePeriodCountdown = 80f;
        //public static float currentGracePeriodCountdown = defaultGracePeriodCountdown;
        public static Dictionary<ulong, string> localPlayerRoles = new Dictionary<ulong, string>();

        public static LNetworkVariable<string> inMeeting = LNetworkVariable<string>.Connect("inMeeting");
        public static LNetworkVariable<string> inGracePeriod = LNetworkVariable<string>.Connect("inGracePeriod");
        public static LNetworkVariable<string> currentGracePeriodCountdown = LNetworkVariable<string>.Connect("currentGracePeriodCountdown");
        public static LNetworkVariable<string> currentMeetingCountdown = LNetworkVariable<string>.Connect("currentMeetingCountdown");
        public static LNetworkVariable<string> MeetingCooldown = LNetworkVariable<string>.Connect("MeetingCooldown");

        public static GameObject? sidebar;
        public static TextMeshProUGUI? sidebarHeaderText;
        public static TextMeshProUGUI? sidebarBodyText;
        public static Sprite? KnifeIcon;
        public static Sprite? LethalMysteryLogo;
        public static Sprite? LethalMysteryBanner;
        public static NetHandler netHandler { get; set; }
        public static PlayerControllerB localPlayer;



        private void Awake()
        {
            netHandler = new NetHandler();
            PrefixSetting = Config.Bind("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            shapeshiftBind = Config.Bind("Gameplay Controls", "Shapeshift", "8", "Disguise yourself as another user");
            AllHotkeys.Add(PrefixSetting);
            AllHotkeys.Add(shapeshiftBind);

            PatchAllStuff();
            SpriteLoader();
            Roles.AppendRoles();
            Controls.InitControls();


        }



        private void PatchAllStuff()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        public static void ResetVariables()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.loadingText.enabled = false;
            }

            MeetingDefaults(); //called in Start.cs instead
            Roles.ResetVariables();
            CharacterDisplay.ResetVariables();
            Tasks.ResetVariables();
            AutoGiveItem.ResetVariables();
        }

        public static void MeetingDefaults()
        {
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {MeetingNum}";
            if (!isHost) return;
            Plugin.mls.LogInfo(">>> b4inMeetingVal:");
            
            Plugin.currentGracePeriodCountdown.Value = $"{Plugin.defaultGracePeriodCountdown}";
            inMeeting.Value = "false";
            Plugin.mls.LogInfo($">>> inMeetingVal: {inMeeting.Value}");
            currentMeetingCountdown.Value = $"{defaultMeetingCountdown}";
            MeetingCooldown.Value = $"{defaultMeetingCooldown}";
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
            if (!Plugin.isHost) return;

            Scene SampleScene = SceneManager.GetSceneAt(0);
            foreach (GameObject obj in SampleScene.GetRootGameObjects())
            {
                if (obj.name.Contains("Nutcracker"))
                {
                    Destroy(obj.gameObject);
                }
                if (obj.name.Contains("Butler"))
                {
                    Destroy(obj.gameObject);
                }
            }
        }

        private static void SpriteLoader()
        {
            string BundleDir = MainDir + "\\Assets\\Assetbundles\\items";

            AssetBundle myBundle = AssetBundle.LoadFromFile(BundleDir);
            Texture2D KnifeTexture = myBundle.LoadAsset<Texture2D>("sprite_knife.png");
            Texture2D LogoTexture = myBundle.LoadAsset<Texture2D>("logo_a.png");
            Texture2D BannerTexture = myBundle.LoadAsset<Texture2D>("default_banner.jpg");
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