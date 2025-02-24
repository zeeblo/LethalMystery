using System.Collections;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using LethalMystery.Players;
using LethalMystery.Patches;
using LethalMystery.Utils;
using LethalMystery.Maps;
using LethalMystery.UI;

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


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.LateUpdate))]
        [HarmonyPostfix]
        private static void HideWeaponsPatch()
        {
            if (inIntro)
            {
                HideWeapons();
            }
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
                if (!Roles.NameIsMonsterType(id.Value)) continue;

                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (id.Key == player.playerClientId)
                    {
                        player.usernameBillboardText.color = Color.red;
                    }
                }
            }


        }

        private static void ResetMonsterNames()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                player.usernameBillboardText.color = Color.white;
            }
        }


        /// <summary>
        /// Hide every weapon
        /// </summary>
        private static void HideWeapons()
        {
            GameObject[] go = GameObject.FindGameObjectsWithTag("PhysicsProp");

            foreach (GameObject obj in go)
            {
                string go_name = obj.name.ToLower().Replace("(clone)", "");
                if (go_name.Contains("knife") || go_name.Contains("gun"))
                {
                    GrabbableObject weapon = obj.GetComponent<GrabbableObject>();

                    if ((weapon.playerHeldBy != null) && Plugin.localPlayer.playerClientId == weapon.playerHeldBy.playerClientId)
                    {
                        obj.GetComponent<MeshRenderer>().enabled = true;
                    }
                    else
                    {
                        obj.GetComponent<MeshRenderer>().enabled = false;
                    }

                }
            }
        }







        private static void ReverseRotation()
        {
            GameNetworkManager.Instance.localPlayerController.thisPlayerBody.transform.Rotate(0, 180, 0);
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
            InsideMap.SpawnInterior();
            InsideMap.SpawnVents();
            InsideMap.SpawnScrapScanPositions();
            InsideMap.MakeLMDoorInteractive();
            InsideMap.SetMinimapLayer();
            Minimap.waypointPrefab = Plugin.localPlayer.transform.Find("Misc/MapDot").gameObject;


            Plugin.enemyVent = UnityEngine.Object.FindObjectOfType<EnemyVent>();
            Plugin.netHandler.SpawnWeaponReceive($"{Roles.CurrentRole.Type}/{Roles.CurrentRole.Name}", Plugin.localPlayer.playerClientId);
            yield return new WaitForSeconds(1.5f);
            
            ShowPlayers(false);
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(modelPosition);
            TimeOfDay.Instance.currentDayTimeStarted = false;

            //Plugin.RemoveEnvironment(true);
            GOTools.HideEnvironment(true);
            GOTools.HideVanillaDungeon();
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
            //GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            //Plugin.RemoveEnvironment(false);
            if (CustomLvl.mapName.Value == "lll_map")
            {
                GOTools.HideVanillaDungeon(false);
            }
            GOTools.HideEnvironment(true, ignore: "Lighting"); // re-enables lighting
            InsideMap.TeleportInside();
            EnvironmentLight(true);
            ShowLights(false);

            DisableIntroCamera();

            Roles.ShowRole(Roles.CurrentRole);
            if (Roles.CurrentRole.Type == Roles.RoleType.monster)
            {
                Controls.monsterControls.Enable();
            }
            Controls.playerControls.Enable();


            inIntro = false;
            Start.startSpawningScraps = true;

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
            GOTools.CheckForWeaponInInventoryNotif();
            Start.InfiniteTime();
            ReverseRotation();
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
