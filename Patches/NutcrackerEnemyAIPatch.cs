using HarmonyLib;
using LethalMystery.Maps;
using UnityEngine;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {


        [HarmonyPatch(nameof(NutcrackerEnemyAI.SpawnShotgunShells))]
        [HarmonyPrefix]
        private static bool NoBulletObjects()
        {
            return false;
        }

        [HarmonyPatch(nameof(NutcrackerEnemyAI.GrabGun))]
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
