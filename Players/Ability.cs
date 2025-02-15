using GameNetcodeStuff;
using HarmonyLib;
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
                float sensitivity = 10f;
                float minRotation = 180 - 20; // -60
                float maxRotation = 180 + 20; // -100
                //float mouseX = __instance.gameplayCamera.transform.localRotation.x; yes
                //float mouseX = __instance.gameplayCamera.transform.localRotation.y; no
                //float mouseX = __instance.gameplayCamera.transform.localRotation.z; no
                //float mouseX = __instance.gameplayCamera.transform.localEulerAngles.y; no
                //float mouseX = __instance.gameplayCamera.transform.localEulerAngles.x; //eh
                //float mouseX = __instance.gameplayCamera.transform.localEulerAngles.z; mo
                //float mouseX = (float)Traverse.Create(__instance).Field("targetLookRot").GetValue(); no
                //float mouseX = __instance.playerGlobalHead.transform.position.x;
                float mouseX = __instance.playerActions.Movement.Look.ReadValue<Vector2>().x * 0.008f * IngamePlayerSettings.Instance.settings.lookSensitivity;
                Plugin.mls.LogInfo($">>> mouseX: {mouseX}");

                //currentRotationX = mouseX;
                currentRotationX += mouseX;
                currentRotationX = Mathf.Clamp(currentRotationX, minRotation, maxRotation);

                Plugin.mls.LogInfo($">>> currentRotationX: {currentRotationX}");

                ventCamera.transform.Rotate(0f, currentRotationX, 0f);

            }
        }



        #endregion Enter Vent
    }
}
