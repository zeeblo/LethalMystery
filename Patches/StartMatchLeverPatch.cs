using HarmonyLib;
using UnityEngine;


namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.BeginHoldingInteractOnLever))]
        [HarmonyPostfix]
        private static void NotEnoughPlayers()
        {
            // Disable Lever if there's less than 4 players in lobby

            /*
            if (StartOfRound.Instance.ClientPlayerList.Keys.ToArray().Length < 4)
            {
                Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                HUDManager.Instance.DisplayTip("Not Enough Players!", "You need at least 4 players to start the game.", isWarning: true);

            }
            */
        }
    }
}
