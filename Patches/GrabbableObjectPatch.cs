using HarmonyLib;
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
