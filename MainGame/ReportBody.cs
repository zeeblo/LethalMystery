using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class ReportBody
    {

        [HarmonyPatch(typeof(RagdollGrabbableObject), nameof(RagdollGrabbableObject.EquipItem))]
        [HarmonyPostfix]
        private static void StartMeeting(RagdollGrabbableObject __instance)
        {
            if (StartOfRound.Instance.inShipPhase || StringAddons.ConvertToBool(Meeting.inMeeting.Value))
                return;
            if (StartOfRound.Instance.shipLeftAutomatically) return;

            PlayerControllerB previousPlayerHeldBy = (PlayerControllerB)Traverse.Create(__instance).Field("previousPlayerHeldBy").GetValue();

            if (previousPlayerHeldBy != null && previousPlayerHeldBy.playerClientId == Plugin.localPlayer.playerClientId)
            {
                Plugin.netHandler.MeetingReceive("body", Plugin.localPlayer.actualClientId);
            }
        }
    }
}
