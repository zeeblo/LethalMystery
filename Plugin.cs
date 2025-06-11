using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Maps;
using LethalMystery.Maps.Sabotages;
using LethalMystery.Network;
using LethalMystery.Patches;
using LethalMystery.Players;
using LethalMystery.Players.Abilities;
using LethalMystery.UI;
using LethalMystery.Utils;
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BepInEx.BepInDependency;
using static LethalMystery.Players.Controls;



namespace LethalMystery
{

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID, DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", DependencyFlags.HardDependency)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "LethalMystery.zeeblo.dev";
        public const string modName = "zeeblo.LethalMystery";
        public const string modVersion = "0.4.5";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? instance;
        public static string MainDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        public static bool inTestMode = true;
        internal static SelectableLevel? currentLevel;
        internal static EnemyVent[]? currentLevelVents;
        internal static RoundManager? currentRound;

        internal static bool isHost;
        public static GameObject shipInstance;

        public static GameObject? sidebar;
        public static TextMeshProUGUI? sidebarHeaderText;
        public static TextMeshProUGUI? sidebarBodyText;
        public static NetHandler netHandler { get; set; }
        public static PlayerControllerB localPlayer;
        public static Terminal terminal;
        public static EnemyVent enemyVent;

        public static string firedText = "Ejected";
        public static string firedTextSub = "";
        public static ulong localID;

        public static PlayerBinds actionInstance;


        private void Awake()
        {
            netHandler = new NetHandler();
            actionInstance = new PlayerBinds();

            LMConfig.AllConfigs(Config);
            PatchAllStuff();
            LMAssets.LoadAllAssets();
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

            if (VotingUI.votingGUI != null)
            {
                Destroy(VotingUI.votingGUI.gameObject);
            }

            UnlockableSuitPatch.ResetVars();
            Meeting.MeetingDefaults();
            Roles.ResetVariables();
            CharacterDisplay.ResetVariables();
            Tasks.ResetVariables();
            AutoGiveItem.ResetVariables();
            EjectPlayers.ResetVars();
            Ability.ResetVars();
            Controls.ResetVars();
            Start.ResetVars();
            Sabotage.ResetVars();
            InsideMap.ResetVars();
        }




        public static void RemoveEnvironment(bool value = true)
        {
            // Environment
            GameObject.Find("OutOfBoundsTerrain")?.gameObject?.SetActive(!value);
            Scene currentScene = SceneManager.GetSceneAt(1);

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



        // thanks to https://github.com/BepInEx/BepInEx/discussions/320#discussioncomment-1495776
        public static bool FoundThisMod(string modID)
        {
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (metadata.GUID.Equals(modID))
                {
                    return true;
                }
            }

            return false;
        }


        public static string GetAssetPath(string file, string parent = "Assetbundles")
        {
            string firstPath = Plugin.MainDir + $"\\Assets\\{parent}\\{file}";
            string secondPath = Plugin.MainDir + $"/{file}";

            if (File.Exists(firstPath))
            {
                return firstPath;
            }
            return secondPath;
        }

    }
}