using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {

        [HarmonyPatch(typeof(UnlockableSuit), "SwitchSuitToThis")]
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
