using HarmonyLib;
using LethalMystery.Utils;
using LethalNetworkAPI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using GameNetcodeStuff;
using LethalMystery.UI;


namespace LethalMystery.MainGame
{

    internal class zTests
    {
        public static Vector3 modelPosition = new Vector3(80f, 20f, 20f);
        public static Vector3 groundPosition = new Vector3(80f, 18.8f, 20f);
        public static Vector3 cameraPosition = new Vector3(80f, 31.05f, 26f);
        public static Vector3 lightPosition = new Vector3(80f, 34f, 20f);
        public static GameObject? playerObj;
        public static GameObject? lght;
        public static Camera? introCamera;
        public static bool entered = false;
        public static bool entered6 = false;
        public static bool disableMovement = false;
        private static GameObject? HeaderImage;
        private static GameObject? BannerImage;
        private static GameObject? BGCanvas;




        [HarmonyPatch]
        internal class AdminCMDS
        {


        }




        [HarmonyPatch(typeof(HUDManager))]
        internal class AdminCMDS_2
        {
            [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Update))]
            [HarmonyPostfix]
            private static void Keys(HUDManager __instance)
            {
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("metalsheet", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
                {
                    Commands.SpawnScrapFunc("ring", $"{GameNetworkManager.Instance.localPlayerController.transform.position}", toInventory: true);
                }
                if (Keyboard.current.digit5Key.wasPressedThisFrame)
                {
                    Plugin.mls.LogInfo($">>> Role is: {Plugin.netHandler.GetallPlayerRoles().Name}");

                    Plugin.mls.LogInfo($">>> InMetting Value: {Meeting.inMeeting.Value}");
                    Plugin.mls.LogInfo($">>> inGracePeriod Value: {Start.inGracePeriod.Value}");
                    Plugin.mls.LogInfo($">>> currentGracePeriodCountdown Value: {Start.currentGracePeriodCountdown.Value}");
                    //Plugin.mls.LogInfo($">>> currentMeetingCountdown Value: {Meeting.currentMeetingCountdown.Value}");
                    Plugin.mls.LogInfo($">>> MeetingCooldown Value: {Meeting.MeetingCooldown.Value}");


                    foreach (KeyValuePair<string, string> d in Voting.playersWhoVoted.Value)
                    {
                        Plugin.mls.LogInfo($">>> P who voted: {d.Key} | val: {d.Value}");
                    }
                }
                if (Keyboard.current.digit7Key.wasPressedThisFrame)
                {

                    Plugin.mls.LogInfo($">>> My ID is: {Plugin.localPlayer.actualClientId}");
                    Plugin.mls.LogInfo($">>> My PID is: {Plugin.localPlayer.playerClientId}");
                    Plugin.mls.LogInfo($">>> thisClientPlayerId: {StartOfRound.Instance.thisClientPlayerId}");
                    Plugin.mls.LogInfo($">>> connectedPlayersAmount: {StartOfRound.Instance.connectedPlayersAmount}");
                    Plugin.mls.LogInfo($">>> livingPlayers: {StartOfRound.Instance.livingPlayers}");
                    //ClientPlayerList.Remove(clientId); (startofround)

                    foreach (KeyValuePair<string, string> d in Voting.playersWhoGotVoted.Value)
                    {
                        Plugin.mls.LogInfo($">>> PID: {d.Key} | VoteVal: {d.Value}");
                    }
                    Plugin.mls.LogInfo($">>skipVal es: {Voting.skipVotes.Value}");
                   
                    
                }

            }
        }
    }
}
