using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Maps.Sabotages
{
    [HarmonyPatch]
    internal class Generator
    {

        [HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.StopInteraction))]
        [HarmonyPostfix]
        private static void FixGeneratorPatch(InteractTrigger __instance)
        {

            if (__instance.currentCooldownValue > 0f)
            {
                GameObject fog = GameObject.Find("Systems/Rendering/SpookyFog");
                if (fog != null)
                {
                    fog.SetActive(false);
                }
            }
        }

    }
}
