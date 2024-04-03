using HarmonyLib;
using LethalMystery.Utils;
using LethalMystery.Players;
using UnityEngine;


namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static AssignmentUI? _assignmentUI;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPostfix]
        private static void Start()
        {
            _assignmentUI = new GameObject("UI").AddComponent<AssignmentUI>();
            Roles.AssignRole("Shapeshifter");


        }


    }
}
