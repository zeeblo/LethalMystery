using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ShipAlarmCord))]
    internal class ShipAlarmCordPatch
    {

        

        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.StopHorn))]
        [HarmonyPostfix]
        private static void CallAMeeting()
        {
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.inMeeting == true)
                return;
            if (!(Plugin.MeetingCooldown <= 0)) // If MeetingCooldown is still 1-10 then dont continue
                return;

            Plugin.inMeeting = true;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            //HUDManager.Instance.DisplayTip("Meeting", $"{Plugin.currentMeetingCountdown}");
            //HUDManager.Instance.tipsPanelAnimator.enabled = true;
            //HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerHint");
            Plugin.ShowSidebar(show: true, $"{Plugin.currentMeetingCountdown}");
        }


        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (StartOfRound.Instance.shipHasLanded == false)
                return;

            if (Plugin.MeetingCooldown >= 0)
            {
                Plugin.MeetingCooldown -= Time.deltaTime;
            }
        }
    }
}
