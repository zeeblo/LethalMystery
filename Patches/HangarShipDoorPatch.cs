using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(HangarShipDoor))]
    internal class HangarShipDoorPatch
    {


        [HarmonyPatch(typeof(HangarShipDoor), "Update")]
        [HarmonyPostfix]
        private static void StartPatch(HangarShipDoor __instance)
        {

            if (Plugin.inMeeting)
            {
                Plugin.currentMeetingCountdown -= Time.deltaTime;
                __instance.PlayDoorAnimation(closed: true);
                __instance.SetDoorButtonsEnabled(false);
                __instance.doorPower = 1;
                __instance.overheated = false;
                __instance.triggerScript.interactable = false;
            }

            if (Plugin.currentMeetingCountdown <= 0)
            {
                __instance.PlayDoorAnimation(closed: false);
                __instance.SetDoorButtonsEnabled(true);
                __instance.doorPower = 0;
                __instance.overheated = true;
                __instance.triggerScript.interactable = true;

                Plugin.inMeeting = false;
                StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Plugin.MeetingNum}";
                Plugin.currentMeetingCountdown = Plugin.defaultMeetingCountdown;
                Plugin.MeetingCooldown = Plugin.defaultMeetingCooldown;
                Plugin.ShowSidebar(show: false, $"{Plugin.currentMeetingCountdown}");
                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }
    }
}
