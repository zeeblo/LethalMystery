using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class InsideMap
    {

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool EnterFacility()
        {


            return false;
        }
    }
}
