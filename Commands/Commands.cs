using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using LethalMystery.Utils;
using LethalMystery.Maps;
using LethalMystery.Players;
using System.Collections.Generic;
using System.Linq;


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
                case "clear":
                    Commands.ClearChat();
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
                case "set":
                    SetCommands(commandarguments, commandarguments.Length);
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
            msgbody = Data.GetInfo("help");
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }
        public static string GetHostHelp()
        {
            msgtitle = "Host Commands:";
            msgbody = Data.GetInfo("hosthelp");
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




        private static void SetCommands(string[] command, int args)
        {
            switch (args)
            {
                case 1:
                    DisplayChatMessage("You need to specify a set command.");
                    break;
                case 3:
                    SetRoleAmount(role: command[1], num: command[2]);
                    break;
            }
        }



        public static void SetRoleAmount(string role, string num)
        {
            if (Roles.specialRoleAmount.ContainsKey(role) == false)
            {
                DisplayChatError("Invalid Command");
                Plugin.mls.LogInfo($">>> role: {role} | num: {num} ");
                return;
            }
            int raw_amount = StringAddons.ConvertToInt(num);
            int amount = (raw_amount == 0 || raw_amount == 1 || raw_amount == 2) ? raw_amount : 1;

            Roles.specialRoleAmount[role] = amount;

            DisplayChatMessage($"Set {role} amount to {amount}");
        }




        public static void ClearChat()
        {
            HUDManager.Instance.ChatMessageHistory.Clear();
            UpdateChatText();
        }




        #endregion Chat Commands






        public static void SpawnScrapFunc(string toSpawn, bool toInventory = false)
        {

            Vector3 position = Vector3.zero;
            position = CalculateSpawnPosition();

            if (Plugin.currentRound != null)
            {
                int len = Plugin.currentRound.currentLevel.spawnableScrap.Count();

                bool spawnable = false;
                for (int i = 0; i < len; i++)
                {
                    Item scrap = Plugin.currentRound.currentLevel.spawnableScrap[i].spawnableItem;
                    if (scrap.spawnPrefab.name.ToLower().Contains(toSpawn.ToLower()))
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
                    //Plugin.mls.LogInfo("Could not spawn " + toSpawn);
                }
            }

        }


        private static Vector3 CalculateSpawnPosition()
        {
            Vector3 position = Vector3.zero;
            System.Random randomNum = new System.Random();
            int index = randomNum.Next(0, InsideMap.scrapLocations.ToArray().Length);
            position = InsideMap.scrapLocations[index];

            //Plugin.mls.LogInfo($">>> Scrap Position: {position}");
            return position;
        }


        public static void SpawnWeapons(string name)
        {
            Vector3 position = new Vector3(12f, -60f, 15f);
            SelectableLevel currentLevel = RoundManager.Instance.playersManager.levels[6]; // "6" (rend) is one of the moons a butler will spawn on

            if (name.ToLower().Contains("knife") || name == "all")
            {
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[11].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
            if (name.ToLower().Contains("shotgun") || name == "all")
            {
                //position = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/spawn_pos").transform.position;
                //position = InsideMap.lll_pos;

                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[9].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
        }


        public static void SpawnBaby()
        {
            Vector3 position = new Vector3(Plugin.localPlayer.transform.position.x, Plugin.localPlayer.transform.position.y + 4, Plugin.localPlayer.transform.position.z);
            SelectableLevel[] currentLevel = RoundManager.Instance.playersManager.levels;

            for (int i = 0; i < currentLevel.Length; i++)
            {
                foreach (SpawnableEnemyWithRarity v in currentLevel[i].Enemies)
                {
                    if (v.enemyType.enemyName.Contains("Maneater"))
                    {
                        UnityEngine.Object.Instantiate<GameObject>(v.enemyType.enemyPrefab, position, Quaternion.identity).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                    }
                }
            }

        }






    }
}