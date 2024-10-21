using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        [HarmonyPostfix]
        private static void DisableDeadline()
        {
            TimeOfDay.Instance.timeUntilDeadline = 99999;
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Plugin.MeetingNum}";
        }
    }
}
