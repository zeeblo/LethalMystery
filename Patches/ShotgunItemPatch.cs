using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(typeof(ShotgunItem), nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ref int ___shellsLoaded)
        {
            ___shellsLoaded = 2;
        }
    }
}
