using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Meeting
    {

        public static void MeetingDefaults()
        {
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Plugin.MeetingNum}";
            if (!Plugin.isHost) return;
            Plugin.mls.LogInfo(">>> b4inMeetingVal:");

            Plugin.currentGracePeriodCountdown.Value = $"{Plugin.defaultGracePeriodCountdown}";
            Plugin.inMeeting.Value = "false";
            Plugin.mls.LogInfo($">>> inMeetingVal: {Plugin.inMeeting.Value}");
            Plugin.currentMeetingCountdown.Value = $"{Plugin.defaultMeetingCountdown}";
            Plugin.MeetingCooldown.Value = $"{Plugin.defaultMeetingCooldown}";
        }



        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.StopHorn))]
        [HarmonyPostfix]
        private static void CallAMeeting()
        {
            if (StartOfRound.Instance.shipHasLanded == false || StringAddons.ConvertToBool(Plugin.inMeeting.Value) == true || Plugin.MeetingNum <= 0)
                return;
            if (!(StringAddons.ConvertToFloat(Plugin.MeetingCooldown.Value) <= 0)) // If MeetingCooldown is still greater than 0 then dont continue
                return;

            Plugin.MeetingNum -= 1;
            Plugin.netHandler.MeetingReceive("meeting", Plugin.localPlayer.actualClientId);
        }


        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.Update))]
        [HarmonyPostfix]
        private static void Cooldown()
        {
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.isHost == false)
                return;

            if (StringAddons.ConvertToFloat(Plugin.MeetingCooldown.Value) >= 0)
            {
                float countdown = StringAddons.ConvertToFloat(Plugin.MeetingCooldown.Value);
                countdown -= Time.deltaTime;
                Plugin.MeetingCooldown.Value = $"{countdown}";
            }
        }


        [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Update))]
        [HarmonyPostfix]
        private static void DoorsPatch(HangarShipDoor __instance)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (StringAddons.ConvertToBool(Plugin.inMeeting.Value))
            {
                if (Plugin.isHost)
                {
                    float countdown = StringAddons.ConvertToFloat(Plugin.currentMeetingCountdown.Value);
                    countdown -= Time.deltaTime;
                    Plugin.currentMeetingCountdown.Value = $"{countdown}";
                }
                __instance.PlayDoorAnimation(closed: true);
                __instance.SetDoorButtonsEnabled(false);
                __instance.doorPower = 1;
                __instance.overheated = false;
                __instance.triggerScript.interactable = false;
            }

            if (StringAddons.ConvertToFloat(Plugin.currentMeetingCountdown.Value) <= 0)
            {
                MeetingDefaults();
                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }


    }
}
