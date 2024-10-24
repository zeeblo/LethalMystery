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
        private static int amnt = 0;

        [HarmonyPatch(typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.Update))]
        [HarmonyPrefix]
        private static bool UpdatePatch(NutcrackerEnemyAI __instance)
        {
            if (amnt == 0)
            {
                __instance.KillEnemy();
                amnt += 1;
            }
            return true;
        }
    }
}
