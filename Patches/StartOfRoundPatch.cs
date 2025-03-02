using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Utils;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {


        /// <summary>
        /// It's the same method except it disables usernames on the map.
        /// </summary>
        [HarmonyPatch(nameof(StartOfRound.SwitchMapMonitorPurpose))]
        [HarmonyPrefix]
        private static bool SwitchMapMonitorPurposePatch(StartOfRound __instance, bool displayInfo)
        {
            if (displayInfo)
            {
                __instance.screenLevelVideoReel.enabled = false;
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




        [HarmonyPatch(nameof(StartOfRound.SetPlanetsWeather))]
        [HarmonyPrefix]
        private static bool NoBadWeather(ref SelectableLevel[] ___levels)
        {
            for (int i = 0; i < ___levels.Length; i++)
            {
                ___levels[i].currentWeather = LevelWeatherType.None;
                /*
                if (___levels[i].overrideWeather)
                {
                    ___levels[i].currentWeather = ___levels[i].overrideWeatherType;
                }
                */
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



        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        [HarmonyPrefix]
        private static bool DontSaveShip()
        {
            return false;
        }

    }
}
