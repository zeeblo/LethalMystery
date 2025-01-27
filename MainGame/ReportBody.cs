using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class ReportBody
    {

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.BeginGrabObject))]
        [HarmonyPrefix]
        private static bool StartMeeting(PlayerControllerB __instance)
        {
            GameObject ragdoll = GOTools.GetObjectPlayerIsLookingAt();
            //Plugin.mls.LogInfo($">>> ragdoll tag: {ragdoll.tag} | ragdoll name: {ragdoll.name}");
            if (ragdoll == Plugin.localPlayer.gameObject) return true;

            if (ragdoll.tag.StartsWith("PlayerRagdoll"))
            {
                Plugin.netHandler.MeetingReceive("body", Plugin.localPlayer.playerClientId);
                return false;
            }

            return true;
        }
    }
}
