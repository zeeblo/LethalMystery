using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using LethalMystery.Players;
namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(typeof(ShotgunItem), nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ref int ___shellsLoaded)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "Sherif")
            {
                ___shellsLoaded = 2;
            }
            
        }
    }
}
