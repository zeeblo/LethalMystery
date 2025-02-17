using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Maps;
using LethalMystery.Utils;
using UnityEngine;


namespace LethalMystery.Players
{

    [HarmonyPatch]
    internal class Ability
    {
        public static bool killedPlayer = false;
        public static float killCooldown = 0;
        public static float cleaningBloodAmt = 0;

        public static void ResetVars()
        {
            killedPlayer = false;
            killCooldown = 0;
            cleaningBloodAmt = 0;
        }



        #region Knife Instakill

        [HarmonyPatch(typeof(KnifeItem), nameof(KnifeItem.HitKnife))]
        [HarmonyPrefix]
        private static bool KnifeKill(KnifeItem __instance, ref int ___knifeHitForce)
        {

            List<ulong> killList = KnifeHitList(__instance);
            if (Roles.CurrentRole?.Type != Roles.RoleType.monster) return true;
            if (killList.Count <= 0) return true;

            if (killCooldown > 0)
            {
                HUDManager.Instance.DisplayTip("Kill Cooldown!", "Can't kill someone right now.", isWarning: true);
            }

            foreach (ulong plrID in killList)
            {
                if (!Roles.NameIsMonsterType(Roles.localPlayerRoles[plrID]) && killedPlayer == false)
                {
                    ___knifeHitForce = 9999;
                    Plugin.netHandler.playerBloodReceive($"{Plugin.localPlayer.playerClientId}/blood", Plugin.localPlayer.playerClientId);
                    killedPlayer = true;
                    killCooldown = LMConfig.defaultKillCooldown;

                    HUDManager.Instance.DisplayTip("Remove Blood!", $"Hold \"{LMConfig.selfcleanBind.Value.ToUpper()}\" to clean yourself", isWarning: true);
                    Commands.DisplayChatMessage($"Hold <color=#FF0000>\"{LMConfig.selfcleanBind.Value.ToUpper()}\"</color> to clean yourself");
                    return true;
                }
            }

            ___knifeHitForce = 1;
            return true;
        }


