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
        public static Dictionary<ulong, bool> deathStatus = new Dictionary<ulong, bool>();

        /*
        public enum winCondition
        {
            None,
            Crew,
            Monster
        }
        */

        public static void ResetVars()
        {
            aliveCrew.Clear();
            aliveMonsters.Clear();
        }

        /*
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void WinConditionsCheck()
        {
            if (!Plugin.isHost) return;
            if (StartOfRound.Instance.inShipPhase) return;
            if (!Start.gameStarted) return;
            if (winCondition) return;
            if (StringAddons.ConvertToInt(Tasks.currentQuota.Value) >= Tasks.maxQuota)
            {
                string type = "employee";
                string topText = "Employees Won!";
                string bottomText = "By tasks";
                
                Plugin.netHandler.roundEndReceive($"{type}/{topText}/{bottomText}", Plugin.localPlayer.playerClientId);
                winCondition = true;
            }
            if (aliveMonsters.Count <= 0)
            {
                string type = "employee";
                string topText = "Employees Won!";
                string bottomText = "";
                
                Plugin.netHandler.roundEndReceive($"{type}/{topText}/{bottomText}", Plugin.localPlayer.playerClientId);
                winCondition = true;
            }
            if (aliveCrew.Count <= 1)
            {
                string type = "monster";
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
        */

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
        /// Delete, Delete, Delete!
        /// Shows who killed / how a player died at the end of a round
        /// in the status popup
        /// </summary>
        public static void DeathNote(ulong playerID, string note)
        {
            if (killedByNote.ContainsKey(playerID)) return;
            killedByNote.Add(playerID, note);
        }


        /// <summary>
        /// Adds player's alive condition for when the round ends.
        /// If they survived the round / died.
        /// </summary>
        /*
        public static void DeathStatus(ulong playerID)
        {
            if (deathStatus.ContainsKey(playerID)) return;
            deathStatus.Add(playerID, true);
        }
        */
    }
}
