using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(TimeOfDay __instance)
        {
            /// <summary>
            /// Disable Deadline
            /// </summary>
            __instance.timeUntilDeadline = 9;
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Plugin.MeetingNum}";


        }


    }
}
