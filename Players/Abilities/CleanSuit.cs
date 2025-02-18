
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Players.Abilities
{
    [HarmonyPatch]
    internal class CleanSuit
    {

        public static float cleaningBloodAmt = 0;


        public static void ResetVars()
        {
            cleaningBloodAmt = 0;
        }


        private static void HoldToClean()
        {
            cleaningBloodAmt += 1f * Time.deltaTime;
            float timeToHold = 4f;
            HUDManager.Instance.holdFillAmount = cleaningBloodAmt;
            HUDManager.Instance.holdInteractionFillAmount.fillAmount = cleaningBloodAmt / timeToHold;

            if (cleaningBloodAmt > timeToHold)
            {
                Controls.StopCleaning();
                //Plugin.localPlayer.RemoveBloodFromBody();
                Plugin.netHandler.playerBloodReceive($"{Plugin.localPlayer.playerClientId}/clean", Plugin.localPlayer.playerClientId);
                Plugin.localPlayer.movementAudio.PlayOneShot(StartOfRound.Instance.changeSuitSFX);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (Controls.cleaningBody)
            {
                HoldToClean();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StopHoldInteractionOnTrigger))]
        [HarmonyPrefix]
        private static bool StopHoldInteractionOnTriggerPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ClickHoldInteraction))]
        [HarmonyPrefix]
        private static bool ClickHoldInteractionPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPrefix]
        private static bool StopInteractionPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }


    }
}
