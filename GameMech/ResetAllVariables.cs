using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Utils;


namespace LethalMystery.GameMech
{

    [HarmonyPatch]
    internal class ResetAllVariables
    {

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndOfGameClientRpc))]
        [HarmonyPostfix]
        private static void EndOfGameClientRpcPatch()
        {
            Controls.monsterControls.Disable();
            GOTools.ClearInventory();
            Plugin.ResetVariables();
            MoreSlots.DefaultSlots();
            

            Plugin.mls.LogInfo(">>> Reset vars from: EndOfGameClientRpcPatch ");
        }


        [HarmonyPatch(typeof(PlayerControllerB), "OnEnable")]
        [HarmonyPostfix]
        private static void OnEnablePatch()
        {
            if (StartOfRound.Instance.inShipPhase)
            {
                Controls.monsterControls.Disable();
                Plugin.ResetVariables();
                Plugin.mls.LogInfo(">>> Reset vars from: OnEnablePatch ");
            }
        }

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
        [HarmonyPostfix]
        private static void DisconnectPatch()
        {
            Controls.monsterControls.Disable();
            Plugin.ResetVariables();
            MoreSlots.DefaultSlots();
            Plugin.mls.LogInfo(">>> Reset vars from: DisconnectPatch ");
        }
    }
}
