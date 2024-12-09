using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ButlerEnemyAI))]
    internal class ButlerEnemyAIPatch
    {
        public static bool spawnedButlerForKnife = false;

        [HarmonyPatch(nameof(ButlerEnemyAI.Update))]
        [HarmonyPrefix]
        private static bool UpdatePatch(ButlerEnemyAI __instance)
        {
            if (spawnedButlerForKnife == false)
            {
                __instance.KillEnemy();
                spawnedButlerForKnife = true;
            }
            return true;
        }

    }
}
