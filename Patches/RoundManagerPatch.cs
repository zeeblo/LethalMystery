using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {


        [HarmonyPatch(typeof(RoundManager), "SetToCurrentLevelWeather")]
        [HarmonyPrefix]
        public static bool DisableSetToCurrentLevelWeather()
        {
            TimeOfDay.Instance.currentLevelWeather = LevelWeatherType.None;
            return false;
        }
    }
}
