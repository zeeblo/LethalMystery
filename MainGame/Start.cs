using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Patches;
using LethalMystery.Players;
using UnityEngine;


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
            if (Plugin.inGracePeriod)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void GracePeriodTime()
        {
            if (Plugin.inGracePeriod)
            {
                // Countdown from the set grace period time
                if (Plugin.currentGracePeriodCountdown >= 0)
                {
                    Plugin.currentGracePeriodCountdown -= Time.deltaTime;
                }
                else
                {
                    HUDManager.Instance.loadingText.enabled = false;
                    Plugin.inGracePeriod = false;
                }

                // Show GUI that displays the grace period time
                if (Plugin.inMeeting == false && Plugin.inGracePeriod)
                {
                    HUDManager.Instance.loadingText.enabled = true;
                    HUDManager.Instance.loadingText.text = $"Grace Period: {(int)Plugin.currentGracePeriodCountdown}";
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
            if (StartOfRound.Instance.shipHasLanded == false || Plugin.inMeeting == true || Plugin.MeetingNum <= 0)
                return;
            if (!(Plugin.MeetingCooldown <= 0)) // If MeetingCooldown is still greater than 0 then dont continue
                return;

            Plugin.inMeeting = true;
            Plugin.MeetingNum -= 1;
            Plugin.inGracePeriod = true;
            Plugin.currentGracePeriodCountdown = Plugin.defaultMeetingCountdown + 140f;
            HUDManagerPatch.DisplayDaysEdit("meeting");

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            Plugin.RemoveEnvironment();
        }


        [HarmonyPatch(typeof(ShipAlarmCord), nameof(ShipAlarmCord.Update))]
        [HarmonyPostfix]
        private static void Countdown()
        {
            if (StartOfRound.Instance.shipHasLanded == false)
                return;

            if (Plugin.MeetingCooldown >= 0)
            {
                Plugin.MeetingCooldown -= Time.deltaTime;
            }
        }




        [HarmonyPatch(typeof(HangarShipDoor), "Update")]
        [HarmonyPostfix]
        private static void DoorsPatch(HangarShipDoor __instance)
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

                Plugin.RemoveEnvironment(false);
                Plugin.MeetingDefaults();

                Plugin.mls.LogInfo(">>> Stopping meeting and opening doors.");
            }
        }


    }
}
