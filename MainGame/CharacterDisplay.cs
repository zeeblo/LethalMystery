using System.Collections;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using LethalMystery.Players;
using LethalMystery.Patches;
using static LethalMystery.Players.Roles;
using System.Xml.Linq;
using LethalMystery.Utils;
using Unity.Services.Authentication.Internal;
using TMPro;
using UnityEngine.SceneManagement;


namespace LethalMystery.MainGame
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
        private static GameObject otherUsersItem;




        #region patches

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PlayerLookInput))]
        [HarmonyPrefix]
        private static bool PlayerLookPatch()
        {
            if (inIntro)
            {
                return false;
            }
            return true;
        }


        #endregion patches

        private static void HideGUI(bool value)
        {
            int opacity = (!value) ? 1 : 0;
            HUDManager.Instance.Chat.targetAlpha = opacity;
            HUDManager.Instance.PlayerInfo.targetAlpha = opacity;
            HUDManager.Instance.Tooltips.targetAlpha = opacity;
            HUDManager.Instance.Clock.targetAlpha = opacity;
        }


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
        /// Switches to last slot when giving role specific weapon
        /// then switches to 2nd slot to not show item in hand
        /// </summary>
        private static void SwitchToNextItem(bool lastItem = false)
        {

            MethodInfo SwitchToItemSlot = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo NextItemSlot = typeof(PlayerControllerB).GetMethod("NextItemSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo SwitchItemSlotsServerRpc = typeof(PlayerControllerB).GetMethod("SwitchItemSlotsServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
            if (lastItem == false)
            {

                int GetNextItemSlot = (int)NextItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { true });
                for (int i = 0; i < 1; i++)
                {
                    SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { GetNextItemSlot, Type.Missing });
                    SwitchItemSlotsServerRpc.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { true });
                }
                return;
            }

            int LastItemSlot = GameNetworkManager.Instance.localPlayerController.ItemSlots.Length - 1;
            int slot = (lastItem) ? LastItemSlot : 1;

            SwitchToItemSlot.Invoke(GameNetworkManager.Instance.localPlayerController, new object[] { slot, Type.Missing });



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
                    if (user.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
                    {
                        user.gameObject.SetActive(value);
                    }
                }

            }

        }


        private static void TurnMonsterNamesRed()
        {
            if (Roles.CurrentRole == null) return;
            if (Roles.CurrentRole.Type != Roles.RoleType.monster) return;


            foreach (KeyValuePair<ulong, string> id in Roles.localPlayerRoles)
            {
                if (NameIsMonsterType(id.Value))
                {
                    int playerID = GOTools.UlongToPlayerID(id.Key);
                    if (playerID != -1)
                    {
                        Plugin.mls.LogInfo(">>> attempting to set name to red");
                        FindMonsterName(playerID);
                    }
                }
            }
        }


        private static void FindMonsterName(int playerID, bool reset = false)
        {
            Scene targetScene = SceneManager.GetSceneByName("SampleSceneRelay");
            string PlayerString = (playerID == 0) ? "Player" : $"Player ({playerID})";
            string usernameCanvas = (playerID == 0) ? "PlayerNameCanvas" : "PlayerUsernameCanvas";

            foreach (GameObject obj in targetScene.GetRootGameObjects())
            {
                if (obj.name == "Environment")
                {
                    Transform username = obj.transform.Find($"HangarShip/{PlayerString}/{usernameCanvas}/Text (TMP)");
                    if (username == null) return;

                    TextMeshProUGUI colorChange = username.gameObject.GetComponent<TextMeshProUGUI>();
                    colorChange.color = Color.red;

                    if (reset)
                    {
                        colorChange.color = Color.white;
                    }
                    return;

                }
            }

            foreach (GameObject obj in targetScene.GetRootGameObjects())
            {
                if (obj.name == "PlayersContainer")
                {
                    Transform username = obj.transform.Find($"{PlayerString}/{usernameCanvas}/Text (TMP)");
                    if (username == null) return;

                    TextMeshProUGUI colorChange = username.gameObject.GetComponent<TextMeshProUGUI>();
                    colorChange.color = Color.red;

                    if (reset)
                    {
                        colorChange.color = Color.white;
                    }
                }
            }
        }

        private static void ResetMonsterNames()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                int playerID = (int)player.playerClientId;
                FindMonsterName(playerID, reset: true);
            }
        }



        public static IEnumerator IntroDisplay()
        {
            Roles.CurrentRole = Plugin.netHandler.GetallPlayerRoles();

            MoreSlots.SlotAmountForServer();
            HideGUI(true);
            ShowSphere(true);
            EnableMovement(false);
            LookAtCamera();
            ResetAnimation();

            Plugin.netHandler.SpawnWeaponReceive($"{Roles.CurrentRole.Type}/{Roles.CurrentRole.Name}", Plugin.localPlayer.playerClientId);
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

            yield return new WaitForSeconds(2.35f);

            SwitchToNextItem(lastItem: true);
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

            Roles.ShowRole(Roles.CurrentRole);
            if (Roles.CurrentRole.Type == Roles.RoleType.monster)
            {
                Controls.monsterControls.Enable();
            }

            inIntro = false;

            if (Plugin.isHost)
            {
                Start.inGracePeriod.Value = "true";
            }


            SwitchToNextItem(lastItem: false);
            yield return new WaitForSeconds(1f);


            GameObject.Find("ShotgunItem(Clone)/ScanNode")?.gameObject.SetActive(true);
            GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD").gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD").gameObject.SetActive(true); // plays spawn animation when enabled
            DisableMainCamera(false);
            HideGUI(false);
            TurnMonsterNamesRed();
        }


        public static void ResetVariables()
        {
            StartOfRound.Instance.StopCoroutine(IntroDisplay());
            inIntro = false;
            ShowSphere(false);
            ShowPlayers(true);
            EnvironmentLight(true);
            DisableIntroCamera();
            DisableMainCamera(false);
            ResetMonsterNames();
            if (Plugin.isHost)
            {
                Start.inGracePeriod.Value = "false";
            }

        }

    }
}
