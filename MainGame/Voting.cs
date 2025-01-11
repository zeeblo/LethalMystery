using GameNetcodeStuff;
using LethalNetworkAPI;
using HarmonyLib;
using LethalMystery.Utils;
using LethalMystery.UI;


namespace LethalMystery.MainGame
{
    [HarmonyPatch]
    internal class Voting
    {
        public static LNetworkVariable<Dictionary<string, string>> allVotes = LNetworkVariable<Dictionary<string, string>>.Connect("allVotes");
        public static LNetworkVariable<string> skipVotes = LNetworkVariable<string>.Connect("skipVotes");
        public static int localPlayerVote = 0;
        public static bool hasVoted = false;
        public static bool canVote = true;
        public static int amountOfPlayers;



        #region Patches


        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
        [HarmonyPostfix]
        private static void UserLeft(ulong clientId)
        {
            if (StringAddons.ConvertToBool(Meeting.inMeeting.Value) == false) return;
            amountOfPlayers -= 1;

            
            VotingUI.UpdatePlayerList(clientId / 2);
            RefreshPlayerVotes($"{clientId / 2}");

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
            amountOfPlayers -= 1;
            //RefreshPlayerVotes();
        }


        #endregion Patches




        public static void VoteSetup()
        {
            hasVoted = false;
            canVote = true;
            localPlayerVote = 0;

            if (!Plugin.isHost) return;

            Dictionary<string, string> rawAllVotes = new Dictionary<string, string>();
            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                if (rawAllVotes.ContainsKey($"{user.actualClientId}") || user.isPlayerDead) continue;
                if (user.actualClientId == 0)
                {
                    rawAllVotes.Add($"{user.actualClientId}", "0");
                }
                else
                {
                    rawAllVotes.Add($"{user.actualClientId / 2}", "0");
                }
            }

            allVotes.Value = rawAllVotes;
            skipVotes.Value = "0";
        }

        public static void RefreshPlayerVotes(string playerID)
        {
            if (!Plugin.isHost) return;
            if (allVotes.Value == null) return;

            Dictionary<string, string> rawAllVotes = new Dictionary<string, string>();
            rawAllVotes = allVotes.Value;
            rawAllVotes.Remove(playerID);

            allVotes.Value = rawAllVotes;
        }


        public static void VoteButtonClick(int userID, UnityEngine.UI.Image check)
        {
            if (canVote == false) return;
            if (StringAddons.ConvertToFloat(Meeting.discussTime.Value) > 0) return;

            Plugin.mls.LogInfo($"Voted {userID}.");
            check.sprite = Plugin.CheckboxEnabledIcon;


            foreach (KeyValuePair<string, string> plrID in allVotes.Value.ToList())
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
            if (canVote == false ) return;
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
