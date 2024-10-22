using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ShipAlarmCord))]
    internal class ShipAlarmCordPatch
    {



        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.StopHorn))]
        [HarmonyPostfix]
        private static void CallAMeeting()
        {
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.inMeeting == true || Plugin.MeetingNum <= 0)
                return;
            if (!(Plugin.MeetingCooldown <= 0)) // If MeetingCooldown is still 1-10 then dont continue
                return;

            Plugin.inMeeting = true;
            Plugin.MeetingNum -= 1;

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            Plugin.RemoveEnvironment();
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
