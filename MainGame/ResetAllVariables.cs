﻿using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.UI;
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
            MinimapUI.DestroyUI();
            if (Plugin.isHost)
            {
                RoundManager.Instance.DespawnPropsAtEndOfRound(despawnAllItems: true);
            }
            GOTools.CleanSlot();

            Plugin.ResetVariables();
            MoreSlots.DefaultSlots();
            HUDManager.Instance.ShowPlayersFiredScreen(false);
            Plugin.netHandler.RemoveCustomNetEvents();



            Plugin.mls.LogInfo(">>> Reset vars from: EndOfGameClientRpcPatch ");
        }


        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
        [HarmonyPostfix]
        private static void DisconnectPatch()
        {
            Plugin.ResetVariables();
            MinimapUI.DestroyUI();
            MoreSlots.DefaultSlots();
            Plugin.netHandler.RemoveCustomNetEvents();
            Plugin.mls.LogInfo(">>> Reset vars from: DisconnectPatch ");
        }

    }
}