        private static List<ulong> KnifeHitList(KnifeItem __instance)
        {
            List<RaycastHit> objectsHitByKnifeList = new List<RaycastHit>();
            List<ulong> killList = new List<ulong>();
            RaycastHit[] objectsHitByKnife;
            int knifeMask = 1084754248;

            objectsHitByKnife = Physics.SphereCastAll(__instance.previousPlayerHeldBy.gameplayCamera.transform.position + __instance.previousPlayerHeldBy.gameplayCamera.transform.right * 0.1f, 0.3f, __instance.previousPlayerHeldBy.gameplayCamera.transform.forward, 0.75f, knifeMask, QueryTriggerInteraction.Collide);
            objectsHitByKnifeList = objectsHitByKnife.OrderBy((RaycastHit x) => x.distance).ToList();
            foreach (RaycastHit i in objectsHitByKnife)
            {
                PlayerControllerB player = i.transform.gameObject.GetComponent<PlayerControllerB>();
                if (player != null)
                {
                    if (Plugin.localPlayer.playerUsername != player.playerUsername)
                    {
                        killList.Add(player.playerClientId);
                    }
                }


            }
            return killList;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void killCooldownFunc()
        {
            if (!killedPlayer) return;

            killCooldown -= 1 * Time.deltaTime;
            if (killCooldown <= 0f)
            {
                HUDManager.Instance.DisplayTip("Kill Cooldown Reset!", "You can now attack someone", false);
                killedPlayer = false;
            }
        }


        #endregion Knife Instakill



        #region Hold To Clean

        private static void HoldToClean()
        {
            cleaningBloodAmt += 1f * Time.deltaTime;
            float timeToHold = 4f;
            HUDManager.Instance.holdFillAmount = cleaningBloodAmt;
            HUDManager.Instance.holdInteractionFillAmount.fillAmount = cleaningBloodAmt / timeToHold;

            if (cleaningBloodAmt > timeToHold)
            {
                Controls.StopCleaning();
                //Plugin.localPlayer.RemoveBloodFromBody();
                Plugin.netHandler.playerBloodReceive($"{Plugin.localPlayer.playerClientId}/clean", Plugin.localPlayer.playerClientId);
                Plugin.localPlayer.movementAudio.PlayOneShot(StartOfRound.Instance.changeSuitSFX);
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch()
        {
            if (Controls.cleaningBody)
            {
                HoldToClean();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StopHoldInteractionOnTrigger))]
        [HarmonyPrefix]
        private static bool StopHoldInteractionOnTriggerPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ClickHoldInteraction))]
        [HarmonyPrefix]
        private static bool ClickHoldInteractionPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPrefix]
        private static bool StopInteractionPatch()
        {
            if (Controls.cleaningBody)
            {
                return false;
            }
            return true;
        }


        #endregion Hold To Clean



        #region Enter Vent


        public static bool isInVent = false;
        public static float currentRotationX = 180f;
        private static GameObject ventCamera;


        public class LM_Vent : MonoBehaviour
        {
            public int thisIndex;
            public string parent;


            void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.GetComponent<Shovel>() != null)
                {
                    Plugin.mls.LogInfo(">>> (!) AAAAA Hit by shovel (!)");
                }

                if (other.gameObject != null)
                {
                    Plugin.mls.LogInfo($">>> (?) intered with: {other.gameObject.name}(?)");
                }
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.LateUpdate))]
        [HarmonyPostfix]
        private static void VentChecks(PlayerControllerB __instance)
        {
            if (isInVent)
            {
                if (ventCamera == null)
                {
                    ventCamera = GameObject.Find("LM_ventCamera");
                }
                float minRotation = 180 - 20;
                float maxRotation = 180 + 20;
                float mouseX = __instance.playerActions.Movement.Look.ReadValue<Vector2>().x * 0.008f * IngamePlayerSettings.Instance.settings.lookSensitivity;

                float newRotation = currentRotationX + mouseX;
                newRotation = Mathf.Clamp(newRotation, minRotation, maxRotation);

                float rotationFix = newRotation - currentRotationX;
                ventCamera.transform.Rotate(0f, rotationFix, 0f);

                currentRotationX = newRotation;

            }
        }


        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPostfix]
        private static void EnterVentPatch(InteractTrigger __instance)
        {

            if (__instance.currentCooldownValue > 0f)
            {
                EnterVent();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Interact_performed))]
        [HarmonyPostfix]
        private static void ExitVentPatch()
        {
            if (isInVent)
            {
                ExitVent();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ScrollMouse_performed))]
        [HarmonyPrefix]
        private static bool ScrollPatch()
        {
            if (isInVent)
            {
                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.CanUseItem))]
        [HarmonyPrefix]
        private static bool CanUseItemPatch()
        {
            if (isInVent)
            {
                return false;
            }
            return true;
        }


        private static void CreateVentCamera()
        {
            Camera ventCamera = new GameObject("LM_ventCamera").AddComponent<Camera>();
            ventCamera.tag = "PhysicsProp";

            GameObject raw_vent = GOTools.GetObjectPlayerIsLookingAt();
            string ventName = raw_vent.name;
            string ventParentName = raw_vent.transform.parent.name;

            GameObject vent = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{ventName}");
            GameObject ventPoint = GameObject.Find($"{CustomLvl.CurrentInside.name}(Clone)/vents/{ventParentName}/{ventName}/point");

            Vector3 ventpos = new Vector3(ventPoint.transform.position.x, ventPoint.transform.position.y, ventPoint.transform.position.z);
            ventCamera.transform.position = ventpos;


            ventCamera.transform.LookAt(vent.transform);
            ventCamera.transform.Rotate(0, 180, 0);
            ventCamera.cullingMask = GameNetworkManager.Instance.localPlayerController.gameplayCamera.cullingMask;

            Canvas canv = GameObject.Find("Systems/UI/Canvas/").GetComponent<Canvas>();
            canv.renderMode = 0;
            canv.worldCamera = ventCamera;
            StartOfRound.Instance.SwitchCamera(ventCamera);
        }

        private static void RemoveVentCamera()
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("PhysicsProp"))
            {
                if (obj.name == "LM_ventCamera")
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

        private static void EnterVent()
        {
            if (!(GOTools.GetObjectPlayerIsLookingAt().name.ToLower().Contains("vent"))) return;
            if (GameObject.Find("LM_ventCamera") != null) return;
            Plugin.mls.LogInfo(">>> Entered vent");
 
            GameObject.Find("Systems/UI/Canvas/Panel/")?.SetActive(false);

            CreateVentCamera();

            Plugin.localPlayer.thisPlayerBody.transform.Rotate(0, 180, 0);
            Plugin.localPlayer.disableMoveInput = true;
            GOTools.HidePlayerModel();
            Plugin.netHandler.hidePlayerReceive($"{Plugin.localPlayer.playerClientId}/hide", Plugin.localPlayer.playerClientId);

            HUDManager.Instance.PlayerInfo.targetAlpha = 0;
            HUDManager.Instance.Tooltips.targetAlpha = 0;
            HUDManager.Instance.Inventory.targetAlpha = 0;
            isInVent = true;
        }


        private static void ExitVent()
        {
            Plugin.mls.LogInfo(">>> Exited vent");
            RemoveVentCamera();
            GameObject.Find("Systems/UI/Canvas/Panel/").SetActive(true);
            Plugin.localPlayer.disableMoveInput = false;
            GOTools.HidePlayerModel(false);
            Plugin.netHandler.hidePlayerReceive($"{Plugin.localPlayer.playerClientId}/show", Plugin.localPlayer.playerClientId);


            HUDManager.Instance.PlayerInfo.targetAlpha = 1;
            HUDManager.Instance.Tooltips.targetAlpha = 1;
            HUDManager.Instance.Inventory.targetAlpha = 1;

            isInVent = false;
        }


        #endregion Enter Vent
    }
}
