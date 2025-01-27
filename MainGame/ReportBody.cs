using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class ReportBody
    {

        [HarmonyPatch(typeof(RagdollGrabbableObject), nameof(RagdollGrabbableObject.EquipItem))]
        [HarmonyPostfix]
        private static void StartMeeting()
        {
            if (StartOfRound.Instance.inShipPhase || StringAddons.ConvertToBool(Meeting.inMeeting.Value))
                return;

            Plugin.netHandler.MeetingReceive("body", Plugin.localPlayer.playerClientId);
        }
    }
}
