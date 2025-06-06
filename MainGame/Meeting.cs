using GameNetcodeStuff;
using System.Reflection;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Meeting
    {

        public static LNetworkVariable<string> discussTime = LNetworkVariable<string>.Connect("discussTime");
        public static LNetworkVariable<string> voteTime = LNetworkVariable<string>.Connect("voteTime");
        public static LNetworkVariable<string> inMeeting = LNetworkVariable<string>.Connect("inMeeting");
        public static LNetworkVariable<string> currentMeetingTime = LNetworkVariable<string>.Connect("currentMeetingCountdown");
        public static LNetworkVariable<string> MeetingCooldown = LNetworkVariable<string>.Connect("MeetingCooldown");
        public static int MeetingNum;

        public static void MeetingDefaults()
        {
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {MeetingNum}";
            VotingUI.inVoteTime = false;
            VotingUI.isCalled = false;
            Controls.UnlockCursor(false);
            Voting.VoteSetup();
            if (!Plugin.isHost) return;
            Plugin.mls.LogInfo(">>> b4inMeetingVal:");

            Start.currentGracePeriodTime.Value = $"{Start.hostGracePeriodTime}";
            inMeeting.Value = "false";
            Plugin.mls.LogInfo($">>> inMeetingVal: {inMeeting.Value}");
            //currentMeetingCountdown.Value = $"{LMConfig.defaultMeetingCountdown}";
            MeetingCooldown.Value = $"{Start.hostMeetingCooldown}";
            discussTime.Value = $"{Start.hostDiscussTime}";
            voteTime.Value = $"{Start.hostVoteTime}";
        }



        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.StopHorn))]
        [HarmonyPostfix]
        private static void CallAMeeting()
        {
            if (StartOfRound.Instance.shipHasLanded == false || StringAddons.ConvertToBool(inMeeting.Value) == true || MeetingNum <= 0)
                return;
            if (!(StringAddons.ConvertToFloat(MeetingCooldown.Value) <= 0)) // If MeetingCooldown is still greater than 0 then dont continue
                return;
            if (StartOfRound.Instance.shipLeftAutomatically) return;

            MeetingNum -= 1;
            Plugin.netHandler.MeetingReceive("meeting", Plugin.localPlayer.actualClientId);
        }


        [HarmonyPatch(typeof(ShipAlarmCord), "Update")]
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
                    //float countdown = StringAddons.ConvertToFloat(currentMeetingCountdown.Value);
                    //countdown -= Time.deltaTime;

                    VoteCountdown();
                    //currentMeetingCountdown.Value = $"{countdown}";
                }
                __instance.PlayDoorAnimation(closed: true);
                __instance.SetDoorButtonsEnabled(false);
                __instance.doorPower = 1;
                __instance.overheated = false;
                __instance.triggerScript.interactable = false;


                StartOfRound.Instance.mapScreen.SwitchScreenOn(false);

            }


            if (StringAddons.ConvertToFloat(voteTime.Value) <= 0 && !StringAddons.ConvertToBool(EjectPlayers.currentlyEjectingPlayer.Value))
            {
                MeetingDefaults();
                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }



        private static void VoteCountdown()
        {
            if (Start.hostDiscussTime > 0 && StringAddons.ConvertToFloat(discussTime.Value) > 0)
            {
                float countdown = StringAddons.ConvertToFloat(discussTime.Value);
                countdown -= Time.deltaTime;

                discussTime.Value = $"{countdown}";
            }
            else
            {
                if (StringAddons.ConvertToFloat(voteTime.Value) <= 0) return;

                float countdown = StringAddons.ConvertToFloat(voteTime.Value);
                countdown -= Time.deltaTime;

                bool plrsVote = Voting.EveryoneVoted();
                if (plrsVote)
                {
                    countdown = 10;
                }

                voteTime.Value = $"{countdown}";
            }
        }



        /// <summary>
        /// Drop all scrap items when being teleported in the ship
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DropAllHeldItems))]
        [HarmonyPrefix]
        private static bool DropItems(PlayerControllerB __instance, bool itemsFall, bool disconnecting)
        {
            for (int i = 0; i < __instance.ItemSlots.Length; i++)
            {
                GrabbableObject grabbableObject = __instance.ItemSlots[i];
                if (!(grabbableObject != null))
                {
                    continue;
                }
                if (!Tasks.allScraps.Contains(__instance.ItemSlots[i].itemProperties.itemName))
                {
                    continue;
                }
                if (itemsFall)
                {
                    grabbableObject.parentObject = null;
                    grabbableObject.heldByPlayerOnServer = false;
                    if (__instance.isInElevator)
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.elevatorTransform, worldPositionStays: true);
                    }
                    else
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.propsContainer, worldPositionStays: true);
                    }
                    __instance.SetItemInElevator(__instance.isInHangarShipRoom, __instance.isInElevator, grabbableObject);
                    grabbableObject.EnablePhysics(enable: true);
                    grabbableObject.EnableItemMeshes(enable: true);
                    grabbableObject.transform.localScale = grabbableObject.originalScale;
                    grabbableObject.isHeld = false;
                    grabbableObject.isPocketed = false;
                    grabbableObject.startFallingPosition = grabbableObject.transform.parent.InverseTransformPoint(grabbableObject.transform.position);
                    grabbableObject.FallToGround(randomizePosition: true);
                    grabbableObject.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
                    if (__instance.IsOwner)
                    {
                        grabbableObject.DiscardItemOnClient();
                    }
                    else if (!grabbableObject.itemProperties.syncDiscardFunction)
                    {
                        grabbableObject.playerHeldBy = null;
                    }
                }
                if (__instance.IsOwner && !disconnecting)
                {
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                    HUDManager.Instance.ClearControlTips();
                    __instance.activatingItem = false;
                }
                __instance.ItemSlots[i] = null;
            }
            if (__instance.isHoldingObject)
            {
                __instance.isHoldingObject = false;
                if (__instance.currentlyHeldObjectServer != null)
                {
                    MethodInfo SetSpecialGrabAnimationBool = typeof(PlayerControllerB).GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
                    SetSpecialGrabAnimationBool.Invoke(__instance, new object[] { true, __instance.currentlyHeldObjectServer });
                }
                __instance.playerBodyAnimator.SetBool("cancelHolding", value: true);
                __instance.playerBodyAnimator.SetTrigger("Throw");
            }
            __instance.activatingItem = false;
            __instance.twoHanded = false;
            __instance.carryWeight = 1f;
            __instance.currentlyHeldObjectServer = null;

            return false;
        }

    }
}
