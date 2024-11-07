using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;


namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {


        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        private static void SpawnHorn(StartOfRound __instance)
        {
            MethodInfo SpawnUnlockable = typeof(StartOfRound).GetMethod("SpawnUnlockable", BindingFlags.NonPublic | BindingFlags.Instance);
            SpawnUnlockable.Invoke(__instance, new object[] {18});
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetPlanetsWeather))]
        [HarmonyPrefix]
        private static bool NoBadWeather(ref SelectableLevel[] ___levels)
        {
            for (int i = 0; i < ___levels.Length; i++)
            {
                ___levels[i].currentWeather = LevelWeatherType.None;
                if (___levels[i].overrideWeather)
                {
                    ___levels[i].currentWeather = ___levels[i].overrideWeatherType;
                }
            }
            return false;
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Update))]
        [HarmonyPostfix]
        private static void stopSpeaker(StartOfRound __instance)
        {
            if (__instance.speakerAudioSource.isPlaying)
            {
                __instance.speakerAudioSource.Stop();
            }
        }

    }
}
