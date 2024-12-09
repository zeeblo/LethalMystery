using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {

        /// <summary>
        /// Players can only select a suit before the game begins
        /// </summary>
        [HarmonyPatch(nameof(UnlockableSuit.SwitchSuitToThis))]
        [HarmonyPrefix]
        private static bool SwitchSuitToThisPatch()
        {
            if(GameNetworkManager.Instance.gameHasStarted)
            {
                return false;
            }
            return true;
        }

    }
}
