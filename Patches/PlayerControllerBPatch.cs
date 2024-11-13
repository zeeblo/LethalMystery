using HarmonyLib;
using GameNetcodeStuff;
using LethalMystery.Players;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {

        
        [HarmonyPatch(typeof(PlayerControllerB), "OnDisable")]
        [HarmonyPostfix]
        private static void OnDisablePatch()
        {
            if (Controls.SpawnWeaponAction != null)
            {
                Controls.SpawnWeaponAction.Disable();
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(PlayerControllerB __instance)
        {
            __instance.takingFallDamage = false;
            // __instance.carryWeight = 1f; // possibly remove weight so people can't speed check others | or set carry weight of weapons to 1
        }


    }
}
