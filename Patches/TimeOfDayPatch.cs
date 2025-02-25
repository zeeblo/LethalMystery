using HarmonyLib;
using LethalMystery.MainGame;


namespace LethalMystery.Patches
{

    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        [HarmonyPatch(nameof(TimeOfDay.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(TimeOfDay __instance)
        {
            /// <summary>
            /// Disable Deadline
            /// </summary>
            __instance.timeUntilDeadline = 9;
            __instance.daysUntilDeadline = 3;
            StartOfRound.Instance.deadlineMonitorText.text = $"Meeting:\n {Meeting.MeetingNum}";

        }



        [HarmonyPatch(nameof(TimeOfDay.VoteShipToLeaveEarly))]
        [HarmonyPrefix]
        private static bool NoDeadVotes()
        {
            return false;
        }


    }
}
