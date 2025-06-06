using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Maps;
using LethalMystery.Players;
using LethalMystery.Players.Abilities;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;
using static LethalMystery.Utils.LMConfig;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Start
    {

        public static LNetworkVariable<string> inGracePeriod = LNetworkVariable<string>.Connect("inGracePeriod");
        public static LNetworkVariable<string> currentGracePeriodTime = LNetworkVariable<string>.Connect("currentGracePeriodCountdown");
        public static bool startSpawningScraps = false;
        public static bool gameStarted = false;
        public static bool localGracePeriod = false;
        public static float scrapTimer = 0;
        [Header("Host Config")]
        public static MonsterAmt hostImposterAmt;
        public static SheriffAmt hostSheriffAmt;
        public static float hostDiscussTime;
        public static float hostVoteTime;
        public static float hostMeetingCooldown;
        public static int hostMeetingNum;
        public static float hostGracePeriodTime;
        public static float hostKillCooldown;
        public static float hostScrapTimer;
        public static bool hostEnableChat;

        public static void ResetVars()
        {
            startSpawningScraps = false;
            scrapTimer = 0;
            gameStarted = false;
            localGracePeriod = false;
        }




        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        private static void StartPatch()
        {
            Plugin.terminal.groupCredits = 0;
        }


        /// <summary>
        /// When player joins for the first time, sync all the host settings
        /// with them.
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void RefreshHostConfig(PlayerControllerB __instance)
        {
            Plugin.localID = __instance.playerClientId;
            LMConfig.SetHostConfigs((int)Plugin.localID);
        }




        /// <summary>
        /// Disable Lever if there's less than 4 players in lobby
        /// </summary>
        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.BeginHoldingInteractOnLever))]
        [HarmonyPostfix]
        private static void StopLeverInteraction()
        {
            NotEnoughPlayers();

            if (gameStarted)
            {
                Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                HUDManager.Instance.DisplayTip("Not so fast!", "Complete your objective.", isWarning: true);

            }
        }



        private static void NotEnoughPlayers()
        {
            if (Plugin.inTestMode && Plugin.FoundThisMod("zeebloTesting.zeeblo.dev") && Plugin.isHost) return;

            if (StartOfRound.Instance.ClientPlayerList.Keys.ToArray().Length < 4)
            {
                Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                HUDManager.Instance.DisplayTip("Not Enough Players!", "You need at least 4 players to start the game.", isWarning: true);

            }

        }




        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPrefix]
        private static void SubscribeToHandler()
        {
            Plugin.netHandler.AddCustomNetEvents();

            if (Plugin.isHost)
            {
                Plugin.netHandler.currentMapReceive($"game_started/{CustomLvl.localCurrentInside}", 0);
                //SetHostConfigs();

            }
        }



        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPostfix]
        private static void Begin()
        {
            LMConfig.defaultMeetingTime = defaultDiscussTime.Value + defaultVoteTime.Value + 15f;
            Plugin.terminal.groupCredits = 0;
            Plugin.localPlayer = GameNetworkManager.Instance.localPlayerController;
            Plugin.firedText = "Ejected";
            Plugin.firedTextSub = "";
            EndGame.winCondition = false;
            EndGame.lastPlayersAlive.Clear();
            EndGame.killedByNote.Clear();
            Meeting.MeetingNum = hostMeetingNum;
            EndGame.monsterNames = "";


            Plugin.ResetVariables();
            MinimapUI.CreateMapIcon();
            Tasks.AppendScraps();
            Roles.AssignRole();
            CharacterDisplay.BlackVision(true);
            BuyItems.SetItemPrices();
            BuyItems.HideItems();

            if (Plugin.FoundThisMod("imabatby.lethallevelloader"))
            {
                Plugin.mls.LogInfo(">>> In Begin() method & found LLL");
                StartOfRound.Instance.StartCoroutine(LLLFound());
            }
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void IntroScreen()
        {
            Plugin.mls.LogInfo(">>> in IntroScreen");
            CharacterDisplay.inIntro = true;
            StartOfRound.Instance.StartCoroutine(CharacterDisplay.IntroDisplay());
        }



        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence))]
        [HarmonyPostfix]
        private static void SpawnHornPatch(StartOfRound __instance)
        {
            SpawnHorn();
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPrefix]
        private static bool GracePeriod()
        {
            if (StringAddons.ConvertToBool(inGracePeriod.Value) || localGracePeriod)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static bool StopDeath()
        {
            if (EjectPlayers.notsafe) return true;
            if (StringAddons.ConvertToBool(inGracePeriod.Value) || localGracePeriod) return false;

            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void GracePeriodTime()
        {
            if (StartOfRound.Instance.inShipPhase) return;

            if (StringAddons.ConvertToBool(inGracePeriod.Value) || localGracePeriod)
            {
                // Countdown from the set grace period time
                if (StringAddons.ConvertToFloat(currentGracePeriodTime.Value) >= 0 && Plugin.isHost)
                {
                    float countdown = StringAddons.ConvertToFloat(currentGracePeriodTime.Value);
                    countdown -= 1 * Time.deltaTime;
                    currentGracePeriodTime.Value = $"{countdown}";
                }
                else
                {

                    if (Plugin.isHost)
                    {
                        inGracePeriod.Value = "false";
                    }
                    localGracePeriod = false;

                }

                HUDManager.Instance.Clock.targetAlpha = 0f; // Hide clock GUI

                // Show GUI that displays the grace period time
                if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false && (StringAddons.ConvertToBool(inGracePeriod.Value) || localGracePeriod))
                {
                    HUDManager.Instance.loadingText.enabled = true;
                    HUDManager.Instance.loadingText.text = $"Grace Period: {(int)StringAddons.ConvertToFloat(currentGracePeriodTime.Value)}";
                }
            }

        }

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        private static void LMChatMsg()
        {
            Commands.DisplayChatMessage($"<color=#FF0000>Lethal Mystery</color> v{Plugin.modVersion}-alpha");
        }


        #region Chat Command


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void SetIsHost()
        {
            Plugin.mls.LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost);
            Plugin.isHost = RoundManager.Instance.NetworkManager.IsHost;
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
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


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            Plugin.currentRound = RoundManager.Instance;

            return true;
        }


        #endregion Chat Commands



        [HarmonyPatch(typeof(HangarShipDoor), "Start")]
        [HarmonyPostfix]
        private static void SampleSceneObjects()
        {
            Plugin.shipInstance = GameObject.Find("Environment/HangarShip");
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void ScrapSpawningTimer()
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (!Plugin.isHost) return;
            if (startSpawningScraps)
            {

                scrapTimer -= 1 * Time.deltaTime;

                if (scrapTimer <= 0)
                {
                    System.Random randomNum = new System.Random();
                    int index = randomNum.Next(0, Tasks.allScraps.ToArray().Length);
                    string scrapName = Tasks.allScraps[index].ToLower();
                    Commands.SpawnScrapFunc(scrapName);
                    scrapTimer = hostScrapTimer;
                }

            }
        }


        private static void SpawnHorn()
        {
            if (!Plugin.isHost) return;
            MethodInfo UnlockShipObject = typeof(StartOfRound).GetMethod("UnlockShipObject", BindingFlags.NonPublic | BindingFlags.Instance);
            UnlockShipObject.Invoke(StartOfRound.Instance, new object[] { 18 });
        }

        public static void InfiniteTime()
        {
            float currentDayTime = 850f;
            //float normalizedTimeOfDay = currentDayTime / TimeOfDay.Instance.totalTime;
            float normalizedTimeOfDay = currentDayTime / 1080;
            TimeOfDay.Instance.currentDayTime = currentDayTime;
            TimeOfDay.Instance.sunAnimator.SetFloat("timeOfDay", Mathf.Clamp(normalizedTimeOfDay, 0f, 0.78f));
        }




        /// <summary>
        /// Things to run since LLL stops certain methods from being
        /// patched
        /// </summary>
        private static IEnumerator LLLFound()
        {
            yield return new WaitForSeconds(2f);
            SpawnHorn();

            CharacterDisplay.inIntro = true;
            StartOfRound.Instance.StartCoroutine(CharacterDisplay.IntroDisplay());
            InsideMap.TPDungeon();
        }



    }
}
