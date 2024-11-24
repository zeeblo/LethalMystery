using HarmonyLib;
using GameNetcodeStuff;
using LethalMystery.Players;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(PlayerControllerB __instance)
        {
            __instance.takingFallDamage = false;
        }


    }
}
