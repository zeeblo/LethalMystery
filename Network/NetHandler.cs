using System.Collections;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Patches;
using LethalMystery.Players;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;
using UnityEngine;
using static LethalMystery.Players.Roles;


namespace LethalMystery.Network
{
    public class NetHandler
    {
        private static LNetworkVariable<Dictionary<ulong, string>> allPlayerRoles;
        private LNetworkMessage<string> spawnWeapon;
        private LNetworkMessage<string> slots;
        private LNetworkMessage<string> meeting;
        private LNetworkMessage<List<string>> addScrapsToList;
        private LNetworkMessage<string> destroyScrap;
        private LNetworkMessage<string> showScrap;
        private LNetworkMessage<string> hideWeapon;
        private LNetworkMessage<string> playerVoted;
        private LNetworkMessage<string> playerDied;
        private LNetworkMessage<string> setupVotes;
        private LNetworkMessage<string> ejectPlayer;

        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            allPlayerRoles = LNetworkVariable<Dictionary<ulong, string>>.Connect("allPlayerRoles");

            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<string>.Connect("Slots");
            meeting = LNetworkMessage<string>.Connect("CallAMeeting");
            addScrapsToList = LNetworkMessage<List<string>>.Connect("addScrapsToList");
            destroyScrap = LNetworkMessage<string>.Connect("destroyScrap");
            showScrap = LNetworkMessage<string>.Connect("showScrap");
            hideWeapon = LNetworkMessage<string>.Connect("hideWeapon");
            playerVoted = LNetworkMessage<string>.Connect("playerVoted");
            playerDied = LNetworkMessage<string>.Connect("playerDied");
            setupVotes = LNetworkMessage<string>.Connect("setupVotes");
            ejectPlayer = LNetworkMessage<string>.Connect("ejectPlayer");

            spawnWeapon.OnServerReceived += SpawnWeaponServer;
            slots.OnServerReceived += SlotsServer;
            slots.OnClientReceived += SlotsClients;
            meeting.OnServerReceived += MeetingServer;
            meeting.OnClientReceived += MeetingClients;
            addScrapsToList.OnServerReceived += addScrapsToListServer;
            addScrapsToList.OnClientReceived += addScrapsToListClients;
            destroyScrap.OnServerReceived += destroyScrapServer;
            destroyScrap.OnClientReceived += destroyScrapClients;
            showScrap.OnServerReceived += showScrapServer;
            showScrap.OnClientReceived += showScrapClients;
            hideWeapon.OnServerReceived += hideWeaponServer;
            hideWeapon.OnClientReceived += hideWeaponClients;
            playerVoted.OnServerReceived += playerVotedServer;
            playerVoted.OnClientReceived += playerVotedClients;
            playerDied.OnServerReceived += playerDiedServer;
            playerDied.OnClientReceived += playerDiedClients;
            setupVotes.OnServerReceived += setupVotesServer;
            setupVotes.OnClientReceived += setupVotesClients;
            ejectPlayer.OnServerReceived += ejectPlayerServer;
            ejectPlayer.OnClientReceived += ejectPlayerClients;
        }

        #region Variables

        public void AddCustomNetEvents()
        {
            Meeting.inMeeting.OnValueChanged += InMeeting_Event;
            Start.inGracePeriod.OnValueChanged += InGracePeriod_Event;
            //Voting.skipVotes.OnValueChanged += InSkipVotes_Event;
            //Voting.allVotes.OnValueChanged += InAllVotes_Event;

        }

        public void RemoveCustomNetEvents()
        {
            Meeting.inMeeting.OnValueChanged -= InMeeting_Event;
            Start.inGracePeriod.OnValueChanged -= InGracePeriod_Event;
            //Voting.skipVotes.OnValueChanged -= InSkipVotes_Event;
            //Voting.allVotes.OnValueChanged -= InAllVotes_Event;
        }


        private static void InMeeting_Event(string idk, string data)
        {
            if (StringAddons.ConvertToBool(data) == false)
            {
                HangarShipDoor ship = Plugin.shipInstance.GetComponent<HangarShipDoor>();
                ship.PlayDoorAnimation(closed: false);
                ship.SetDoorButtonsEnabled(true);
                ship.doorPower = 0;
                ship.overheated = true;
                ship.triggerScript.interactable = true;

                Plugin.RemoveEnvironment(false);
                Meeting.MeetingDefaults();
                StartOfRound.Instance.mapScreen.SwitchScreenOn(true);

                if (VotingUI.votingGUI != null)
                {
                    Plugin.Destroy(VotingUI.votingGUI);
                }
            }
        }

