using GameNetcodeStuff;
using LethalNetworkAPI;
using HarmonyLib;
using LethalMystery.Utils;
using LethalMystery.UI;
using System.Collections.Generic;
using System.Linq;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Voting
    {
        [PublicNetworkVariable]
        public static LNetworkVariable<Dictionary<string, string>> playersWhoGotVoted = LNetworkVariable<Dictionary<string, string>>.Connect("playersWhoGotVoted");
        [PublicNetworkVariable]
        public static LNetworkVariable<Dictionary<string, string>> playersWhoVoted = LNetworkVariable<Dictionary<string, string>>.Connect("playersWhoVoted");
        public static LNetworkVariable<string> skipVotes = LNetworkVariable<string>.Connect("skipVotes");
        public static int localPlayerVote = 0;
        public static bool hasVoted = false;
        public static bool canVote = true;



        #region Patches


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
        [HarmonyPostfix]
        private static void UserLeft(ulong clientId)
        {
            Plugin.mls.LogInfo($">>> PlayerLeft: {clientId}");
            Plugin.mls.LogInfo($">>> PlayerLeftDiv: {clientId / 2}");
            RefreshPlayerVotes($"{clientId / 2}");
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false) return;

            VotingUI.UpdatePlayerList(clientId / 2);
            Plugin.mls.LogInfo($"<<< localPlayerVote: {localPlayerVote}");
            if ($"{clientId / 2}" == $"{localPlayerVote}")
            {
                Plugin.mls.LogInfo(">>> Player has been given back their vote.");
                hasVoted = false;
            }

            if (Plugin.isHost)
            {
                EndGame.aliveCrew.Remove(clientId / 2);
                EndGame.aliveMonsters.Remove(clientId / 2);
            }
        }


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.KickPlayer))]
        [HarmonyPostfix]
        private static void UserKicked(int playerObjToKick)
        {
            // Have not tested this method at all, hoping it works :D

            Plugin.mls.LogInfo($">>> PlayerKicked: {playerObjToKick}");
            RefreshPlayerVotes($"{playerObjToKick}");
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false) return;

            VotingUI.UpdatePlayerList(StartOfRound.Instance.allPlayerScripts[playerObjToKick].playerClientId);
            Plugin.mls.LogInfo($"<<< localPlayerVote: {localPlayerVote}");
            if ($"{playerObjToKick}" == $"{localPlayerVote}")
            {
                Plugin.mls.LogInfo(">>> Player has been given back their vote.");
                hasVoted = false;
            }

            if (Plugin.isHost)
            {
                EndGame.aliveCrew.Remove(StartOfRound.Instance.allPlayerScripts[playerObjToKick].playerClientId);
                EndGame.aliveMonsters.Remove(StartOfRound.Instance.allPlayerScripts[playerObjToKick].playerClientId);
            }
        }



        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void PlayerDied()
        {
            canVote = false;
            Plugin.netHandler.playerDiedReceive($"{Plugin.localPlayer.playerClientId}", Plugin.localPlayer.playerClientId);
        }

        #endregion Patches



        public static void ResetVars()
        {
            if (Plugin.localPlayer.isPlayerDead) return;

            hasVoted = false;
            canVote = true;
            localPlayerVote = 0;
        }

        public static void VoteSetup()
        {

            ResetVars();
            if (!Plugin.isHost) return;

            Plugin.netHandler.setupVotesReceive("/setup", Plugin.localPlayer.playerClientId);
        }


        /// <summary>
        /// Removes dead and disconnected players from the dictionary
        /// </summary>
        public static void RefreshPlayerVotes(string playerID)
        {
            if (!Plugin.isHost) return;
            if (playersWhoGotVoted.Value == null) return;

            Plugin.netHandler.setupVotesReceive($"{playerID}/refresh", Plugin.localPlayer.playerClientId);
        }

        
        public static bool EveryoneVoted()
        {
            if (StringAddons.ConvertToFloat(Meeting.voteTime.Value) <= 10) return false;
            foreach (KeyValuePair<string, string> v in playersWhoVoted.Value)
            {
                if (v.Value != "voted") return false;
            }
            TallyVotes();
            return true;
        }


        public static void VoteButtonClick(int userID, UnityEngine.UI.Image check)
        {
            if (canVote == false || hasVoted) return;
            if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) return;

            Plugin.mls.LogInfo($"Voted {userID}.");
            check.sprite = LMAssets.CheckboxEnabledIcon;

            ulong PlayerThatVoted = Plugin.localPlayer.playerClientId;
            foreach (KeyValuePair<string, string> plrID in playersWhoGotVoted.Value)
            {
                if (plrID.Key == $"{userID}")
                {
                    Plugin.netHandler.playerVotedReceive($"vote/{userID}/{PlayerThatVoted}", PlayerThatVoted);
                    localPlayerVote = userID;
                    hasVoted = true;
                    break;
                }
            }


        }


        public static void SkipButtonClick(UnityEngine.UI.Image check)
        {
            if (canVote == false || hasVoted) return;
            if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) return;

            check.sprite = LMAssets.CheckboxEnabledIcon;
            ulong PlayerThatVoted = Plugin.localPlayer.playerClientId;
            Plugin.netHandler.playerVotedReceive($"skip/skip/{PlayerThatVoted}", PlayerThatVoted);
            hasVoted = true;
        }


        public static void TallyVotes()
        {
            List<int> playerVotes = new List<int>();
            foreach (KeyValuePair<string, string> v in Voting.playersWhoGotVoted.Value)
            {
                int voteNum = StringAddons.ConvertToInt(v.Value);
                playerVotes.Add(voteNum);
            }
            int skipVotes = StringAddons.ConvertToInt(Voting.skipVotes.Value);
            playerVotes.Add(skipVotes);


            int maxVote = playerVotes.Max(n => n);
            playerVotes.Remove(maxVote);

            foreach (int vote in playerVotes)
            {
                if (vote == maxVote)
                {
                    Plugin.mls.LogInfo(">>> Skipping Vote <<<");
                    return;
                }
            }

            foreach (KeyValuePair<string, string> v in Voting.playersWhoGotVoted.Value)
            {
                int voteNum = StringAddons.ConvertToInt(v.Value);
                if (voteNum == maxVote)
                {
                    int plrID = StringAddons.ConvertToInt(v.Key);
                    string PlayerName = StartOfRound.Instance.allPlayerScripts[plrID].playerUsername;
                    Plugin.mls.LogInfo($">>> Voted {PlayerName} <<<");

                    Plugin.netHandler.ejectPlayerReceive($"{plrID}", Plugin.localPlayer.playerClientId);
                    break;
                }

            }
        }

    }
}
