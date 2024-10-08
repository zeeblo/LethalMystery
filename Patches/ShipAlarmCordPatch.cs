using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace LethalMystery.Patches
{
    [HarmonyPatch(typeof(ShipAlarmCord))]
    internal class ShipAlarmCordPatch
    {
        public static int meetingHornID;


        [HarmonyPatch(typeof(ShipAlarmCord), "Start")]
        [HarmonyPostfix]
        private static void GetUnlockableID(int ___unlockableID)
        {
            Plugin.mls.LogInfo(">>> ShipHornID: " + ___unlockableID);
            //meetingHornID = ___unlockableID;
        }
    }
}
