using HarmonyLib;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {
        public static bool spawnedNutForWeapon = false;

        [HarmonyPatch(nameof(NutcrackerEnemyAI.Update))]
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



        [HarmonyPatch(nameof(NutcrackerEnemyAI.SpawnShotgunShells))]
        [HarmonyPrefix]
        private static bool NoBulletObjects()
        {
            return false;
        }

    }
}
