using System.Collections;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using LethalMystery.Players;
using LethalMystery.Patches;

namespace LethalMystery.GameMech
{
    [HarmonyPatch]
    public class CharacterDisplay
    {

        private static Vector3 modelPosition = new Vector3(-80f, 20f, 20f);
        private static Vector3 groundPosition = new Vector3(-80f, 18.8f, 20f);
        private static Vector3 cameraPosition = new Vector3(-80f, 31.05f, 26f);
        private static Vector3 lightPosition = new Vector3(-80f, 34f, 20f);
        private static GameObject? lght;
        //private static Camera? introCamera;
        private static GameObject? sphere;
        public static bool inIntro = false;




        #region patches

        [HarmonyPatch(typeof(PlayerControllerB), "PlayerLookInput")]
        [HarmonyPrefix]
        private static bool PlayerLookPatch()
        {
            if (inIntro)
            {
                return false;
            }
            return true;
        }


        private static IEnumerator RemoveMoonInfo(StartOfRound __instance)
        {
            __instance.StartNewRoundEvent.Invoke();
            yield return new WaitForSeconds(1f);
            HUDManager.Instance.LevellingAudio.Stop();
            StartMatchLever leverScript = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
            leverScript.triggerScript.timeToHold = 0.7f;
            leverScript.triggerScript.interactable = false;
            __instance.displayedLevelResults = false;
            __instance.StartTrackingAllPlayerVoices();
            if (!GameNetworkManager.Instance.gameHasStarted)
            {
                GameNetworkManager.Instance.LeaveLobbyAtGameStart();
                GameNetworkManager.Instance.gameHasStarted = true;
            }
            UnityEngine.Object.FindObjectOfType<QuickMenuManager>().DisableInviteFriendsButton();
            if (!GameNetworkManager.Instance.disableSteam)
            {
                GameNetworkManager.Instance.SetSteamFriendGrouping(GameNetworkManager.Instance.steamLobbyName, __instance.connectedPlayersAmount + 1, "Landed on " + __instance.currentLevel.PlanetName);
            }
            __instance.SetDiscordStatusDetails();
            __instance.timeSinceRoundStarted = 0f;
            __instance.shipLeftAutomatically = false;
            __instance.ResetStats();
            __instance.inShipPhase = false;
            __instance.SwitchMapMonitorPurpose();
            __instance.SetPlayerObjectExtrapolate(enable: false);
            __instance.shipAnimatorObject.gameObject.GetComponent<Animator>().SetTrigger("OpenShip");
            if (__instance.currentLevel.currentWeather != LevelWeatherType.None)
            {
                WeatherEffect weatherEffect = TimeOfDay.Instance.effects[(int)__instance.currentLevel.currentWeather];
                weatherEffect.effectEnabled = true;
                if (weatherEffect.effectPermanentObject != null)
                {
                    weatherEffect.effectPermanentObject.SetActive(value: true);
                }
            }
            yield return null;
            yield return new WaitForSeconds(0.2f);
            if (TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None && !__instance.currentLevel.overrideWeather)
            {
                TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
            }
            __instance.shipDoorsEnabled = true;
            if (__instance.currentLevel.planetHasTime)
            {
                TimeOfDay.Instance.currentDayTimeStarted = true;
                TimeOfDay.Instance.movingGlobalTimeForward = true;
            }
            UnityEngine.Object.FindObjectOfType<HangarShipDoor>().SetDoorButtonsEnabled(doorButtonsEnabled: true);
            //__instance.TeleportPlayerInShipIfOutOfRoomBounds();
            yield return new WaitForSeconds(0.05f);
            Debug.Log($"startofround: {__instance.currentLevel.levelID}; {__instance.hoursSinceLastCompanyVisit}");
            if (__instance.currentLevel.levelID == 3 && __instance.hoursSinceLastCompanyVisit >= 0)
            {
                __instance.hoursSinceLastCompanyVisit = 0;
                TimeOfDay.Instance.TimeOfDayMusic.volume = 0.6f;
                Debug.Log("Playing time of day music");
                TimeOfDay.Instance.PlayTimeMusicDelayed(__instance.companyVisitMusic, 1f);
            }
            HUDManager.Instance.loadingText.enabled = false;
            HUDManager.Instance.loadingDarkenScreen.enabled = false;
            //__instance.shipDoorAudioSource.PlayOneShot(__instance.openingHangarDoorAudio, 1f);
            yield return new WaitForSeconds(0.8f);
            __instance.shipDoorsAnimator.SetBool("Closed", value: false);
            yield return new WaitForSeconds(5f);
            yield return new WaitForSeconds(10f);
            if (__instance.currentLevel.spawnEnemiesAndScrap && __instance.currentLevel.planetHasTime)
            {
                HUDManager.Instance.quotaAnimator.SetBool("visible", value: true);
                TimeOfDay.Instance.currentDayTime = TimeOfDay.Instance.CalculatePlanetTime(__instance.currentLevel);
                TimeOfDay.Instance.RefreshClockUI();
            }
            yield return new WaitForSeconds(4f);
            //OnShipLandedMiscEvents();
            __instance.SetPlayerObjectExtrapolate(enable: false);
            __instance.shipHasLanded = true;
            leverScript.triggerScript.animationString = "SA_PushLeverBack";
            leverScript.triggerScript.interactable = true;
            leverScript.hasDisplayedTimeWarning = false;
        }

