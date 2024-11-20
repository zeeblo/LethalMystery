using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using LethalMystery.Players;
using LethalMystery.Utils;


namespace LethalMystery
{
    public class Commands
    {

        public static readonly Dictionary<string, string> HelpCmds = new Dictionary<string, string>()
        {
            {
                "vote", "/vote playerID - use in a meeting to vote a specific user. To see everyone's playerID type /ids \n /vote skip - use in a meeting to skip votting"
            }
        };

        public static GrabbableObject? randomObject;
        public static ShotgunItem? gunObject;


        #region Chat Commands
        internal static string? msgtitle;
        internal static string? msgbody;
        internal static string NetCommandPrefix = "<size=0>LCMD:";
        internal static string NetHostCommandPrefix = "<size=0>LMCMD:";
        internal static string NetCommandPostfix = "</size>";
        internal static string? playerwhocalled;


        internal static bool NonHostCommands(string command)
        {
            bool IsNonHostCommand = true;
            string[] commandargs = command.Split(' ');
            int numOfInputs = commandargs.Length;

            if (numOfInputs > 1)
            {
                foreach (var cmd in Commands.HelpCmds)
                {
                    // user typed "/help (command name)"
                    if (command.StartsWith("help") && commandargs[1].ToLower().Contains(cmd.Key))
                    {

                        msgtitle = cmd.Key;
                        msgbody = cmd.Value;
                        DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);

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

            if (!Plugin.isHost)
            {
                msgtitle = "Command";
                msgbody = "Unable to send command since you are not host.";
                return;
            }

            switch (commandarguments[0])
            {
                case "set imps":
                    break;
            }
        }


        internal static void SendHostCommand(string commandInput)
        {
            if (!Plugin.isHost)
            {
                return;
            }
            string commandToClients = NetHostCommandPrefix + commandInput + NetCommandPostfix;
            HUDManager.Instance.AddTextToChatOnServer(commandToClients, -1);
        }

        public static void ProcessNetHostCommand(string commandInput)
        {
        }
        public static void ProcessCommand(string commandInput)
        {
            ProcessCommandInput(commandInput);
        }




        public static void DisplayChatMessage(string chatMessage)
        {
            string formattedMessage =
                $"<color=#FFFF00>{chatMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }
        public static void DisplayChatError(string errorMessage)
        {
            string formattedMessage =
                $"<color=#FF0000>ERROR</color>: <color=#FF0000>{errorMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }

        private static void UpdateChatText()
        {
            HUDManager.Instance.chatText.text = string.Join("\n", HUDManager.Instance.ChatMessageHistory);
        }










        public static string GetHelp()
        {
            msgtitle = "Available Commands:";
            msgbody = "/help vote - Info on how to vote a user out \n /role - See what your role is \n /hosthelp - see host only commands \n /ids - view everyone's playerID";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }
        public static string GetHostHelp()
        {
            msgtitle = "Host Commands:";
            msgbody = "/set tasks - set the number of tasks (default is 10) \n /set imps - set the number of imposters (default 1)";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string GetRole()
        {
            msgtitle = "Role:";
            msgbody = "You currently have no role.";

            if (Players.Roles.CurrentRole != null)
            {
                msgbody = Players.Roles.CurrentRole.Name + ": " + Players.Roles.CurrentRole.Desc;
            }

            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string GetPlayerIDs()
        {

            string name = "";
            PlayerControllerB player;

            foreach (KeyValuePair<ulong, int> clientPlayer in StartOfRound.Instance.ClientPlayerList)
            {
                player = StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientPlayer.Key]];
                name += $"{player.playerUsername} || ID: {player.playerClientId}\n";
            }

            msgtitle = "IDs:";
            msgbody = name;
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string SetVote(string vote)
        {

            msgtitle = "Vote:";

            if (GameNetworkManager.Instance.localPlayerController.playersManager.inShipPhase) // temporary, add check for when in meeting instead
            {
                msgbody = "You can only vote in a meeting.";
            }
            else if (vote.ToLower() == "skip")
            {
                msgbody = "You voted to skip!";
            }
            else
            {
                ulong playerID;

                if (!ulong.TryParse(vote, out playerID) || !StartOfRound.Instance.ClientPlayerList.Keys.Contains(playerID))
                {
                    msgbody = "Invalid. Type \"/ids\" to see the available IDs \n Type \"/help vote\" to properly use this command";
                    DisplayChatError("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
                    return msgbody + "/" + msgtitle;
                }

                playerID = ulong.Parse(vote);
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.playerClientId == playerID)
                    {
                        msgbody = "You voted " + player.playerUsername + " (ID: " + playerID + ")";
                    }
                }
            }

            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }






        #endregion Chat Commands






        public static void SpawnScrapFunc(string toSpawn, string location, bool toInventory = false)
        {

            Vector3 position = Vector3.zero;

            Plugin.mls.LogInfo(">>> ABOVE CALC <<<");
            if (location != "vnt" || location != "btn")
            {
                position = CalculateSpawnPosition(playerID: location);
            }

            Plugin.mls.LogInfo(">>> BELOW CALC <<<");

            if (Plugin.currentRound != null)
            {
                int len = Plugin.currentRound.currentLevel.spawnableScrap.Count();

                Plugin.mls.LogInfo(">>> @ LEN THING <<<");
                bool spawnable = false;
                for (int i = 0; i < len; i++)
                {
                    Item scrap = Plugin.currentRound.currentLevel.spawnableScrap[i].spawnableItem;
                    Plugin.mls.LogInfo($">< Scrap name: {scrap} ");
                    if (scrap.spawnPrefab.name.ToLower().Contains(toSpawn))
                    {

                        GameObject objToSpawn = scrap.spawnPrefab;
                        GameObject gameObject = UnityEngine.Object.Instantiate(objToSpawn, position, Quaternion.identity, Plugin.currentRound.spawnedScrapContainer);

                        GrabbableObject component = gameObject.GetComponent<GrabbableObject>();

                        component.startFallingPosition = position;
                        component.targetFloorPosition = component.GetItemFloorPosition(position);
                        component.SetScrapValue(10);
                        component.NetworkObject.Spawn();
                        spawnable = true;

                        if (toInventory)
                        {
                            randomObject = component;
                        }

                        break;
                    }
                }
                if (!spawnable)
                {
                    Plugin.mls.LogInfo("Could not spawn " + toSpawn);
                }
            }

        }



        public static void SpawnWeapons(string name)
        {
            Vector3 position = new Vector3(12f, -60f, 15f);
            SelectableLevel currentLevel = RoundManager.Instance.playersManager.levels[6]; // "6" (rend) is the moon butlers will spawn on

            if (name.ToLower().Contains("knife") || name == "all")
            {
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[11].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
            if (name.ToLower().Contains("shotgun") || name == "all")
            {
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[9].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
        }



        private static Vector3 CalculateSpawnPosition(string playerID, string place = "none")
        {
            Vector3 position = Vector3.zero;
            if (place == "skeldmap")
            {
                Plugin.mls.LogInfo(">>> REACHED SKELD THING<<<");

                System.Random randomNum = new System.Random();
                //int index = randomNum.Next(0, scrapLocations.ToArray().Length);
                //position = scrapLocations[index];


            }
            else
            {
                Plugin.mls.LogInfo(">>> REACHED @PLAYER THING<<<");
                PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                {
                    Plugin.mls.LogInfo($"Checking Playername: {testedPlayer.playerUsername.ToLower()} || {playerID}");
                    if ($"{testedPlayer.playerClientId}" == playerID)
                    {
                        Plugin.mls.LogInfo($"Found player {testedPlayer.playerUsername}");
                        position = testedPlayer.transform.position;

                        break;
                    }
                }
            }

            return position;
        }


    }
}