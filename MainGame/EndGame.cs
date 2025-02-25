using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Utils;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class EndGame
    {
        public static List<ulong> aliveCrew = new List<ulong>();
        public static List<ulong> aliveMonsters = new List<ulong>();
        public enum winCondition
        {
            None,
            Crew,
            Monster
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void ChecksPatch()
        {
            if (!Plugin.isHost) return;
            if (StartOfRound.Instance.inShipPhase) return;
            if (!Start.gameStarted) return;
            if (winCondition) return;
            if (StringAddons.ConvertToInt(Tasks.currentQuota.Value) >= Tasks.maxQuota)
            {
                winCondition = true;
            }
            if (aliveCrew.Count  0)
            {

            }
        }


        public static void SetupMonsterAmount()
        {
            foreach (KeyValuePair<ulong, string> id in Roles.localPlayerRoles)
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


        public static void SetupCrewAmount()
        {
            foreach (KeyValuePair<ulong, string> id in Roles.localPlayerRoles)
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

    }
}