        [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
        [HarmonyPrefix]
        private static bool openingDoorsSequencePatch(StartOfRound __instance)
        {
            StartOfRound.Instance.StartCoroutine(RemoveMoonInfo(__instance));
            return false;
        }



        #endregion patches


        private static void CreateSphere()
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = groundPosition;
            sphere.transform.localScale = new Vector3(20, 20, 20);
        }

        private static void ShowSphere(bool value)
        {
            if (sphere != null)
            {
                sphere.SetActive(value);
            }
            else
            {
                CreateSphere();
            }
        }

        private static void EnableMovement(bool value)
        {
            if (value)
            {
                MethodInfo OnEnable = typeof(PlayerControllerB).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
                OnEnable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                GameNetworkManager.Instance.localPlayerController.disableMoveInput = false;
            }
            else
            {
                MethodInfo OnDisable = typeof(PlayerControllerB).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
                OnDisable.Invoke(GameNetworkManager.Instance.localPlayerController, null);
                GameNetworkManager.Instance.localPlayerController.disableMoveInput = true;
            }

        }

        /// <summary>
        /// If user is holding a weapon, switch to the next slot.
        /// (This doesn't specifically check if it's a weapon but that's how it's used)
        /// </summary>
        private static void SwitchToNextItem()
        {
            MethodInfo GetFirstEmptyItemSlot = typeof(PlayerControllerB).GetMethod("FirstEmptyItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            int FirstEmptyItemSlot = (int)GetFirstEmptyItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, null);

            MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { FirstEmptyItemSlot, Type.Missing });
        }

        private static void LookAtCamera()
        {
            GameNetworkManager.Instance.localPlayerController.thisPlayerBody.rotation = Quaternion.identity;
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles = new Vector3(0, 0, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.localEulerAngles.z);
        }

        public static void BlackVision(bool value)
        {
            GameObject.Find("Systems/UI/Canvas/Panel/")?.SetActive(!value);
        }

        private static void DisableMainCamera(bool value)
        {
            GameObject.Find("Systems/UI/Canvas/Panel/")?.SetActive(!value); // for some reason allows the below code to work (using the method above doesn't)
            GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/")?.SetActive(!value);
            if (GameNetworkManager.Instance.localPlayerController == null) return;
            GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.enabled = !value;
        }

