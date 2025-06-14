﻿using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Utils;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class EndGame
    {
        public static bool winCondition = false;
        public static List<ulong> aliveCrew = new List<ulong>();
        public static List<ulong> aliveMonsters = new List<ulong>();
        public static Dictionary<ulong, string> killedByNote = new Dictionary<ulong, string>();
        public static List<ulong> lastPlayersAlive = new List<ulong>();
        public static string monsterNames = "";

        public static void ResetVars()
        {
            aliveCrew.Clear();
            aliveMonsters.Clear();
            monsterNames = "";
        }

        
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void WinConditionsCheck()
        {
            // Note: Replace names with ids instead then convert back to names
            if (Plugin.inTestMode && Plugin.FoundThisMod("zeebloTesting.zeeblo.dev") && Plugin.isHost) return;
            if (!Plugin.isHost) return;
            if (StartOfRound.Instance.inShipPhase) return;
            if (!Start.gameStarted) return;
            if (winCondition) return;
            if (StringAddons.ConvertToInt(Tasks.currentQuota.Value) >= Tasks.maxQuota)
            {
                string type = monsterNames;
                string topText = "Employees Won!";
                string bottomText = "By tasks";

                Plugin.netHandler.roundEndReceive($"{type}/{topText}/{bottomText}", Plugin.localPlayer.playerClientId);
                winCondition = true;
            }
            if (aliveMonsters.Count <= 0)
            {
                string type = monsterNames;
                string topText = "Employees Won!";
                string bottomText = "";

                Plugin.netHandler.roundEndReceive($"{type}/{topText}/{bottomText}", Plugin.localPlayer.playerClientId);
                winCondition = true;
            }
            if (aliveCrew.Count <= 1)
            {
                string type = monsterNames;
                string topText = "Monsters Won!";
                string bottomText = "";
                foreach (ulong impID in aliveMonsters)
                {
                    foreach (PlayerControllerB plr in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (plr.playerClientId == impID)
                        {
                            bottomText += plr.playerUsername + ", ";
                            bottomText = (bottomText.EndsWith(", ")) ? bottomText.Replace(",", "") : bottomText;
                        }
                    }
                }

                Plugin.netHandler.roundEndReceive($"{type}/{topText}/{bottomText}", Plugin.localPlayer.playerClientId);
                winCondition = true;
            }

        }
        
       
        public static void SetupMonsterAmount(Dictionary<ulong, string> playerAndRoles)
        {
            foreach (KeyValuePair<ulong, string> id in playerAndRoles)
            {

                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (aliveMonsters.Contains(player.playerClientId)) continue;
                    if (id.Key == player.playerClientId && Roles.NameIsMonsterType(id.Value))
                    {
                        aliveMonsters.Add(player.playerClientId);
                        monsterNames += player.playerUsername + ", ";
                        monsterNames = (monsterNames.EndsWith(", ")) ? monsterNames.Replace(",", "") : monsterNames;
                    }
                }
            }
        }


        public static void SetupCrewAmount(Dictionary<ulong, string> playerAndRoles)
        {
            foreach (KeyValuePair<ulong, string> id in playerAndRoles)
            {

                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (aliveCrew.Contains(player.playerClientId)) continue;
                    if ((id.Key == player.playerClientId) && !Roles.NameIsMonsterType(id.Value))
                    {
                        aliveCrew.Add(player.playerClientId);
                    }
                }
            }
        }



        /// <summary>
        /// When a monsters/employees win, tally up the people still alive
        /// </summary>
        public static void AddLastAlive()
        {

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (lastPlayersAlive.Contains(player.playerClientId)) continue;
                if (player.isPlayerDead == false)
                {
                    lastPlayersAlive.Add(player.playerClientId);
                }

            }
        }


        /// <summary>
        /// Delete, Delete, Delete!
        /// Shows who killed / how a player died at the end of a round
        /// in the status popup
        /// </summary>
        public static void DeathNote(ulong playerID, string note)
        {
            if (killedByNote.ContainsKey(playerID)) return;
            killedByNote.Add(playerID, note);
        }


    }
}
