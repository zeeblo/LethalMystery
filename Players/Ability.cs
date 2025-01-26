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

            List<ulong> killList = KnifeHitList(__instance);
            if (Roles.CurrentRole?.Type != Roles.RoleType.monster) return true;
            if (killList.Count <= 0) return true;

            foreach (ulong plrID in killList)
            {
                if (!Roles.NameIsMonsterType(Roles.localPlayerRoles[plrID]) && killedPlayer == false)
                {
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
                PlayerControllerB player = i.transform.gameObject.GetComponent<PlayerControllerB>();
                if (player != null)
                {
                    if (Plugin.localPlayer.playerUsername != player.playerUsername)
                    {
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