        private static void InGracePeriod_Event(string idk, string data)
        {
            if (StringAddons.ConvertToBool(data) == false)
            {
                HUDManager.Instance.loadingText.enabled = false;
            }
        }


        public void SetallPlayerRoles(Dictionary<ulong, string> roles)
        {
            if (allPlayerRoles == null) return;
            allPlayerRoles.Value = roles;
        }

        public Role GetallPlayerRoles()
        {
            string role = "";
            Role assignedRole = new Role("boop", "I booped your nose.", RoleType.employee); // placeholder, should never register as an actual role.
            Roles.localPlayerRoles = allPlayerRoles.Value;

            foreach (KeyValuePair<ulong, string> plrID in allPlayerRoles.Value)
            {
                if (Plugin.localPlayer.playerClientId == plrID.Key)
                {
                    role = plrID.Value;
                    break;
                }
            }
            foreach (Role rl in Roles.allRoles)
            {
                if (rl.Name == role)
                {
                    assignedRole = rl;
                    break;
                }
            }

            return assignedRole;
        }




        #endregion Variables




        public void SpawnWeaponServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the SpawnMobServer: {data}");
            Commands.SpawnWeapons(data);
        }
        public void SpawnWeaponReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the SpawnMobReceive");
            string[] splitData = data.Split('/');
            string type = splitData[0];
            string name = splitData[1];

            if (type == "monster")
            {
                spawnWeapon.SendServer("knife");
            }
            else if (name == "sheriff")
            {
                spawnWeapon.SendServer("shotgun");
            }

        }



        public void SlotsServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsServer:");
            slots.SendClients(data);
        }
        public void SlotsClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsClients:");
            string[] splitData = data.Split('/');
            ulong.TryParse(splitData[0], out ulong playerID);
            string type = splitData[1];

            if (type == "default")
            {
                for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    StartOfRound.Instance.allPlayerScripts[i].ItemSlots = new GrabbableObject[4];
                }
            }
            else if (type == "slots")
            {
                for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == playerID)
                    {
                        StartOfRound.Instance.allPlayerScripts[i].ItemSlots = new GrabbableObject[5];
                        break;
                    }
                    
                }
            }

        }
        public void SlotsReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsReceive:");
            slots.SendServer(data);
        }



        public void MeetingServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingServer:");

            Meeting.inMeeting.Value = "true";
            Start.inGracePeriod.Value = "true";
            Start.currentGracePeriodTime.Value = $"{LMConfig.defaultMeetingTime + 140f}";

            meeting.SendClients(data);
        }
        public void MeetingClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingClients:");
            StartOfRound.Instance.StartCoroutine(meetingSetup(data));
            
        }
        public void MeetingReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingReceive:");
            meeting.SendServer(data);
        }


        private static IEnumerator meetingSetup(string data)
        {
            
            if (!Plugin.localPlayer.isInHangarShipRoom)
            {
                Plugin.localPlayer.DropAllHeldItemsAndSync();
            }

            yield return new WaitForSeconds(0.4f);

            MoreSlots.SwitchToEmptySlot();

            yield return new WaitForSeconds(0.4f);

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            Plugin.RemoveEnvironment();
            HUDManagerPatch.DisplayDaysEdit(data);

            if (data == "body")
            {
                GOTools.RemoveGameObject("PlayerRagdoll");
            }
            
            yield return new WaitForSeconds(2f);
            VotingUI.isCalled = true;
        }



        public void addScrapsToListServer(List<string> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListServer:");

            addScrapsToList.SendClients(data);
        }
        public void addScrapsToListClients(List<string> data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListClients:");
            Tasks.allScraps = data;

        }
        public void addScrapsToListReceive(List<string> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListReceive:");
            addScrapsToList.SendServer(data);
        }



        public void destroyScrapServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the destroyScrapServer:");
            Plugin.mls.LogInfo($">>> data in svr: {data}");
            string[] splitData = data.Split('/');
            ulong.TryParse(splitData[0], out ulong playerID);
            string input = splitData[1];

            if (input == "destroy")
            {

                Camera gameplayCamera = StartOfRound.Instance.allPlayerScripts[playerID].gameplayCamera;
                float grabDistance = StartOfRound.Instance.allPlayerScripts[playerID].grabDistance;
                bool twoHanded = StartOfRound.Instance.allPlayerScripts[playerID].twoHanded;
                float sinkingValue = StartOfRound.Instance.allPlayerScripts[playerID].sinkingValue;
                Transform transform = StartOfRound.Instance.allPlayerScripts[playerID].transform;

                Ray interactRay = new Ray(gameplayCamera.transform.position, gameplayCamera.transform.forward);
                RaycastHit hit;
                int interactableObjectsMask = (int)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("interactableObjectsMask").GetValue();

                if (!Physics.Raycast(interactRay, out hit, grabDistance, interactableObjectsMask) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || twoHanded || sinkingValue > 0.73f || Physics.Linecast(gameplayCamera.transform.position, hit.collider.transform.position + transform.up * 0.16f, 1073741824, QueryTriggerInteraction.Ignore))
                {
                    return;
                }
                GrabbableObject currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
                string itmName = currentlyGrabbingObject.itemProperties.itemName.ToLower().Replace("(clone)", "");


                //destroyScrap.SendClients(input);
                UnityEngine.Object.Destroy(currentlyGrabbingObject.gameObject);

            }
            else
            {
                destroyScrap.SendClients(data);
            }

        }
        public void destroyScrapClients(string data)
        {

            // Activated by TaskUpdate()
            // Destroys gameObject specific user is holding on all clients when they enter the ship
            string[] splitData = data.Split('/');
            ulong.TryParse(splitData[0], out ulong playerID);
            Int32.TryParse(splitData[1], out int slot);

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == playerID && StartOfRound.Instance.allPlayerScripts[i].currentlyHeldObjectServer != null)
                    Plugin.Destroy(StartOfRound.Instance.allPlayerScripts[i].currentlyHeldObjectServer.gameObject);
            }


        }
        public void destroyScrapReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the destroyScrapReceive:");
            Plugin.mls.LogInfo($">>> data: {data}");
            destroyScrap.SendServer(data);
        }




        public void showScrapServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the showScrapServer:");
            showScrap.SendClients(data);
        }
        public void showScrapClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the showScrapClients:");

            if (data.EndsWith("CollectItem"))
            {
                string[] splitData = data.Split('/');
                ulong.TryParse(splitData[0], out ulong playerID);
                Int32.TryParse(splitData[1], out int slot);

                Plugin.mls.LogInfo(">>> CollectItem part");
                GrabbableObject itm = StartOfRound.Instance.allPlayerScripts[playerID].ItemSlots[slot];
                if (itm == null || Plugin.localPlayer.playerClientId == playerID)
                {
                    return;
                }
                HUDManager.Instance.AddNewScrapFoundToDisplay(itm);
            }
            else
            {
                string[] splitData = data.Split('/');
                ulong.TryParse(splitData[0], out ulong playerID);

                Camera gameplayCamera = StartOfRound.Instance.allPlayerScripts[playerID].gameplayCamera;
                float grabDistance = StartOfRound.Instance.allPlayerScripts[playerID].grabDistance;
                bool twoHanded = StartOfRound.Instance.allPlayerScripts[playerID].twoHanded;
                float sinkingValue = StartOfRound.Instance.allPlayerScripts[playerID].sinkingValue;
                Transform transform = StartOfRound.Instance.allPlayerScripts[playerID].transform;

                Ray interactRay = new Ray(gameplayCamera.transform.position, gameplayCamera.transform.forward);
                RaycastHit hit;
                int interactableObjectsMask = (int)Traverse.Create(GameNetworkManager.Instance.localPlayerController).Field("interactableObjectsMask").GetValue();

                if (!Physics.Raycast(interactRay, out hit, grabDistance, interactableObjectsMask) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || twoHanded || sinkingValue > 0.73f || Physics.Linecast(gameplayCamera.transform.position, hit.collider.transform.position + transform.up * 0.16f, 1073741824, QueryTriggerInteraction.Ignore))
                {
                    return;
                }
                GrabbableObject currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
                if (currentlyGrabbingObject != null)
                {
                    string itmName = currentlyGrabbingObject.itemProperties.itemName.ToLower().Replace("(clone)", "");

                    HUDManager.Instance.AddNewScrapFoundToDisplay(currentlyGrabbingObject);
                    HUDManager.Instance.DisplayNewScrapFound();

                }
            }

        }
        public void showScrapReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the showScrapReceive:");
            showScrap.SendServer(data);
        }





        public void hideWeaponServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the hideWeaponServer:");

            hideWeapon.SendClients(data);
        }
        public void hideWeaponClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the hideWeaponClients:");
            string[] splitData = data.Split('/');
            ulong.TryParse(splitData[0], out ulong playerID);
            bool.TryParse(splitData[1], out bool dataBool);

            if (Plugin.localPlayer.playerClientId == playerID) return;

            Plugin.mls.LogInfo(">>> setting thing to invis");
            GameObject userItem = StartOfRound.Instance.allPlayerScripts[playerID].currentlyHeldObjectServer.gameObject;
            userItem.SetActive(dataBool);
            StartOfRound.Instance.StartCoroutine(ShowWeaponTimer(userItem));
        }
        public void hideWeaponReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the hideWeaponReceive:");
            hideWeapon.SendServer(data);
        }


        /// <summary>
        /// In the intro all items/weapons that other users are holding are hidden
        /// so that the current player only sees their own weapon, this re-enables
        /// the weapons so they're shown when the intro screen ends.
        /// </summary>
        private static IEnumerator ShowWeaponTimer(GameObject itm)
        {
            yield return new WaitForSeconds(8);
            itm.SetActive(true);
            Plugin.mls.LogInfo("<<< Successfully made item visible");
        }



        public void playerVotedServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the playerVotedServer:");
            string[] splitData = data.Split('/');
            string playerThatVoted = splitData[2];

            Dictionary<string, string> votes = Voting.playersWhoVoted.Value;
            votes[playerThatVoted] = "voted";

            Voting.playersWhoVoted.Value = votes;

            playerVoted.SendClients(data);
        }
        public void playerVotedClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the playerVotedClients:");

            string[] splitData = data.Split('/');
            string type = splitData[0];
            Int32.TryParse(splitData[1], out int userID);
            string skipVal = Voting.skipVotes.Value;
            Dictionary<string, string> votes = Voting.playersWhoGotVoted.Value;

            //Plugin.mls.LogInfo($">> type: {type}");
            //Plugin.mls.LogInfo($">> userID: {userID}");
            //Plugin.mls.LogInfo($">> SKIPS: {skipVal}");

            switch (type)
            {
                case "vote":
                    votes[$"{userID}"] = $"{StringAddons.AddInts(votes[$"{userID}"], 1)}";
                    VotingUI.UpdateVoteText(userID);
                    break;
                case "skip":
                    Voting.skipVotes.Value = $"{StringAddons.AddInts(skipVal, 1)}";
                    VotingUI.UpdateSkipText();
                    break;
            }

        }
        public void playerVotedReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the playerVotedReceive:");
            playerVoted.SendServer(data);
        }



        public void playerDiedServer(string data, ulong id)
        {
            playerDied.SendClients(data);
        }
        public void playerDiedClients(string data)
        {
            Int32.TryParse(data, out int userID);

            Voting.RefreshPlayerVotes($"{userID}");
        }
        public void playerDiedReceive(string data, ulong id)
        {
            playerDied.SendServer(data);
        }


        public void setupVotesServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the setupVotesServer:");
            Dictionary<string, string> rawAllVotes = new Dictionary<string, string>();
            Dictionary<string, string> rawPlayersWhoVoted = new Dictionary<string, string>();
            string[] splitData = data.Split('/');
            string type = splitData[1];
            
            if (type == "setup")
            {

                foreach (KeyValuePair<ulong, int> i in StartOfRound.Instance.ClientPlayerList)
                {
                    if (StartOfRound.Instance.allPlayerScripts[i.Value].isPlayerDead) continue;

                    rawAllVotes.Add($"{i.Value}", "0");
                    rawPlayersWhoVoted.Add($"{i.Value}", "0");
                    //Plugin.mls.LogInfo($">>> ADDED ID IN SETUP: {i.Value}");
                }
                Voting.playersWhoGotVoted.Value = rawAllVotes;
                Voting.playersWhoVoted.Value = rawPlayersWhoVoted;
                Voting.skipVotes.Value = "0";
            }
            else if (type == "refresh")
            {
                setupVotes.SendClients(data);
            }
            


            
        }
        public void setupVotesClients(string data)
        {
            string[] splitData = data.Split('/');
            string playerID = splitData[0].Trim();
            
            Plugin.mls.LogInfo($">>> in refresh: removed playerID: {playerID}");

            Dictionary<string, string> votes = Voting.playersWhoGotVoted.Value;
            votes.Remove(playerID);

            Voting.playersWhoGotVoted.Value = votes;
            Voting.playersWhoVoted.Value = votes;

        }
        public void setupVotesReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the setupVotesReceive:");
            setupVotes.SendServer(data);
        }



        public void ejectPlayerServer(string data, ulong id)
        {
            ejectPlayer.SendClients(data);
        }
        public void ejectPlayerClients(string data)
        {
            if (data == $"{Plugin.localPlayer.playerClientId}")
            {
                EjectPlayers.notsafe = true;
            }

            EjectPlayers.localEject = true;
        }
        public void ejectPlayerReceive(string data, ulong id)
        {
            EjectPlayers.currentlyEjectingPlayer.Value = "true";
            ejectPlayer.SendServer(data);
        }


    }
}
