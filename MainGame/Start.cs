using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Start
    {



        /// <summary>
        /// Disable Lever if there's less than 4 players in lobby
        /// </summary>
        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.BeginHoldingInteractOnLever))]
        [HarmonyPostfix]
        private static void NotEnoughPlayers()
        {

            /*
            if (StartOfRound.Instance.ClientPlayerList.Keys.ToArray().Length < 4)
            {
                Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                HUDManager.Instance.DisplayTip("Not Enough Players!", "You need at least 4 players to start the game.", isWarning: true);

            }
            */
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPrefix]
        private static void SubscribeToHandler()
        {
            Plugin.netHandler.AddCustomNetEvents();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPostfix]
        private static void Begin()
        {
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            terminal.groupCredits = 9999;
            Plugin.localPlayer = GameNetworkManager.Instance.localPlayerController;

            Plugin.ResetVariables();
            Tasks.AppendScraps();
            Roles.AssignRole();
            CharacterDisplay.BlackVision(true);

        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void IntroScreen()
        {
            CharacterDisplay.inIntro = true;
            StartOfRound.Instance.StartCoroutine(CharacterDisplay.IntroDisplay());
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void SpawnHorn(StartOfRound __instance)
        {
            if (!Plugin.isHost) return;

            MethodInfo UnlockShipObject = typeof(StartOfRound).GetMethod("UnlockShipObject", BindingFlags.NonPublic | BindingFlags.Instance);
            UnlockShipObject.Invoke(__instance, new object[] { 18 });
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPrefix]
        private static bool GracePeriod()
        {
            if (StringAddons.ConvertToBool(Plugin.inGracePeriod.Value))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void GracePeriodTime()
        {
            if (StartOfRound.Instance.inShipPhase) return;

            if (StringAddons.ConvertToBool(Plugin.inGracePeriod.Value))
            {
                // Countdown from the set grace period time
                if (StringAddons.ConvertToFloat(Plugin.currentGracePeriodCountdown.Value) >= 0 && Plugin.isHost)
                {
                    float countdown = StringAddons.ConvertToFloat(Plugin.currentGracePeriodCountdown.Value);
                    countdown -= Time.deltaTime;
                    Plugin.currentGracePeriodCountdown.Value = $"{countdown}";
                }
                else
                {
                    
                    if (Plugin.isHost)
                    {
                        Plugin.inGracePeriod.Value = "false";
                    }

                }

                // Show GUI that displays the grace period time
                if (StringAddons.ConvertToBool(Plugin.inMeeting.Value) == false && StringAddons.ConvertToBool(Plugin.inGracePeriod.Value))
                {
                    HUDManager.Instance.loadingText.enabled = true;
                    HUDManager.Instance.loadingText.text = $"Grace Period: {(int)StringAddons.ConvertToFloat(Plugin.currentGracePeriodCountdown.Value)}";
                }
            }

        }


        /// <summary>
        /// Gets rid of landmines, turrets, etc.
        /// </summary>
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnMapObjects))]
        [HarmonyPrefix]
        private static bool RemoveDangerObjects()
        {
            return false;
        }

        // Shoutout to peacefulCompany
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        private static bool NoEnemiesPatch(ref SelectableLevel newLevel)
        {
            foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
            {
                enemy.rarity = 0;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                outsideEnemy.rarity = 0;
            }
            foreach (SpawnableEnemyWithRarity daytimeEnemy in newLevel.DaytimeEnemies)
            {
                daytimeEnemy.rarity = 0;
            }
            return true;
        }



        #region Chat Command


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void SetIsHost()
        {
            Plugin.mls.LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost);
            Plugin.isHost = RoundManager.Instance.NetworkManager.IsHost;

        }


        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPostfix]
        private static void UpdateNewInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            Plugin.currentLevel = ___currentLevel;
            Plugin.currentLevelVents = ___allEnemyVents;
            HUDManager.Instance.chatTextField.characterLimit = 999;
        }


        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        private static void UpdateCurrentLevelInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            Plugin.currentLevel = ___currentLevel;
            Plugin.currentLevelVents = ___allEnemyVents;
        }


        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            Plugin.currentRound = RoundManager.Instance;

            return true;
        }


        #endregion Chat Commands







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
                Plugin.MeetingDefaults();
                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }


        [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Start))]
        [HarmonyPostfix]
        private static void SampleSceneObjects()
        {
            Plugin.shipInstance = GameObject.Find("Environment/HangarShip/AnimatedShipDoor");
        }



    }
}
