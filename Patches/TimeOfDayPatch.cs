using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        [HarmonyPatch(typeof(TimeOfDay), "UpdateProfitQuotaCurrentTime")]
        [HarmonyPrefix]
        private static bool DisableDeadline()
        {
            TimeOfDay.Instance.timeUntilDeadline = 999;
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Plugin.MeetingNum}";
            return false;
        }
    }
}
