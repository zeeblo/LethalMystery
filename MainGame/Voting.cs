using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Voting
    {
        public static LNetworkVariable<Dictionary<string, string>> allVotes = LNetworkVariable<Dictionary<string, string>>.Connect("allVotes");
        public static LNetworkVariable<string> skipVotes = LNetworkVariable<string>.Connect("skipVotes");


        public static void VoteSetup()
        {
            if (!Plugin.isHost) return;

            Dictionary<string, string> rawAllVotes = new Dictionary<string, string>();
            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                if (rawAllVotes.ContainsKey($"{user.actualClientId}")) continue;
                rawAllVotes.Add($"{user.actualClientId}", "0");
            }

            allVotes.Value = rawAllVotes;
            skipVotes.Value = "0";
        }



        public static void VoteButtonClick(int index, UnityEngine.UI.Image check)
        {
            Plugin.mls.LogInfo($"Vote Button {index} clicked.");
            check.sprite = Plugin.CheckboxEnabledIcon;


            foreach (KeyValuePair<string, string> plrID in allVotes.Value)
            {
                if (plrID.Key == $"{index}")
                {
                    allVotes.Value[plrID.Key] =  $"{StringAddons.AddInts(plrID.Value, 1)}";
                }
            }
        }


        public static void SkipButtonClick(int index, UnityEngine.UI.Image check)
        {
            Plugin.mls.LogInfo($"Skip Button {index} clicked.");
            check.sprite = Plugin.CheckboxEnabledIcon;

            skipVotes.Value = $"{StringAddons.AddInts(skipVotes.Value, 1)}";
            VotingUI.UpdateSkipText();
        }


    }
}
