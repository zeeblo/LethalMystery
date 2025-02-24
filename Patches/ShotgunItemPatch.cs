using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Players.Abilities;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPrefix]
        private static bool ShootPatch(ShotgunItem __instance, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                InstantKill.killType killtype = InstantKill.killMonster(__instance, shotgunPosition: shotgunPosition, shotgunForward: shotgunForward);
                switch (killtype)
                {
                    case InstantKill.killType.None:
                        return true;
                    case InstantKill.killType.player:
                        return true;
                    case InstantKill.killType.self:
                        return true;
                }
            }
            return true;
        }






        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ShotgunItem __instance, ref int ___shellsLoaded, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                ___shellsLoaded = 2;

            }
            
        }



    }
}
