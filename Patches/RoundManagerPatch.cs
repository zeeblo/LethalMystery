using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {

        /// <summary>
        /// Gets rid of landmines, turrets, etc.
        /// </summary>
        [HarmonyPatch(nameof(RoundManager.SpawnMapObjects))]
        [HarmonyPrefix]
        private static bool RemoveDangerObjects()
        {
            Plugin.mls.LogInfo(">>> RemoveDangerObjects()");
            return false;
        }

        // Shoutout to peacefulCompany
        [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        private static bool NoEnemiesPatch(ref SelectableLevel newLevel)
        {
            Plugin.mls.LogInfo(">>> NoEnemiesPatch()");
            foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
            {
                enemy.rarity = 0;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                outsideEnemy.rarity = 0;
            }
            foreach (SpawnableEnemyWithRarity daytimeEnemy in newLevel.DaytimeEnemies)
            {
                daytimeEnemy.rarity = 0;
            }
            return true;
        }


        [HarmonyPatch(nameof(RoundManager.SpawnEnemyOnServer))]
        [HarmonyPrefix]
        private static bool NoEnemiesPatch2()
        {
            Plugin.mls.LogInfo(">>> NoEnemiesPatch2()");
            return false;
        }


        [HarmonyPatch(nameof(RoundManager.SpawnEnemyGameObject))]
        [HarmonyPrefix]
        private static bool NoEnemiesPatch3()
        {
            Plugin.mls.LogInfo(">>> NoEnemiesPatch3()");
            return false;
        }


    }
}
