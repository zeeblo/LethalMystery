using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {

        [HarmonyPatch(typeof(GrabbableObject), "LateUpdate")]
        [HarmonyPostfix]
        private static void HideWeaponOnMonitorPatch(ref Transform ___radarIcon)
        {
            if (___radarIcon != null)
            {
                ___radarIcon.gameObject.SetActive(false);
            }
        }
    }
}
