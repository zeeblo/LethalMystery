using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {

        [HarmonyPatch(nameof(Shovel.HitShovelClientRpc))]
        [HarmonyPostfix]
        private static void CheckID(int hitSurfaceID)
        {
            Plugin.mls.LogInfo($">>> HitSurfaceID: {hitSurfaceID}");
        }
    }
}