        private static void ResetAnimation()
        {
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Walking", value: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sprinting", value: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("Sideways", value: false);
            GameNetworkManager.Instance.localPlayerController.Crouch(crouch: false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetBool("FallNoJump", value: false);
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            GameNetworkManager.Instance.localPlayerController.StopPerformingEmoteServerRpc();
        }


        private static void EnvironmentLight(bool light)
        {
            GameObject.Find("Systems/Rendering/VolumeMain")?.SetActive(light);
            GameObject.Find("Environment/OutOfBoundsTrigger1")?.SetActive(light);
            GameObject.Find("StoryLogs")?.SetActive(light);
        }

        private static void CreateLights()
        {
            GameObject AreaLight = GameObject.Find("Area Light (5)");
            if (AreaLight != null)
            {
                Quaternion rot = Quaternion.Euler(90, 0, 0);
                lght = UnityEngine.Object.Instantiate(AreaLight.gameObject, lightPosition, rot);
            }

        }

        private static void ShowLights(bool value)
        {
            if (lght != null)
            {
                lght.gameObject.SetActive(value);
            }
            else
            {
                CreateLights();
            }
        }


        private static void CreateCamera()
        {
            Camera introCamera = new GameObject("LM_IntroCamera").AddComponent<Camera>();
            introCamera.tag = "Player";
            introCamera.transform.position = cameraPosition;
            introCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);
            introCamera.cullingMask = GameNetworkManager.Instance.localPlayerController.gameplayCamera.cullingMask;

            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.renderMode = 0;
            canv.worldCamera = introCamera;
            StartOfRound.Instance.SwitchCamera(introCamera);
        }


        private static void DisableIntroCamera()
        {
            /* "Hacky" way of removing introCamera if multiple
             * happen to spawn instead of 1 for some reason
             */
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (obj.name == "LM_IntroCamera")
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            canv.renderMode = RenderMode.ScreenSpaceCamera;

            if (GameNetworkManager.Instance.localPlayerController != null)
            {
                StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
            }

        }

        private static void ShowPlayers(bool value)
        {

            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                if (user != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    if (user.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId)
                    {
                        user.gameObject.SetActive(value);
                    }
                }

            }

        }


        public static IEnumerator IntroDisplay()
        {
            ShowSphere(true);
            EnableMovement(false);
            LookAtCamera();
            ResetAnimation();
            Commands.SpawnWeapons("all");
            yield return new WaitForSeconds(1.5f);

            ShowPlayers(false);
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(modelPosition);

            Plugin.RemoveEnvironment(true);
            EnvironmentLight(false);
            ShowLights(true);

            yield return new WaitForSeconds(1.5f);
            BlackVision(false);
            DisableMainCamera(true);
            CreateCamera();

            HUDManagerPatch.DisplayDaysEdit("role");
            MoreSlots.AllowMoreSlots();
            //MoreSlots.DisplayMoreSlots();

            yield return new WaitForSeconds(2.35f);
            AutoGiveItem.doneSpawningWeapons = true;
            GameObject.Find("ShotgunItem(Clone)/ScanNode")?.gameObject.SetActive(false); // disable red scan node that's visible to intro cam

            yield return new WaitForSeconds(2f);
            ShowSphere(false);
            EnableMovement(true);
            LookAtCamera();
            ShowPlayers(true);
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            Plugin.RemoveEnvironment(false);
            EnvironmentLight(true);
            ShowLights(false);

            DisableIntroCamera();
            if (Roles.CurrentRole != null)
            {
                Roles.ShowRole(Roles.CurrentRole);

                if (Roles.CurrentRole.Type == Roles.RoleType.monster)
                {
                    Controls.monsterControls.Enable();
                }
            }
            inIntro = false;
            Plugin.inGracePeriod = true;

            yield return new WaitForSeconds(1f);
            SwitchToNextItem();

            GameObject.Find("ShotgunItem(Clone)/ScanNode")?.gameObject.SetActive(true);
            GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD").gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD").gameObject.SetActive(true); // plays spawn animation when enabled
            DisableMainCamera(false);


        }


        public static void ResetVariables()
        {
            StartOfRound.Instance.StopCoroutine(IntroDisplay());
            inIntro = false;
            ShowSphere(false);
            ShowPlayers(true);
            EnvironmentLight(true);
            DisableIntroCamera();
            Plugin.inGracePeriod = false;
            DisableMainCamera(false);

        }

    }
}
