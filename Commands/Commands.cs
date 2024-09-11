using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Unity.Netcode;
using TMPro;
using LethalMystery.Patches;
using HarmonyLib.Tools;
using System.Linq;
using static Steamworks.InventoryItem;
using System.Security;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Collections;
using UnityEngine.AI;
using System.Runtime.CompilerServices;
using Unity.Services.Authentication.Internal;

namespace LethalMystery
{
    public class Commands
    {

        public static readonly Dictionary<string, Dictionary<string, string>> HelpCmds = new Dictionary<string, Dictionary<string, string>>
        {
            {"vote", new Dictionary<string, string>
                {
                    {"title", "vote"},
                    {"body", "/vote playerID - use in a meeting to vote a specific user. To see everyone's playerID type /ids \n /vote skip - use in a meeting to skip votting"},
                }
            }
        };


        internal static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside, Vector3 location)
        {
            if (!Plugin.isHost)
            {
                return;
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f && inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        if (Plugin.currentLevel != null)
                        {
                            Plugin.currentRound?.SpawnEnemyOnServer(location, 0.0f, Plugin.currentLevel.Enemies.IndexOf(enemy));
                        }
                    }
                    return;
                }
                catch
                {
                    Plugin.mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f && !inside)
            {
                try
                {
                    int i = 0;
                    for (; i < amount; i++)
                    {
                        if (Plugin.currentLevel != null)
                        {
                            UnityEngine.Object.Instantiate<GameObject>(Plugin.currentLevel.OutsideEnemies[Plugin.currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, location, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                        }
                    }
                    Plugin.mls.LogInfo($"You wanted to spawn: {amount} enemies");
                    Plugin.mls.LogInfo("Spawned an enemy. Total Spawned: " + i + "at position:" + location);
                    return;
                }
                catch
                {
                    Plugin.mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (inside)
            {
                try
                {
                    int i = 0;
                    for (; i < amount; i++)
                    {
                        if (Plugin.currentLevel != null)
                        {
                            Plugin.currentRound?.SpawnEnemyOnServer(Plugin.currentRound.allEnemyVents[UnityEngine.Random.Range(0, Plugin.currentRound.allEnemyVents.Length)].floorNode.position, Plugin.currentRound.allEnemyVents[i].floorNode.eulerAngles.y, Plugin.currentLevel.Enemies.IndexOf(enemy));
                        }

                    }
                    Plugin.mls.LogInfo($"You wanted to spawn: {amount} enemies");
                    Plugin.mls.LogInfo(("Total Spawned: " + i));
                    return;
                }
                catch
                {
                    Plugin.mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            int j = 0;
            for (; j < amount; j++)
            {
                if (Plugin.currentLevel != null)
                {
                    UnityEngine.Object.Instantiate<GameObject>(Plugin.currentLevel.OutsideEnemies[Plugin.currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                }
            }
            Plugin.mls.LogInfo($"You wanted to spawn: {amount} enemies");
            Plugin.mls.LogInfo(("Total Spawned: " + j));
        }



        public static string SpawnEnemyFunc(string text)
        {

            Plugin.msgtitle = "Spawned Enemies";
            string[] array = text.Split(' ');
            if (Plugin.currentLevel == null || Plugin.levelEnemySpawns == null || Plugin.currentLevel.Enemies == null)
            {
                Plugin.msgtitle = "Command";
                Plugin.msgbody = (Plugin.currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                Plugin.DisplayChatError(Plugin.msgtitle + "\n" + Plugin.msgbody);
                return Plugin.msgbody + "/" + Plugin.msgtitle;
            }
            if (array.Length < 2)
            {
                Plugin.msgtitle = "Command Error";
                Plugin.msgbody = "Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})";
                Plugin.DisplayChatError(Plugin.msgtitle + "\n" + Plugin.msgbody);
                Plugin.mls.LogWarning("Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})");
                return Plugin.msgbody + "/" + Plugin.msgtitle;
            }
            int amount = 1;
            string vstate = "alive";
            Vector3 position = Vector3.zero;
            string sposition = "random";
            var args = array.Skip(2);

            foreach (string arg in args)
            {
                string[] darg = arg.Split('=');
                switch (darg[0])
                {
                    case "a":
                    case "amount":
                        amount = int.Parse(darg[1]);
                        Plugin.mls.LogInfo($"{amount}");
                        break;
                    case "s":
                    case "state":
                        vstate = darg[1];
                        Plugin.mls.LogInfo(vstate);
                        break;
                    case "p":
                    case "position":
                        sposition = darg[1];
                        Plugin.mls.LogInfo(sposition);
                        break;
                    default:
                        break;
                }
            }

            if (sposition != "random")
            {
                position = CalculateSpawnPosition(sposition);
                if (position == Vector3.zero && sposition != "random")
                {
                    Plugin.mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
            }

            if (array.Length > 1)
            {
                bool flag = false;
                string enemyName = "";
                foreach (SpawnableEnemyWithRarity enemy in Plugin.currentLevel.Enemies)
                {
                    if (enemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                    {
                        try
                        {
                            flag = true;
                            enemyName = enemy.enemyType.enemyName;
                            if (sposition == "random")
                            {
                                SpawnEnemy(enemy, amount, inside: true, location: new Vector3(0f, 0f, 0f));
                            }
                            else
                            {
                                SpawnEnemy(enemy, amount, inside: true, location: position);
                            }
                            Plugin.mls.LogInfo((object)("Spawned " + enemy.enemyType.enemyName));
                        }
                        catch
                        {
                            Plugin.mls.LogInfo((object)"Could not spawn enemy");
                        }
                        Plugin.msgbody = "Spawned: " + enemyName;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (SpawnableEnemyWithRarity outsideEnemy in Plugin.currentLevel.OutsideEnemies)
                    {
                        if (outsideEnemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                        {
                            try
                            {
                                flag = true;
                                enemyName = outsideEnemy.enemyType.enemyName;
                                Plugin.mls.LogInfo(outsideEnemy.enemyType.enemyName);
                                Plugin.mls.LogInfo(("The index of " + outsideEnemy.enemyType.enemyName + " is " + Plugin.currentLevel.OutsideEnemies.IndexOf(outsideEnemy)));
                                if (sposition == "random")
                                {
                                    SpawnEnemy(outsideEnemy, amount, inside: false, location: new Vector3(0f, 0f, 0f));
                                }
                                else
                                {
                                    SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
                                }
                                Plugin.mls.LogInfo(("Spawned " + outsideEnemy.enemyType.enemyName));
                            }
                            catch (Exception ex)
                            {
                                Plugin.mls.LogInfo("Could not spawn enemy");
                                Plugin.mls.LogInfo(("The game tossed an error: " + ex.Message));
                            }
                            Plugin.msgbody = "Spawned " + amount + " " + enemyName + (amount > 1 ? "s" : "");
                            break;
                        }
                    }
                }
            }
            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }


        public static bool ToggleGodMode()
        {
            Plugin.enableGod = !Plugin.enableGod;
            return Plugin.enableGod;
        }



        public static void SpawnScrapFunc(string toSpawn, string name)
        {

            Vector3 position = Vector3.zero;

            Plugin.mls.LogInfo(">>> ABOVE CALC <<<");
            if (name != "vnt" || name != "btn")
            {
                position = CalculateSpawnPosition(name);
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
                    if (scrap.spawnPrefab.name.ToLower().Contains(toSpawn))
                    {

                        GameObject objToSpawn = scrap.spawnPrefab;
                        GameObject gameObject = UnityEngine.Object.Instantiate(objToSpawn, position, Quaternion.identity, Plugin.currentRound.spawnedScrapContainer);
                        GrabbableObject component = gameObject.GetComponent<GrabbableObject>();

                        component.startFallingPosition = position;
                        component.targetFloorPosition = component.GetItemFloorPosition(position);
                        component.SetScrapValue(10); // Set Scrap Value
                        component.NetworkObject.Spawn();
                        spawnable = true;

                        break;
                    }
                }
                if (!spawnable)
                {
                    Plugin.mls.LogInfo("Could not spawn " + toSpawn);
                }
            }

        }


        public static string GetHelp()
        {
            Plugin.msgtitle = "Available Commands";
            Plugin.msgbody = "/help vote - Info on how to vote a user out \n /role - See what your role is \n /hosthelp - see host only commands \n /ids - view everyone's playerID";
            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }
        public static string GetHostHelp()
        {
            Plugin.msgtitle = "Host Commands";
            Plugin.msgbody = "/set tasks - set the number of tasks (default is 10) \n /set imps - set the number of imposters (default 1)";
            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }

        public static string GetRole()
        {
            Plugin.msgtitle = "Role";
            Plugin.msgbody = "You currently have no role.";

            if (Players.Roles.CurrentRole != "")
            {
                Plugin.msgbody = Players.Roles.CurrentRole + ": " + Players.Roles.RoleAttrs[Players.Roles.CurrentRole]["desc"];
            }

            
            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
            return Plugin.msgbody + "/" + Plugin.msgtitle;
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

            Plugin.msgtitle = "IDs";
            Plugin.msgbody = name;
            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }

        public static string SetVote(string vote)
        {
            
            Plugin.msgtitle = "vote";

            if (GameNetworkManager.Instance.localPlayerController.playersManager.inShipPhase) // temporary, add check for when in meeting instead
            {
                Plugin.msgbody = "You can only vote in a meeting.";
            }
            else if (vote.ToLower() == "skip")
            {
                Plugin.msgbody = "You voted to skip!";
            }
            else
            {
                ulong playerID;

                if ( !ulong.TryParse(vote, out playerID) || !StartOfRound.Instance.ClientPlayerList.Keys.Contains(playerID))
                {
                    Plugin.msgbody = "Invalid. Type \"/ids\" to see the available IDs \n Type \"/help vote\" to properly use this command";
                    Plugin.DisplayChatError("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
                    return Plugin.msgbody + "/" + Plugin.msgtitle;
                }

                playerID = ulong.Parse(vote);
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.playerClientId == playerID)
                    {
                        Plugin.msgbody = "You voted " + player.playerUsername + " (ID: " + playerID + ")";
                    }
                }
            }

            Plugin.DisplayChatMessage("<color=#FF00FF>" + Plugin.msgtitle + "</color>\n" + Plugin.msgbody);
            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }

        public static bool CheckPrefix(string text)
        {
            string prefix = "/";

            if (Plugin.PrefixSetting?.Value != "" && Plugin.PrefixSetting != null)
            {
                prefix = Plugin.PrefixSetting.Value;
            }
            if (!text.ToLower().StartsWith(prefix.ToLower()))
            {
                return false;
            }
            return true;
        }

        private static Vector3 CalculateSpawnPosition(string username)
        {
            Vector3 position = Vector3.zero;
            if (username == "skeldmap")
            {
                Plugin.mls.LogInfo(">>> REACHED SKELD THING<<<");

                System.Random randomNum = new System.Random();
                //int index = randomNum.Next(0, scrapLocations.ToArray().Length);
                //position = scrapLocations[index];


            }
            else
            {
                //username = username.Replace("player (1)", "player #1").Replace("player (2)", "player #2").Replace("player (3)", "player #3").Replace("player (4)", "player #4").Replace("player (5)", "player #5").Replace("player (6)", "player #6").Replace("player (7)", "player #7").Replace("player (8)", "player #8").Replace("player (9)", "player #9");
                Plugin.mls.LogInfo(">>> REACHED @PLAYER THING<<<");
                PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                {
                    Plugin.mls.LogInfo($"Checking Playername: {testedPlayer.playerUsername.ToLower()} || {username}");
                    if ($"{testedPlayer.playerClientId}" == username)
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