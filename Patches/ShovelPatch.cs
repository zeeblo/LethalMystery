using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {

        /*
        [HarmonyPatch(nameof(Shovel.HitShovel))]
        [HarmonyPostfix]
        private static void HitPatch()
        {
            int shovelMask = 1084754248;
            RaycastHit[] objectsHitByShovel = Physics.SphereCastAll(Plugin.localPlayer.gameplayCamera.transform.position + Plugin.localPlayer.gameplayCamera.transform.right * -0.35f, 0.8f, Plugin.localPlayer.gameplayCamera.transform.forward, 1.5f, shovelMask, QueryTriggerInteraction.Collide);
            List<RaycastHit> objectsHitByShovelList = objectsHitByShovel.OrderBy((RaycastHit x) => x.distance).ToList();


            for (int i = 0; i < objectsHitByShovelList.Count; i++)
            {
                Plugin.mls.LogInfo($">>> Just hit: {objectsHitByShovelList[i].transform.gameObject.name}");
            }
        }
        */
    }
}
