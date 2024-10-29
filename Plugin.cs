using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Patches;
using LethalMystery.Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




/*
 * - Add Start command
 * ! Have a GUI that says if they're the monster/crew & if they're shapeshifter (Stays on whole game)
 * - Have the speaker dialouge tell players their role instead.
 * - Have a chat commands that will tell the users their role & what they do
 * - Imposters can spawn their weapon (Using config not Keyboard)
 * - 
*/




/*
 * Win Game Mechanics
 * - Top text will say (Monsters/Employees won).
 * - If monsters won the bottom text will say the names of the 1 or 2 imps
*/


namespace LethalMystery
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "zeeblo.LethalMystery";
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

        private void Awake()
        {

            PatchAllStuff();
            Roles.AppendRoles();

            PrefixSetting = instance?.Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");

        }


        private void PatchAllStuff()
        {
            _harmony.PatchAll(typeof(ButlerEnemyAIPatch));
            _harmony.PatchAll(typeof(HUDManagerPatch));
            _harmony.PatchAll(typeof(NutcrackerEnemyAIPatch));
            _harmony.PatchAll(typeof(PlayerControllerBPatch));
            _harmony.PatchAll(typeof(ShotgunItemPatch));
            _harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(TimeOfDayPatch));
            _harmony.PatchAll(typeof(UnlockableSuitPatch));
            _harmony.PatchAll(typeof(GameMech.MainGame.CheckPlayerAmount));
            _harmony.PatchAll(typeof(GameMech.MainGame.StartGame));
            _harmony.PatchAll(typeof(GameMech.MainGame.Meeting));
            _harmony.PatchAll(typeof(GameMech.MainGame.ControlDoors));
            _harmony.PatchAll(typeof(GameMech.Tasks.Items));
            _harmony.PatchAll(typeof(GameMech.Tasks.Assignment));
            _harmony.PatchAll(typeof(GameMech.Tests.AdminCMDS));
            _harmony.PatchAll(typeof(GameMech.Tests.AdminCMDS_2));
        }


        public static void ResetVariables()
        {
            ButlerEnemyAIPatch.spawnedButlerForKnife = false;
            NutcrackerEnemyAIPatch.spawnedNutForWeapon = false;
            PlayerControllerBPatch.checkedForWeapon = false;
            StartOfRoundPatch.doneSpawningWeapons = false;
        }


        public static void RemoveEnvironment(bool view = false)
        {
            // Environment 
            GameObject OutOfBoundsTerrain = GameObject.Find("OutOfBoundsTerrain").gameObject;
            Scene currentScene = SceneManager.GetSceneAt(1); // might not work on other moons. try getting the current scene

            OutOfBoundsTerrain.SetActive(view);

            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                if (obj.name == "Environment")
                {
                    obj.SetActive(view);
                    break;
                }
            }

        }

        public static void DespawnEnemies()
        {
            Scene currentScene = SceneManager.GetSceneAt(0);
            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                if (obj.name.Contains("Nutcracker"))
                {
                    Plugin.mls.LogInfo($"< Finding nuts");
                    Destroy(obj.gameObject);
                }
                if (obj.name.Contains("Butler"))
                {
                    Plugin.mls.LogInfo($"< Finding Butlers");
                    Destroy(obj.gameObject);
                }
            }
        }


    }
}