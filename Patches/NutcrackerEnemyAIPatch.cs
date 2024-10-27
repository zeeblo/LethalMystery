using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {
        public static bool spawnedNutForWeapon = false;

        [HarmonyPatch(typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.Update))]
        [HarmonyPrefix]
        private static bool UpdatePatch(NutcrackerEnemyAI __instance)
        {
            if (spawnedNutForWeapon == false)
            {
                __instance.KillEnemy();
                spawnedNutForWeapon = true;
            }
            return true;
        }

    }
}
