using HarmonyLib;
using LethalMystery.Maps;
using UnityEngine;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {


        [HarmonyPatch("SpawnShotgunShells")]
        [HarmonyPrefix]
        private static bool NoBulletObjects()
        {
            return false;
        }

        [HarmonyPatch("GrabGun")]
        [HarmonyPrefix]
        private static bool GrabGunPatch()
        {
            return false;
        }


        /*
        [HarmonyPatch(nameof(NutcrackerEnemyAI.FireGunServerRpc))]
        [HarmonyPrefix]
        private static bool FireGunServerRpcPatch()
        {
            return false;
        }
        */

    }
}
