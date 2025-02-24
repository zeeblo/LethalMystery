using HarmonyLib;
using LethalMystery.Players;
using LethalMystery.Players.Abilities;
using UnityEngine;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(nameof(ShotgunItem.ShootGunAndSync))]
        [HarmonyPrefix]
        private static bool ShootPatch()
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                Vector3 shotgunPosition = Plugin.localPlayer.gameplayCamera.transform.position - Plugin.localPlayer.gameplayCamera.transform.up * 0.45f;
                Vector3 forward = Plugin.localPlayer.gameplayCamera.transform.forward;

                InstantKill.killType killtype = InstantKill.killMonster(shotgunPosition: shotgunPosition, shotgunForward: forward);
                switch (killtype)
                {
                    case InstantKill.killType.None:
                        return true;
                    case InstantKill.killType.player:
                        return true;
                    case InstantKill.killType.self:
                        return false;
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
