using GameNetcodeStuff;
using HarmonyLib;



namespace LethalMystery.Players.Abilities
{

    [HarmonyPatch]
    internal class Ability
    {
        
        public static void ResetVars()
        {
            InstantKill.ResetVars();
            CleanSuit.ResetVars();
            Vent.ResetVars();
        }



        /// <summary>
        /// Prevent user from dropping their special role item
        /// </summary>
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool DontDropSpecialItem(PlayerControllerB __instance)
        {
            if (__instance.ItemSlots[__instance.currentItemSlot] == null) return true;
            if (__instance.ItemSlots[__instance.currentItemSlot].playerHeldBy.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId) return true;
            if (Roles.CurrentRole == null) return true;
            if (__instance.ItemSlots[__instance.currentItemSlot].name.ToLower().Contains(Roles.CurrentRole.GetItem()))
            {
                return false;
            }

            return true;
        }




    }
}
