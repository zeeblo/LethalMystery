using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Utils;


namespace LethalMystery.MainGame
{

    [HarmonyPatch]
    internal class ResetAllVariables
    {

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndOfGameClientRpc))]
        [HarmonyPostfix]
        private static void EndOfGameClientRpcPatch()
        {
            Controls.monsterControls.Disable();
            Controls.playerControls.Disable();
            //GOTools.ClearInventory();
            if (Plugin.isHost)
            {
                RoundManager.Instance.DespawnPropsAtEndOfRound(despawnAllItems: true);
            }
            GOTools.CleanSlot();

            Plugin.ResetVariables();
            MoreSlots.DefaultSlots();
            Plugin.netHandler.RemoveCustomNetEvents();

            Plugin.mls.LogInfo(">>> Reset vars from: EndOfGameClientRpcPatch ");
        }


        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
        [HarmonyPostfix]
        private static void DisconnectPatch()
        {
            Controls.monsterControls.Disable();
            Controls.playerControls.Disable();
            Plugin.ResetVariables();
            MoreSlots.DefaultSlots();
            Plugin.netHandler.RemoveCustomNetEvents();
            Plugin.mls.LogInfo(">>> Reset vars from: DisconnectPatch ");
        }

    }
}
