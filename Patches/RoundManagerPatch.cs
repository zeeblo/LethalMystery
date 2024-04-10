using HarmonyLib;
using LethalMystery.Players;
using UnityEngine;


namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPostfix]
        private static void Start()
        {
            Roles.AssignRole();

        }



    }
}
