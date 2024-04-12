using HarmonyLib;


namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.BeginHoldingInteractOnLever))]
        [HarmonyPostfix]
        private static void NotifyPatch()
        {
            // Disable Lever if there's less than 4 players in lobby
            if (StartOfRound.Instance.ClientPlayerList.Keys.ToArray().Length < 4)
            {
                float playerPosX = GameNetworkManager.Instance.localPlayerController.transform.position.x;
                float playerPosY = GameNetworkManager.Instance.localPlayerController.transform.position.y;
                float playerPosZ = GameNetworkManager.Instance.localPlayerController.transform.position.z;
                UnityEngine.Vector3 playerPos = new UnityEngine.Vector3(playerPosX, playerPosY, playerPosZ-5);

                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(playerPos);
                HUDManager.Instance.DisplayTip("Not Enough Players!", "You need at least 4 players to start the game.", isWarning: true);

            }
        }
    }
}
