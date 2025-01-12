using HarmonyLib;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Meeting
    {

        public static LNetworkVariable<string> discussTime = LNetworkVariable<string>.Connect("discussTime");
        public static LNetworkVariable<string> voteTime = LNetworkVariable<string>.Connect("voteTime");
        public static LNetworkVariable<string> inMeeting = LNetworkVariable<string>.Connect("inMeeting");
        public static LNetworkVariable<string> currentMeetingCountdown = LNetworkVariable<string>.Connect("currentMeetingCountdown");
        public static LNetworkVariable<string> MeetingCooldown = LNetworkVariable<string>.Connect("MeetingCooldown");
        public static int MeetingNum = LMConfig.defaultMeetingNum;

        public static void MeetingDefaults()
        {
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {MeetingNum}";
            VotingUI.inVoteTime = false;
            VotingUI.isCalled = false;
            Voting.VoteSetup();
            if (!Plugin.isHost) return;
            Plugin.mls.LogInfo(">>> b4inMeetingVal:");

            Start.currentGracePeriodCountdown.Value = $"{LMConfig.defaultGracePeriodCountdown}";
            inMeeting.Value = "false";
            Plugin.mls.LogInfo($">>> inMeetingVal: {inMeeting.Value}");
            currentMeetingCountdown.Value = $"{LMConfig.defaultMeetingCountdown}";
            MeetingCooldown.Value = $"{LMConfig.defaultMeetingCooldown}";
            discussTime.Value = $"{LMConfig.defaultDiscussTime}";
            voteTime.Value = $"{LMConfig.defaultVoteTime}";
        }



        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.StopHorn))]
        [HarmonyPostfix]
        private static void CallAMeeting()
        {
            if (StartOfRound.Instance.shipHasLanded == false || StringAddons.ConvertToBool(inMeeting.Value) == true || MeetingNum <= 0)
                return;
            if (!(StringAddons.ConvertToFloat(MeetingCooldown.Value) <= 0)) // If MeetingCooldown is still greater than 0 then dont continue
                return;

            MeetingNum -= 1;
            Plugin.netHandler.MeetingReceive("meeting", Plugin.localPlayer.actualClientId);
        }


        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.Update))]
        [HarmonyPostfix]
        private static void Cooldown()
        {
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.isHost == false)
                return;

            if (StringAddons.ConvertToFloat(MeetingCooldown.Value) >= 0)
            {
                float countdown = StringAddons.ConvertToFloat(MeetingCooldown.Value);
                countdown -= Time.deltaTime;
                MeetingCooldown.Value = $"{countdown}";
            }
        }


        [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Update))]
        [HarmonyPostfix]
        private static void MeetingTimeLimit(HangarShipDoor __instance)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (StringAddons.ConvertToBool(inMeeting.Value))
            {
                if (Plugin.isHost)
                {
                    float countdown = StringAddons.ConvertToFloat(currentMeetingCountdown.Value);
                    countdown -= Time.deltaTime;

                    VoteCountdown();
                    currentMeetingCountdown.Value = $"{countdown}";
                }
                __instance.PlayDoorAnimation(closed: true);
                __instance.SetDoorButtonsEnabled(false);
                __instance.doorPower = 1;
                __instance.overheated = false;
                __instance.triggerScript.interactable = false;
            }

            if (StringAddons.ConvertToFloat(currentMeetingCountdown.Value) <= 0)
            {
                MeetingDefaults();
                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }



        private static void VoteCountdown()
        {
            if (LMConfig.defaultDiscussTime > 0 && StringAddons.ConvertToFloat(discussTime.Value) > 0)
            {
                float countdown = StringAddons.ConvertToFloat(discussTime.Value);
                countdown -= Time.deltaTime;

                discussTime.Value = $"{countdown}";
            }
            else
            {
                float countdown = StringAddons.ConvertToFloat(voteTime.Value);
                countdown -= Time.deltaTime;

                voteTime.Value = $"{countdown}";
            }
        }

    }
}
