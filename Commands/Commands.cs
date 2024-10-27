using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using static Steamworks.InventoryItem;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using System.ComponentModel;


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

        internal static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside, Vector3 location)
        {
            if (location.x != 0f && location.y != 0f && location.z != 0f && inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        if (Plugin.currentLevel != null)
                        {
                            List<SpawnableEnemyWithRarity> EnemyList = RoundManager.Instance.playersManager.levels
                                  .SelectMany(level => level.Enemies).ToList();

                            foreach (SpawnableEnemyWithRarity en in EnemyList)
                            {
                                Plugin.mls.LogInfo($">>> EnemyList: {en.enemyType.enemyName}");
                            }

                            Plugin.currentRound?.SpawnEnemyOnServer(location, 0.0f, EnemyList.IndexOf(enemy));
                        }
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Plugin.mls.LogInfo($">>> first check: {ex}");
                    Plugin.mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f)
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
                    Plugin.mls.LogInfo("second check");
                    Plugin.mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
        }


        public static string SpawnEnemyFunc(string text)
        {
            Plugin.msgtitle = "Spawned Enemies";
            string[] array = text.Split(' ');
            string playerID = array[0];
            string entity = array[1];
            if (Plugin.currentLevel == null || Plugin.levelEnemySpawns == null || Plugin.currentLevel.Enemies == null)
            {
                Plugin.msgtitle = "Command";
                Plugin.msgbody = (Plugin.currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                Plugin.DisplayChatError(Plugin.msgtitle + "\n" + Plugin.msgbody);
                return Plugin.msgbody + "/" + Plugin.msgtitle;
            }
            int amount = 1;
            Vector3 position = Vector3.zero;
            position = CalculateSpawnPosition(playerID);

            bool flag = false;
            string enemyName = "";

            StartOfRound playersManager = RoundManager.Instance.playersManager;

            SpawnableEnemyWithRarity matchingEnemy = playersManager.levels
                .SelectMany(level => level.Enemies)
                .FirstOrDefault(enemy => enemy.enemyType.enemyName
                    .ToLower().Contains(entity.ToLower()));


            Plugin.mls.LogInfo(">> below matching");
            if (matchingEnemy != null)
            {
                flag = true;
                SpawnEnemy(matchingEnemy, amount, inside: true, location: position);
            }
            else
            {
                Plugin.mls.LogInfo("Could not find enemy matching: " + entity);
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

                            SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
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

            return Plugin.msgbody + "/" + Plugin.msgtitle;
        }


        public static bool ToggleGodMode()
        {
            Plugin.enableGod = !Plugin.enableGod;
            return Plugin.enableGod;
        }



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
                        component.SetScrapValue(10); // Set Scrap Value
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



        public static void SpawnWeapons()
        {
            Vector3 position = new Vector3(12f, -60f, 15f);
            SelectableLevel currentLevel = RoundManager.Instance.playersManager.levels[6]; // "6" (rend) is the moon butlers will spawn on

            string buttcrack = currentLevel.Enemies[11].enemyType.name;
            string nutcrack = currentLevel.Enemies[9].enemyType.name;
            if (buttcrack == "Butler")
            {
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[11].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
            if (nutcrack == "Nutcracker")
            {
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.Enemies[9].enemyType.enemyPrefab, position, Quaternion.identity)
                    .gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
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

            if (Players.Roles.CurrentRole != null)
            {
                Plugin.msgbody = Players.Roles.CurrentRole.Name + ": " + Players.Roles.CurrentRole.Desc;
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

                if (!ulong.TryParse(vote, out playerID) || !StartOfRound.Instance.ClientPlayerList.Keys.Contains(playerID))
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