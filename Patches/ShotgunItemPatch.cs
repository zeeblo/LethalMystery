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
        private static void InfiniteBullets(ShotgunItem __instance, ref int ___shellsLoaded, Vector3 shotgunPosition, Vector3 shotgunForward)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                ___shellsLoaded = 2;

                killMonster(__instance, shotgunPosition: shotgunPosition, shotgunForward: shotgunForward);
            }
            
        }


        private static void killMonster(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward)
        {
            Ray ray = new Ray(shotgunPosition, shotgunForward);
            if (Physics.Raycast(ray, out var hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider != null)
                {
                    Plugin.mls.LogInfo($">>> Just Shot at {hitInfo.collider.transform.gameObject.name}");
                }
            }
        }
    }
}
