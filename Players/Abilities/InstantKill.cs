using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.Players.Abilities
{

    [HarmonyPatch]
    internal class InstantKill
    {
        public static bool killedPlayer = false;
        public static float killCooldown = 0;

        public static void ResetVars()
        {
            killedPlayer = false;
            killCooldown = 0;
        }

        public enum killType
        {
            None,
            player,
            self
        }


        #region Sheriff 


        public static killType killMonster(UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {

            Ray ray = new Ray(shotgunPosition, shotgunForward);
            if (Physics.Raycast(ray, out var hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndPlayers, QueryTriggerInteraction.Ignore))
            {

                if (hitInfo.collider == null) return killType.None;
                if (hitInfo.collider.transform.gameObject.GetComponent<PlayerControllerB>() == null) return killType.None;
                PlayerControllerB targetPlayer = hitInfo.collider.transform.gameObject.GetComponent<PlayerControllerB>();


                Plugin.mls.LogInfo($">>> Shot at {hitInfo.collider.transform.gameObject.name}");
                if (targetPlayer.playerClientId == Plugin.localPlayer.playerClientId) return killType.None;
                if (Roles.NameIsMonsterType(Roles.localPlayerRoles[targetPlayer.playerClientId]))
                {
                    return killType.player;
                }
                else
                {
                    Plugin.localPlayer.DamagePlayer(999);
                }

                Plugin.netHandler.deathInfoReceive($"killedby/{targetPlayer.playerClientId}/Shot by: {Plugin.localPlayer.playerUsername}", Plugin.localPlayer.playerClientId);

                return killType.self;


            }
            return killType.None;
        }


        #endregion Sheriff




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
                    killCooldown = 25f; //LMConfig.defaultKillCooldown (should inherit from host);

                    StartOfRound.Instance.StartCoroutine(knifeCooldown());
                    Plugin.netHandler.deathInfoReceive($"killedby/{plrID}/Stabbed by: {Plugin.localPlayer.playerUsername}", Plugin.localPlayer.playerClientId);

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


        private static IEnumerator knifeCooldown()
        {
            yield return new WaitForSeconds(Start.hostKillCooldown);
            HUDManager.Instance.DisplayTip("Kill Cooldown", "Your cooldown is over!", isWarning: false);
            killCooldown = 0;
            killedPlayer = false;
        }

        /*
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
        */


        #endregion Knife Instakill


    }
}
