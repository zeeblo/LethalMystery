using GameNetcodeStuff;
using LethalNetworkAPI;


namespace LethalMystery.MainGame
{

    internal class Voting
    {
        public static LNetworkVariable<Dictionary<string, string>> allVotes = LNetworkVariable<Dictionary<string, string>>.Connect("allVotes");
        public static LNetworkVariable<string> skipVotes = LNetworkVariable<string>.Connect("skipVotes");
        public static int localPlayerVote = 0;
        public static bool hasVoted = false;

        public static void VoteSetup()
        {
            if (!Plugin.isHost) return;

            Dictionary<string, string> rawAllVotes = new Dictionary<string, string>();
            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                //if (rawAllVotes.ContainsKey($"{user.actualClientId}")) continue;
                //rawAllVotes.Add($"{user.actualClientId}", "0");
                rawAllVotes.Add($"{user.playerClientId}", "0");
            }

            allVotes.Value = rawAllVotes;
            skipVotes.Value = "0";
            hasVoted = false;
            localPlayerVote = 0;
        }



        public static void VoteButtonClick(int userID, UnityEngine.UI.Image check)
        {
            Plugin.mls.LogInfo($"Voted {userID}.");
            check.sprite = Plugin.CheckboxEnabledIcon;


            foreach (KeyValuePair<string, string> plrID in allVotes.Value.ToList())
            {
                if (plrID.Key == $"{userID}")
                {
                    Plugin.netHandler.playerVotedReceive($"vote/{userID}", Plugin.localPlayer.actualClientId);
                    localPlayerVote = userID;
                    hasVoted = true;
                    break;
                }
            }

            
        }


        public static void SkipButtonClick(UnityEngine.UI.Image check)
        {
            check.sprite = Plugin.CheckboxEnabledIcon;
            Plugin.netHandler.playerVotedReceive($"skip/", Plugin.localPlayer.actualClientId);
            hasVoted = true;
        }


    }
}
