using HarmonyLib;
using LethalMystery.Players;
namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ref int ___shellsLoaded)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                ___shellsLoaded = 2;
            }
            
        }
    }
}
