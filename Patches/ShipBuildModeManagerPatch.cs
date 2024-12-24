using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ShipBuildModeManager))]
    internal class ShipBuildModeManagerPatch
    {

        /// <summary>
        /// Prevent users from storing items when the game has started
        /// </summary>
        [HarmonyPatch(typeof(ShipBuildModeManager), nameof(ShipBuildModeManager.StoreObjectLocalClient))]
        [HarmonyPrefix]
        private static bool PreventStoring()
        {
            if (StartOfRound.Instance.inShipPhase)
            {
                return true;
            }
            HUDManager.Instance.DisplayTip("Oops!", "You can only store items before the game has started.");
            return false;
        }
    }
}
