using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Patches;
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


        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>? levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int>? enemyRaritys;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve>? enemyPropCurves;
        internal static SelectableLevel? currentLevel;
        internal static EnemyVent[]? currentLevelVents;
        internal static RoundManager? currentRound;
        internal static bool EnableInfiniteAmmo = false;
        internal static ConfigEntry<string>? PrefixSetting;
        internal static bool enableGod;
        internal static bool EnableInfiniteCredits = false;
        internal static int CustomDeadline = int.MinValue;
        internal static bool usingTerminal = false;
        internal static PlayerControllerB? playerRef;
        internal static bool isHost;
        internal static string? msgtitle;
        internal static string? msgbody;
        internal static string NetCommandPrefix = "<size=0>LCMD:";
        internal static string NetHostCommandPrefix = "<size=0>LMCMD:";
        internal static string NetCommandPostfix = "</size>";
        internal static string? playerwhocalled;

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

            PrefixSetting = instance?.Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            enableGod = false;
        }


        private void PatchAllStuff()
        {
            _harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(RoundManagerPatch));
            _harmony.PatchAll(typeof(StartMatchLeverPatch));
            _harmony.PatchAll(typeof(UnlockableSuitPatch));
            _harmony.PatchAll(typeof(HUDManagerPatch));
            _harmony.PatchAll(typeof(TerminalPatch));
            _harmony.PatchAll(typeof(ShipAlarmCordPatch));
            _harmony.PatchAll(typeof(HangarShipDoorPatch));
            _harmony.PatchAll(typeof(TimeOfDayPatch));
        }


        public static void RemoveEnvironment(bool view = false)
        {
            // Environment 
            GameObject OutOfBoundsTerrain = GameObject.Find("OutOfBoundsTerrain").gameObject;
            Scene currentScene = SceneManager.GetSceneAt(1);

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


        #region UI Elements

        public static void CreateSidebar(string body, string header="Meeting")
        {
            GameObject RightCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopRightCorner");
            sidebar = new GameObject("Sidebar");
            sidebar.transform.SetParent(RightCorner.transform, false);

            Image sidebarBackground = sidebar.AddComponent<Image>();
            sidebarBackground.color = Color.yellow;

            RectTransform sidebarRect = sidebar.GetComponent<RectTransform>();
            sidebarRect.sizeDelta = new Vector2(65, 20);
            sidebarRect.localPosition = new Vector2(-70, -120);
            
            GameObject sidebarTextObject = new GameObject("SidebarText");
            sidebarTextObject.transform.SetParent(sidebar.transform, false);

            sidebarHeaderText = sidebarTextObject.AddComponent<TextMeshProUGUI>();
            sidebarHeaderText.color = Color.black;
            sidebarHeaderText.text = $" <color=#FF0000>{header}:</color> {body}";
            sidebarHeaderText.fontWeight = FontWeight.Heavy;
            sidebarHeaderText.alignment = TextAlignmentOptions.Left;
            sidebarHeaderText.fontSize = 8;

            // Make the size and position of the text object relative to the background
            RectTransform textRect = sidebarHeaderText.GetComponent<RectTransform>();
            textRect.sizeDelta = sidebarRect.sizeDelta;
            textRect.anchoredPosition = Vector2.zero;

        }


        public static void UpdateSidebar(string body, string header = "Meeting")
        {
            if (sidebarHeaderText == null)
            {
                CreateSidebar(body, header: header);
            }
            else
            {
                sidebarHeaderText.text = $" <color=#FF0000>{header}:</color> {body}";
                sidebarHeaderText.fontWeight = FontWeight.Heavy;
                sidebarHeaderText.alignment = TextAlignmentOptions.Left;
                sidebarHeaderText.fontSize = 8;
            }

        }

        public static void ShowSidebar(bool show, string body = "", string header = "Meeting")
        {
            if (sidebar != null)
            {
                sidebar.gameObject.SetActive(show);
            }
            else
            {
                CreateSidebar(body, header: header);
            }
        }


        #endregion UI Elements


        #region Chat Commands


        internal static bool NonHostCommands(string command)
        {
            bool IsNonHostCommand = true;
            string[] commandargs = command.Split(' ');
            int numOfInputs = commandargs.Length;


            Plugin.mls.LogInfo($"num: {numOfInputs} | cmd: {command} ");

            if (numOfInputs > 1)
            {
                foreach (var cmd in Commands.HelpCmds)
                {

                    if (command.StartsWith("help") && commandargs[1].ToLower().Contains(cmd.Key))
                    {

                        Plugin.msgtitle = cmd.Key;
                        Plugin.msgbody = cmd.Value;
                        Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);

                        return IsNonHostCommand;
                    }
                }

                switch (commandargs[0])
                {
                    case "vote":
                        Commands.SetVote(commandargs[1]);
                        break;
                    default:
                        IsNonHostCommand = false;
                        break;

                }

                return IsNonHostCommand;
            }


            switch (command)
            {
                case "help":
                    Commands.GetHelp();
                    break;
                case "hosthelp":
                    Commands.GetHostHelp();
                    break;
                case "ids":
                    Commands.GetPlayerIDs();
                    break;
                case "role":
                    Commands.GetRole();
                    break;
                default:
                    IsNonHostCommand = false;
                    break;

            }


            return IsNonHostCommand;
        }

        internal static void ProcessCommandInput(string command)
        {
            msgtitle = "default";
            msgbody = "<color=#FF0000>ERR</color>: unknown";
            string[] commandarguments = command.Split(' ');

            if (NonHostCommands(command))
            {
                return;
            }

            if (!isHost)
            {
                msgtitle = "Command";
                msgbody = "Unable to send command since you are not host.";
                return;
            }

            switch (commandarguments[0])
            {
                case "spawnenemy":
                case "spweny":
                    Commands.SpawnEnemyFunc(command);
                    break;
                case "god":
                    msgtitle = "God Mode";
                    msgbody = "God Mode set to: " + Commands.ToggleGodMode();
                    SendHostCommand(command);
                    break;
                default:
                    msgtitle = "Command";
                    msgbody = "Unknown command: " + commandarguments[0];
                    Plugin.DisplayChatError(msgbody);
                    break;
            }
        }


        internal static void SendHostCommand(string commandInput)
        {
            if (!isHost)
            {
                return;
            }
            string commandToClients = Plugin.NetHostCommandPrefix + commandInput + Plugin.NetCommandPostfix;
            HUDManager.Instance.AddTextToChatOnServer(commandToClients, -1);
        }

        public static void ProcessNetHostCommand(string commandInput)
        {
            if (commandInput.ToLower().Contains("god"))
            {
                enableGod = !enableGod;
                msgtitle = "Host sent command:";
                msgbody = "God Mode set to: " + enableGod;
            }
            if (commandInput.ToLower().Contains("infammo") || commandInput.ToLower().Contains("ammo"))
            {
                EnableInfiniteAmmo = !EnableInfiniteAmmo;
                msgtitle = "Host sent command:";
                msgbody = "Infinite Ammo: " + EnableInfiniteAmmo;
            }

        }
        public static void ProcessCommand(string commandInput)
        {
            Plugin.ProcessCommandInput(commandInput);
        }




        public static void DisplayChatMessage(string chatMessage)
        {
            string formattedMessage =
                $"<color=#FF00FF>LM</color>: <color=#FFFF00>{chatMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }
        public static void DisplayChatError(string errorMessage)
        {
            string formattedMessage =
                $"<color=#FF0000>LM: ERROR</color>: <color=#FF0000>{errorMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }

        private static void UpdateChatText()
        {
            HUDManager.Instance.chatText.text = string.Join("\n", HUDManager.Instance.ChatMessageHistory);
        }


        #endregion Chat Commands

    }
}