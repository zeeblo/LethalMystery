using HarmonyLib;
using LethalMystery.Players;
using UnityEngine;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {


        [HarmonyPatch(nameof(GrabbableObject.Start))]
        [HarmonyPostfix]
        private static void StartPatch(GrabbableObject __instance)
        {
            if (__instance.itemProperties.twoHanded)
            {
                __instance.itemProperties.twoHanded = false;
            }
            if (Roles.CurrentRole != null)
            {
                if (__instance.itemProperties.name.Contains(Roles.CurrentRole.GetItem()) && Roles.CurrentRole.GetItem() != "")
                {
                    __instance.itemProperties.weight = 1f;
                    return;
                }
            }


            //Plugin.mls.LogInfo($">>> Item: {__instance.itemProperties.itemName} | weight: {__instance.itemProperties.weight}");
            //Plugin.mls.LogInfo($"\n Player Weight: {Plugin.localPlayer.carryWeight}");

            __instance.itemProperties.weight = 1.05f;
        }

        [HarmonyPatch(nameof(GrabbableObject.LateUpdate))]
        [HarmonyPostfix]
        private static void HideScrapsOnMonitorPatch(ref Transform ___radarIcon)
        {
            if (___radarIcon != null)
            {
                ___radarIcon.gameObject.SetActive(false);
            }
        }

    }
}
