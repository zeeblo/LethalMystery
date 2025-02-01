using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;


namespace LethalMystery.Maps
{

    [HarmonyPatch]
    internal class CustomLvl
    {


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        private static void MoonsInTerminal(Terminal __instance)
        {
            List<SelectableLevel> allMoons = new List<SelectableLevel>();



            foreach (SelectableLevel moon in __instance.moonsCatalogueList)
            {
                allMoons.Add(moon);
            }
        }
    }
}
