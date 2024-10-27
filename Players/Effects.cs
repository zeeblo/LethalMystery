using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMystery.Players
{
    internal class Effects
    {

        
        [HarmonyPatch(typeof(PlayerControllerB))]
        internal class General
        {
            [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
            [HarmonyPostfix]
            public static void UpdatePatch(PlayerControllerB __instance)
            {
                __instance.takingFallDamage = false;
            }

        }

    }
}
