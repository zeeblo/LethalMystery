using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using UnityEngine.InputSystem;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {

        public static List<int> playerSuits = new List<int>();

        public static void ResetVars()
        {
            playerSuits.Clear();
        }


        /// <summary>
        /// Players can only select a suit that hasn't been claimed
        /// before the game starts
        /// </summary>
        [HarmonyPatch(nameof(UnlockableSuit.SwitchSuitToThis))]
        [HarmonyPrefix]
        private static bool SwitchSuitToThisPatch(UnlockableSuit __instance, PlayerControllerB playerWhoTriggered)
        {
            Plugin.mls.LogInfo($">>> SwitchSuitToThis() | SuitID: {__instance.suitID}");
            if (GameNetworkManager.Instance.gameHasStarted) return false;
            if (__instance.suitID == 0) return true;
            if (playerSuits.Contains(__instance.suitID)) return false;
            Plugin.netHandler.suitsReceive($"{playerWhoTriggered.currentSuitID}/{__instance.suitID}", playerWhoTriggered.playerClientId);

            return true;
        }


    }
}
