using HarmonyLib;

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
