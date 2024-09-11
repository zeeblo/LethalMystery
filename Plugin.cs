using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Patches;
using Unity.Netcode;
using UnityEngine;




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
            //_harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(RoundManagerPatch));
            _harmony.PatchAll(typeof(StartMatchLeverPatch));
            _harmony.PatchAll(typeof(UnlockableSuitPatch));
            _harmony.PatchAll(typeof(HUDManagerPatch));
        }





        #region Chat Commands


        internal static bool NonHostCommands(string command)
        {
            bool IsNonHostCommand = true;
            string[] commandargs = command.Split(' ');
            int numOfInputs = commandargs.Length;


            Plugin.mls.LogInfo($"num: {numOfInputs} | cmd: {command} ");

            if (numOfInputs > 1)
            {
                foreach (var title in Commands.HelpCmds)
                {

                    if (command.StartsWith("help") && commandargs[1].ToLower().Contains(title.Key))
                    {
                        foreach (var body in title.Value)
                        {
                            Plugin.msgtitle = title.Key;
                            Plugin.msgbody = body.Value;
                            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
                        }
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