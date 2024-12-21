using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ButlerEnemyAI))]
    internal class ButlerEnemyAIPatch
    {
        public static bool spawnedButlerForKnife = false;

        [HarmonyPatch(nameof(ButlerEnemyAI.Start))]
        [HarmonyPostfix]
        private static void StartPatch(ButlerEnemyAI __instance)
        {
            if (!Plugin.isHost) return;

            __instance.KillEnemy();
        }

    }
}
