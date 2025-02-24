using System.Numerics;
using HarmonyLib;
using LethalMystery.Players;
using UnityEngine;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ShotgunItem __instance, ref int ___shellsLoaded, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                ___shellsLoaded = 2;

                killMonster(__instance, shotgunPosition: shotgunPosition, shotgunForward: shotgunForward);
            }
            
        }


        private static void killMonster(ShotgunItem __instance, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {

            Ray ray = new Ray(shotgunPosition, shotgunForward);
            /*
            if (Physics.Raycast(ray, out var hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
            }
            */
            if (Physics.Raycast(ray, out var hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndPlayers, QueryTriggerInteraction.Ignore))
            {
                Plugin.mls.LogInfo($">>> Shot at {hitInfo.collider.transform.gameObject.name}");

            }

            /*
            ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);
            RaycastHit[] playerCollider = new RaycastHit[10];

            int num3 = Physics.SphereCastNonAlloc(ray, 5f, playerCollider, 15f, 524288, QueryTriggerInteraction.Collide);
            List<PlayerControllerBPatch> list = new List<PlayerControllerBPatch>();
            for (int i = 0; i < num3; i++)
            {
                if (!enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>())
                {
                    continue;
                }
                /*

                IHittable component;
                if (enemyColliders[i].distance == 0f)
                {
                    Plugin.mls.LogInfo(">>> Spherecast started inside enemy collider");
                }
                else if (!Physics.Linecast(shotgunPosition, playerCollider[i].point, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && enemyColliders[i].transform.TryGetComponent<IHittable>(out component))
                {
                    Plugin.mls.LogInfo($">>> Shot at {hitInfo.collider.transform.gameObject.name}");

                    
                    float num4 = UnityEngine.Vector3.Distance(shotgunPosition, enemyColliders[i].point);
                    int force = ((num4 < 3.7f) ? 5 : ((!(num4 < 6f)) ? 2 : 3));
                    EnemyAICollisionDetect component2 = enemyColliders[i].collider.GetComponent<EnemyAICollisionDetect>();
                    if ((!(component2 != null) || (!(component2.mainScript == null) && !list.Contains(component2.mainScript))) && component.Hit(force, shotgunForward, __instance.playerHeldBy, playHitSFX: true) && component2 != null)
                    {
                        list.Add(component2.mainScript);
                    }
                    
                }
            }
            */
        }
    }
}
