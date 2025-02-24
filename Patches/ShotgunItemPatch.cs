using System.Numerics;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Players;
using UnityEngine;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {


        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void InfiniteBullets(ShotgunItem __instance, ref int ___shellsLoaded, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {
            if (Roles.CurrentRole != null && Roles.CurrentRole.Name == "sheriff")
            {
                ___shellsLoaded = 2;

                killMonster(__instance, shotgunPosition: shotgunPosition, shotgunForward: shotgunForward);
            }
            
        }


        private static void killMonster(ShotgunItem __instance, UnityEngine.Vector3 shotgunPosition, UnityEngine.Vector3 shotgunForward)
        {

            /*
            Ray ray = new Ray(shotgunPosition, shotgunForward);
            if (Physics.Raycast(ray, out var hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndPlayers, QueryTriggerInteraction.Ignore))
            {
                
                if (hitInfo.collider == null) return;
                if (hitInfo.collider.transform.gameObject.GetComponent<PlayerControllerB>() == null) return;
                PlayerControllerB targetPlayer = hitInfo.collider.transform.gameObject.GetComponent<PlayerControllerB>();

                if (targetPlayer.playerClientId == Plugin.localPlayer.playerClientId) return;
                if (Roles.NameIsMonsterType(Roles.localPlayerRoles[targetPlayer.playerClientId]))
                {
                    Plugin.localPlayer.DamagePlayer(999);
                }
               

                Plugin.mls.LogInfo($">>> Shot at {hitInfo.collider.transform.gameObject.name}");
            }
             */
        }
    }
}
