using HarmonyLib;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {

        [HarmonyPatch(nameof(NutcrackerEnemyAI.Start))]
        [HarmonyPostfix]
        private static void StartPatch(NutcrackerEnemyAI __instance)
        {
            if (!Plugin.isHost) return;

            __instance.KillEnemy();


        }



        [HarmonyPatch(nameof(NutcrackerEnemyAI.SpawnShotgunShells))]
        [HarmonyPrefix]
        private static bool NoBulletObjects()
        {
            return false;
        }

    }
}
