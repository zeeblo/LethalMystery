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
        public static float killCooldown = LMConfig.defaultKillCooldown;

        [HarmonyPatch(typeof(KnifeItem), nameof(KnifeItem.HitKnife))]
        [HarmonyPrefix]
        private static bool KnifeKill(KnifeItem __instance, ref int ___knifeHitForce)
        {
            Plugin.mls.LogInfo(">>> 1");
            List<ulong> killList = KnifeHitList(__instance);
            Plugin.mls.LogInfo(">>> 4");
            if (Roles.CurrentRole?.Type != Roles.RoleType.monster) return true;
            Plugin.mls.LogInfo(">>> 5");
            if (killList.Count <= 0) return true;

            Plugin.mls.LogInfo(">>> in KnifeKill");

            foreach (ulong plrID in killList)
            {
                Plugin.mls.LogInfo(">>> 6");
                if (!Roles.NameIsMonsterType(Roles.localPlayerRoles[plrID]) && killedPlayer == false)
                {
                    Plugin.mls.LogInfo(">>> 7");
                    ___knifeHitForce = 9999;
                    killedPlayer = true;
                    return true;
                }
            }

            
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
                Plugin.mls.LogInfo(">>> 2");
                PlayerControllerB player = i.transform.gameObject.GetComponent<PlayerControllerB>();
                if (player != null)
                {
                    Plugin.mls.LogInfo(">>> 3");
                    if (Plugin.localPlayer.playerUsername != player.playerUsername)
                    {
                        Plugin.mls.LogInfo($">>> Added {player.actualClientId}");
                        killList.Add(player.actualClientId);
                    }
                }


            }
            return killList;
        }

        //[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        //[HarmonyPostfix]
        private static void killCooldownFunc()
        {
            killCooldown -= Time.deltaTime;
            if (killCooldown <= 0f)
            {
                HUDManager.Instance.DisplayTip("Kill Cooldown Reset!", "You can now attack someone", false);
                killedPlayer = false;
            }
        }


    }
}
