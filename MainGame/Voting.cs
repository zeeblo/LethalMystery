using GameNetcodeStuff;
using LethalNetworkAPI;
using HarmonyLib;
using LethalMystery.Utils;
using LethalMystery.UI;
using Unity.Services.Authentication.Internal;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Voting
    {
        [PublicNetworkVariable]
        public static LNetworkVariable<Dictionary<string, string>> allVotes = LNetworkVariable<Dictionary<string, string>>.Connect("allVotes");
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
        }


        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void PlayerDied()
        {
            canVote = false;
            Plugin.mls.LogInfo($">>> KillPlayer: {Plugin.localPlayer.playerClientId}");
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
            if (allVotes.Value == null) return;
            Plugin.mls.LogInfo(">>> Refreshed dictionary");

            Plugin.netHandler.setupVotesReceive($"{playerID}/refresh", Plugin.localPlayer.playerClientId);
        }




        public static void VoteButtonClick(int userID, UnityEngine.UI.Image check)
        {
            if (canVote == false) return;
            if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) return;

            Plugin.mls.LogInfo($"Voted {userID}.");
            check.sprite = Plugin.CheckboxEnabledIcon;

            // got rid of .ToList()
            foreach (KeyValuePair<string, string> plrID in allVotes.Value)
            {
                if (plrID.Key == $"{userID}")
                {
                    Plugin.netHandler.playerVotedReceive($"vote/{userID}", Plugin.localPlayer.playerClientId);
                    localPlayerVote = userID;
                    hasVoted = true;
                    break;
                }
            }


        }


        public static void SkipButtonClick(UnityEngine.UI.Image check)
        {
            if (canVote == false) return;
            if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) return;

            check.sprite = Plugin.CheckboxEnabledIcon;
            Plugin.netHandler.playerVotedReceive($"skip/", Plugin.localPlayer.playerClientId);
            hasVoted = true;
        }


        private static void TallyVotes()
        {
            List<int> playerVotes = new List<int>();
            foreach (KeyValuePair<string, string> v in Voting.allVotes.Value)
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

            foreach (KeyValuePair<string, string> v in Voting.allVotes.Value)
            {
                int voteNum = StringAddons.ConvertToInt(v.Value);
                if (voteNum == maxVote)
                {
                    int plrID = StringAddons.ConvertToInt(v.Key);
                    string PlayerName = StartOfRound.Instance.allPlayerScripts[plrID].playerUsername;
                    Plugin.mls.LogInfo($">>> Voted {PlayerName} <<<");
                    break;
                }

            }
        }

    }
}
