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


        /// <summary>
        /// It's the same method except it gets disabled usernames on the map.
        /// </summary>
        [HarmonyPatch(nameof(StartOfRound.SwitchMapMonitorPurpose))]
        [HarmonyPrefix]
        private static bool SwitchMapMonitorPurposePatch(StartOfRound __instance, bool displayInfo)
        {
            if (displayInfo)
            {
                __instance.screenLevelVideoReel.enabled = true;
                __instance.screenLevelVideoReel.gameObject.SetActive(value: true);
                __instance.screenLevelDescription.enabled = true;
                __instance.mapScreenPlayerName.enabled = false;
                __instance.mapScreen.overrideCameraForOtherUse = true;
                __instance.mapScreen.SwitchScreenOn();
                __instance.mapScreen.cam.enabled = true;
                Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                terminal.displayingPersistentImage = null;
                terminal.terminalImage.enabled = false;
            }
            else
            {
                __instance.screenLevelVideoReel.enabled = false;
                __instance.screenLevelVideoReel.gameObject.SetActive(value: false);
                __instance.screenLevelDescription.enabled = false;
                __instance.mapScreenPlayerName.enabled = false;
                __instance.mapScreen.overrideCameraForOtherUse = false;
            }
            return false;
        }



        [HarmonyPatch(nameof(StartOfRound.Start))]
        [HarmonyPostfix]
        private static void SpawnHorn(StartOfRound __instance)
        {
            MethodInfo SpawnUnlockable = typeof(StartOfRound).GetMethod("SpawnUnlockable", BindingFlags.NonPublic | BindingFlags.Instance);
            SpawnUnlockable.Invoke(__instance, new object[] {18});
        }


        [HarmonyPatch(nameof(StartOfRound.SetPlanetsWeather))]
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


        [HarmonyPatch(nameof(StartOfRound.Update))]
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
