﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMystery.MainGame;
using LethalMystery.Maps;
using LethalMystery.Maps.Sabotages;
using LethalMystery.Patches;
using LethalMystery.Players;
using LethalMystery.UI;
using LethalMystery.Utils;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
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
        private LNetworkMessage<string> playerBlood;
        private LNetworkMessage<string> despawnGO;
        private LNetworkMessage<string> currentMap;
        private LNetworkMessage<string> hidePlayer;
        private LNetworkMessage<string> buyItem;
        private LNetworkMessage<string> setWaypoint;
        private LNetworkMessage<string> roundEnd;
        private LNetworkMessage<string> deathInfo;
        private LNetworkMessage<string> sabotage;
        private LNetworkMessage<string> suits;
        private LNetworkMessage<string> resetSlotBg;
        private LNetworkMessage<string> setHostConfigs;

        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            allPlayerRoles = LNetworkVariable<Dictionary<ulong, string>>.Connect("allPlayerRoles");

            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<string>.Connect("Slots");
            meeting = LNetworkMessage<string>.Connect("meeting");
            addScrapsToList = LNetworkMessage<List<string>>.Connect("addScrapsToList");
            destroyScrap = LNetworkMessage<string>.Connect("destroyScrap");
            showScrap = LNetworkMessage<string>.Connect("showScrap");
            hideWeapon = LNetworkMessage<string>.Connect("hideWeapon");
            playerVoted = LNetworkMessage<string>.Connect("playerVoted");
            playerDied = LNetworkMessage<string>.Connect("playerDied");
            setupVotes = LNetworkMessage<string>.Connect("setupVotes");
            ejectPlayer = LNetworkMessage<string>.Connect("ejectPlayer");
            playerBlood = LNetworkMessage<string>.Connect("playerBlood");
            despawnGO = LNetworkMessage<string>.Connect("despawnGO");
            currentMap = LNetworkMessage<string>.Connect("currentMap");
            hidePlayer = LNetworkMessage<string>.Connect("hidePlayer");
            buyItem = LNetworkMessage<string>.Connect("buyItem");
            setWaypoint = LNetworkMessage<string>.Connect("setWaypoint");
            roundEnd = LNetworkMessage<string>.Connect("roundEnd");
            deathInfo = LNetworkMessage<string>.Connect("deathInfo");
            sabotage = LNetworkMessage<string>.Connect("sabotage");
            suits = LNetworkMessage<string>.Connect("suits");
            resetSlotBg = LNetworkMessage<string>.Connect("resetSlotBg");
            setHostConfigs = LNetworkMessage<string>.Connect("setHostConfigs");

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
            playerBlood.OnServerReceived += playerBloodServer;
            playerBlood.OnClientReceived += playerBloodClients;
            despawnGO.OnServerReceived += despawnGOServer;
            currentMap.OnServerReceived += currentMapServer;
            currentMap.OnClientReceived += currentMapClients;
            hidePlayer.OnServerReceived += hidePlayerServer;
            hidePlayer.OnClientReceived += hidePlayerClients;
            buyItem.OnServerReceived += buyItemServer;
            setWaypoint.OnServerReceived += setWaypointServer;
            setWaypoint.OnClientReceived += setWaypointClients;
            roundEnd.OnServerReceived += roundEndServer;
            roundEnd.OnClientReceived += roundEndClients;
            deathInfo.OnServerReceived += deathInfoServer;
            deathInfo.OnClientReceived += deathInfoClients;
            sabotage.OnServerReceived += sabotageServer;
            sabotage.OnClientReceived += sabotageClients;
            suits.OnServerReceived += suitsServer;
            suits.OnClientReceived += suitsClients;
            resetSlotBg.OnServerReceived += resetSlotBgServer;
            resetSlotBg.OnClientReceived += resetSlotBgClients;
            setHostConfigs.OnServerReceived += SetHostConfigsServer;
            setHostConfigs.OnClientReceived += SetHostConfigsClients;
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
                HangarShipDoor ship = Plugin.shipInstance.transform.Find("AnimatedShipDoor").gameObject.GetComponent<HangarShipDoor>();
                ship.PlayDoorAnimation(closed: false);
                ship.SetDoorButtonsEnabled(true);
                ship.doorPower = 0;
                ship.overheated = true;
                ship.triggerScript.interactable = true;

                Plugin.RemoveEnvironment(false);
                GOTools.HideLMInsideDungeon(false);
                Meeting.MeetingDefaults();
                StartOfRound.Instance.mapScreen.SwitchScreenOn(true);
                GOTools.RemoveGameObject("PhysicsProp", "RagdollGrabbableObject");
                StartOfRound.Instance.StartCoroutine(cleanSlot());
                Minimap.allPlayerPoints.Clear();
                Minimap.ClearAllPoints();
                MinimapUI.DestroyUI();
                MinimapUI.CreateMapIcon();
                if (VotingUI.votingGUI != null)
                {
                    Plugin.Destroy(VotingUI.votingGUI.gameObject);
                    Plugin.Destroy(VotingUI.voteIcon);
                }
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            }
        }


        private static IEnumerator cleanSlot()
        {
            yield return new WaitForSeconds(2f);

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            for (int i = 0; i < localPlayer.ItemSlots.Count(); i++)
            {
                if (localPlayer.ItemSlots[i] == null)
                {
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
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




        private void SpawnWeaponServer(string data, ulong id)
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



        private void SlotsServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsServer:");
            slots.SendClients(data);
        }
        private void SlotsClients(string data)
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



        private void MeetingServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingServer:");

            Meeting.inMeeting.Value = "true";
            Start.inGracePeriod.Value = "true";
            Start.currentGracePeriodTime.Value = $"{LMConfig.defaultMeetingTime + 40f}";

            meeting.SendClients(data);
        }
        private void MeetingClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingClients:");
            Start.localGracePeriod = true;
            Minimap.lastPlayerPos = Plugin.localPlayer.transform.position;
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

            yield return new WaitForSeconds(2f);
            VotingUI.isCalled = true;
        }



        private void addScrapsToListServer(List<string> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListServer:");

            addScrapsToList.SendClients(data);
        }
        private void addScrapsToListClients(List<string> data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListClients:");
            Tasks.allScraps = data;

        }
        public void addScrapsToListReceive(List<string> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListReceive:");
            addScrapsToList.SendServer(data);
        }



        private void destroyScrapServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the destroyScrapServer:");
            Plugin.mls.LogInfo($">>> data in svr: {data}");
            string[] splitData = data.Split('/');
            Int32.TryParse(splitData[0], out int playerID);
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
        private void destroyScrapClients(string data)
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




        private void showScrapServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the showScrapServer:");
            showScrap.SendClients(data);
        }
        private void showScrapClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the showScrapClients:");

            if (data.EndsWith("CollectItem"))
            {
                string[] splitData = data.Split('/');
                Int32.TryParse(splitData[0], out int playerIndex);
                ulong.TryParse(splitData[0], out ulong playerID);
                Int32.TryParse(splitData[1], out int slot);

                Plugin.mls.LogInfo(">>> CollectItem part");
                GrabbableObject itm = StartOfRound.Instance.allPlayerScripts[playerIndex].ItemSlots[slot];
                if (itm == null || Plugin.localPlayer.playerClientId == playerID)
                {
                    return;
                }
                HUDManager.Instance.AddNewScrapFoundToDisplay(itm);
            }
            else
            {
                string[] splitData = data.Split('/');
                Int32.TryParse(splitData[0], out int playerID);

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





        private void hideWeaponServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the hideWeaponServer:");

            hideWeapon.SendClients(data);
        }
        private void hideWeaponClients(string data)
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



        private void playerVotedServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the playerVotedServer:");
            string[] splitData = data.Split('/');
            string playerThatVoted = splitData[2];

            Dictionary<string, string> votes = Voting.playersWhoVoted.Value;
            votes[playerThatVoted] = "voted";

            Voting.playersWhoVoted.Value = votes;

            playerVoted.SendClients(data);
        }
        private void playerVotedClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the playerVotedClients:");

            string[] splitData = data.Split('/');
            string type = splitData[0];
            Int32.TryParse(splitData[1], out int userID);
            ulong.TryParse(splitData[2], out ulong playerThatVoted);
            string skipVal = Voting.skipVotes.Value;
            Dictionary<string, string> votes = Voting.playersWhoGotVoted.Value;

            //Plugin.mls.LogInfo($">> type: {type}");
            //Plugin.mls.LogInfo($">> userID: {userID}");
            //Plugin.mls.LogInfo($">> SKIPS: {skipVal}");
            SetVoteBackground(playerThatVoted);

            switch (type)
            {
                case "vote":
                    votes[$"{userID}"] = $"{StringAddons.AddInts(votes[$"{userID}"], 1)}";
                    VotingUI.UpdateVoteNum(userID);
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


        /// <summary>
        /// Set player slot background for player that voted
        /// </summary>
        private static void SetVoteBackground(ulong targetID, bool voted = true)
        {
            Color voteColor = (voted) ? new Color(0.6509f, 0, 0.1431f, 1) : new Color(0.6509f, 0.2091f, 0.0031f, 1);
            foreach (GameObject slot in VotingUI.slotObjects)
            {
                if (slot == null) continue;
                PlayerSlot comp = slot.GetComponent<PlayerSlot>();
                if (comp.playerID == targetID)
                {
                    slot.transform.Find("nametag").GetComponent<Image>().color = voteColor;
                    break;
                }
            }
        }




        private void resetSlotBgServer(string data, ulong id)
        {
            ulong.TryParse(data, out ulong userID);
            // Change "voted" string back to nothing
            Voting.playersWhoVoted.Value[$"{userID}"] = "";
            resetSlotBg.SendClients(data);
        }
        private void resetSlotBgClients(string data)
        {
            ulong.TryParse(data, out ulong userID);
            SetVoteBackground(userID, false);
        }
        public void resetSlotBgReceive(string data, ulong id)
        {
            resetSlotBg.SendServer(data);
            Plugin.mls.LogInfo($">>> resetSlotBg ({Plugin.localPlayer.playerClientId})");
        }







        private void playerDiedServer(string data, ulong id)
        {
            ulong.TryParse(data, out ulong deadID);
            EndGame.aliveCrew.Remove(deadID);
            EndGame.aliveMonsters.Remove(deadID);


            playerDied.SendClients(data);
        }
        private void playerDiedClients(string data)
        {
            Int32.TryParse(data, out int userID);
            ulong.TryParse(data, out ulong deadID);

            Voting.RefreshPlayerVotes($"{userID}");
        }
        public void playerDiedReceive(string data, ulong id)
        {
            playerDied.SendServer(data);
        }


        private void setupVotesServer(string data, ulong id)
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
        private void setupVotesClients(string data)
        {
            string[] splitData = data.Split('/');
            string playerID = splitData[0].Trim();

            Dictionary<string, string> votes = Voting.playersWhoGotVoted.Value;
            votes.Remove(playerID);

            Voting.playersWhoGotVoted.Value = votes;
            Voting.playersWhoVoted.Value = votes;

            Plugin.mls.LogInfo($">>> setupVotesClients() ID: {playerID}");

            foreach (KeyValuePair<string, string> val in votes)
            {
                Plugin.mls.LogInfo($">>> votes Key: {val.Key} | votes Val: {val.Value}");
            }
        }
        public void setupVotesReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the setupVotesReceive:");
            setupVotes.SendServer(data);
        }



        private void ejectPlayerServer(string data, ulong id)
        {
            ejectPlayer.SendClients(data);
        }
        private void ejectPlayerClients(string data)
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


        private void playerBloodServer(string data, ulong id)
        {
            playerBlood.SendClients(data);
        }
        private void playerBloodClients(string data)
        {
            string[] splitData = data.Split('/');
            string type = splitData[1];
            ulong.TryParse(splitData[0], out ulong playerID);

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (type == "clean")
                {
                    if (player.playerClientId != playerID) continue;
                    player.RemoveBloodFromBody();
                }
                else if (type == "blood")
                {
                    if (player.playerClientId != playerID) continue;
                    player.AddBloodToBody();
                }
            }

        }
        public void playerBloodReceive(string data, ulong id)
        {
            playerBlood.SendServer(data);
        }




        private void despawnGOServer(string data, ulong id)
        {
            StartOfRound.Instance.StartCoroutine(DespawnTimer());
        }

        public void despawnGOReceive(string data, ulong id)
        {
            despawnGO.SendServer(data);
        }


        private static IEnumerator DespawnTimer()
        {
            yield return new WaitForSeconds(3);
            Plugin.DespawnEnemies();
        }



        private void currentMapServer(string data, ulong id)
        {
            string type = data.Split('/')[0];
            string mapName = data.Split('/')[1];
            if (type.Contains("custom_map") || type.Contains("game_started"))
            {
                CustomLvl.mapName.Value = data.Split('/')[1];
            }
            else if (type.StartsWith("normal"))
            {
                CustomLvl.mapName.Value = mapName;
            }
            else
            {
                CustomLvl.mapName.Value = "lll_map";
            }

            currentMap.SendClients(data);
        }

        private void currentMapClients(string data)
        {
            string type = data.Split('/')[0];
            string mapName = data.Split('/')[1];

            if (type.Contains("game_started"))
            {
                switch (mapName)
                {
                    case "skeld":
                        CustomLvl.CurrentInside = LMAssets.SkeldMap;
                        break;
                    case "office":
                        CustomLvl.CurrentInside = LMAssets.OfficeMap;
                        break;
                }
            }

            if (type.Contains("normal"))
            {
                CustomLvl.CurrentInside = null;
            }

            StartOfRound.Instance.screenLevelDescription.text = $"Map: {mapName.ToUpper()}";
        }

        public void currentMapReceive(string data, ulong id)
        {
            currentMap.SendServer(data);
        }




        private void hidePlayerServer(string data, ulong id)
        {
            hidePlayer.SendClients(data);
        }
        private void hidePlayerClients(string data)
        {
            string[] splitData = data.Split('/');
            string type = splitData[1];
            ulong.TryParse(splitData[0], out ulong playerID);
            bool value = (type == "hide") ? false : true;

            if (playerID == Plugin.localPlayer.playerClientId) return;

            foreach (PlayerControllerB user in StartOfRound.Instance.allPlayerScripts)
            {
                if (user.playerClientId == playerID)
                {
                    user.gameObject.SetActive(value);

                    if (user.currentlyHeldObjectServer != null)
                    {
                        user.currentlyHeldObjectServer.gameObject.GetComponent<MeshRenderer>().enabled = value;
                    }
                }
            }

        }
        public void hidePlayerReceive(string data, ulong id)
        {
            hidePlayer.SendServer(data);
        }



        private void buyItemServer(string data, ulong id)
        {
            string[] splitData = data.Split('/');
            string orderName = splitData[1];
            ulong.TryParse(splitData[0], out ulong playerID);
            Int32.TryParse(splitData[2], out int amt);
            Transform plrT = StartOfRound.Instance.allPlayerScripts[playerID].transform;

            foreach (Item itm in Plugin.terminal.buyableItemsList)
            {
                if (itm.itemName.Replace("-", " ").ToLower().StartsWith(orderName.ToLower()))
                {
                    Vector3 pos = new Vector3(plrT.position.x, plrT.position.y + 2.3f, plrT.position.z);

                    for (int i = 0; i < amt; i++)
                    {
                        GameObject boughtItem = UnityEngine.Object.Instantiate(itm.spawnPrefab, pos, Quaternion.identity);
                        boughtItem.GetComponent<NetworkObject>().Spawn(true);
                    }

                    return;
                }
            }
        }

        public void buyItemReceive(string data, ulong id)
        {
            buyItem.SendServer(data);
        }




        private void setWaypointServer(string data, ulong id)
        {
            setWaypoint.SendClients(data);
        }
        private void setWaypointClients(string data)
        {
            string pid = data.Split('/')[0];
            string worldPosition = data.Split('/')[1];
            Dictionary<string, string> localPlayerPoint = new Dictionary<string, string>();

            localPlayerPoint = Minimap.allPlayerPoints;
            if (localPlayerPoint.ContainsKey(pid))
            {
                localPlayerPoint[pid] = worldPosition;
            }
            else
            {
                localPlayerPoint.Add(pid, worldPosition);
            }

            Minimap.allPlayerPoints = localPlayerPoint;
            foreach (KeyValuePair<string, string> plrPos in localPlayerPoint)
            {
                Plugin.mls.LogInfo($">>> plrPos: {plrPos.Key}");
                if ($"{Plugin.localPlayer.playerClientId}" == plrPos.Key) continue;
                if (GameObject.Find($"waypoint_{plrPos.Key}"))
                {
                    Plugin.Destroy(GameObject.Find($"waypoint_{plrPos.Key}"));
                }
                Vector3 position = Minimap.GetPlayerPoint(plrPos.Key);
                GameObject mapdot = Plugin.Instantiate(Minimap.waypointPrefab, position, Quaternion.identity);
                mapdot.name = $"waypoint_{plrPos.Key}";
                mapdot.SetActive(true);
                ulong.TryParse(plrPos.Key, out ulong playid);
                MinimapUI.CreatePositionSlot(playid, position);
            }
        }

        public void setWaypointReceive(string data, ulong id)
        {
            setWaypoint.SendServer(data);
        }






        private void roundEndServer(string data, ulong id)
        {
            roundEnd.SendClients(data);
        }
        private void roundEndClients(string data)
        {
            string type = data.Split("/")[0];
            string topText = data.Split("/")[1];
            string bottomText = data.Split("/")[2];

            Plugin.firedText = topText;
            Plugin.firedTextSub = bottomText;
            EndGame.AddLastAlive();

            StartOfRound.Instance.StartCoroutine(endEvents(type));
        }
        public void roundEndReceive(string data, ulong id)
        {
            roundEnd.SendServer(data);
        }


        private static IEnumerator endEvents(string impNames)
        {
            HUDManager.Instance.ShowPlayersFiredScreen(true);
            StartOfRound.Instance.ShipLeaveAutomatically();

            yield return new WaitForSeconds(5f);
            HUDManager.Instance.ShowPlayersFiredScreen(false);

            yield return new WaitForSeconds(15f);
            //HUDManager.Instance.AddChatMessage(impNames, "<color=green>IMPOSTERS</color>");
            Commands.DisplayChatMessage($"Imposters: <color=#FF0000> {impNames}</color>");
        }



        private void deathInfoServer(string data, ulong id)
        {
            deathInfo.SendClients(data);
        }
        private void deathInfoClients(string data)
        {
            string type = data.Split("/")[0];
            ulong.TryParse(data.Split("/")[1], out ulong pid);
            string note = data.Split("/")[2];

            if (Plugin.localPlayer.playerClientId == pid)
            {
                Plugin.localPlayer.DamagePlayer(999); // kill target player
            }

            switch (type)
            {
                case "killedby":
                    EndGame.DeathNote(pid, note);
                    break;
            }

        }
        public void deathInfoReceive(string data, ulong id)
        {
            deathInfo.SendServer(data);
        }



        private void sabotageServer(string data, ulong id)
        {
            sabotage.SendClients(data);
        }
        private void sabotageClients(string data)
        {

            switch (data)
            {
                case "lights/break":
                    StartOfRound.Instance.StartCoroutine(Generator.generatorEvents());
                    break;
                case "lights/fix":
                    Generator.FixGenerator();
                    break;
            }

        }
        public void sabotageReceive(string data, ulong id)
        {
            sabotage.SendServer(data);
        }





        private void suitsServer(string data, ulong id)
        {
            suits.SendClients(data);
        }
        private void suitsClients(string data)
        {
            int currentID = StringAddons.ConvertToInt(data.Split("/")[0]);
            int suitID = StringAddons.ConvertToInt(data.Split("/")[1]);

            UnlockableSuitPatch.playerSuits.Remove(currentID);
            UnlockableSuitPatch.playerSuits.Add(suitID);

        }
        public void suitsReceive(string data, ulong id)
        {
            suits.SendServer(data);
        }



        private void SetHostConfigsServer(string data, ulong id)
        {

            string[] splitData = data.Split("/");
            string key = splitData[0];
            string value = splitData[1];
            
            int.TryParse(splitData[2], out int playerID);

            if (playerID >= 0)
            {
                Plugin.mls.LogInfo($">>> >= 0 SetHostConfigsServer(): {data}");
                string newData = ReConfigSettings(key, (ulong)playerID);
                setHostConfigs.SendClients(newData);
            }
            else
            {
                Plugin.mls.LogInfo($">>> -1 SetHostConfigsServer(): {data}");
                setHostConfigs.SendClients(data);
            }

        }
        private void SetHostConfigsClients(string data)
        {
            string[] splitData = data.Split("/");
            string key = splitData[0];
            string value = splitData[1];
            ulong.TryParse(splitData[2], out ulong playerID);

            
            if (Plugin.localID != playerID && playerID >= 0) return;



            Plugin.mls.LogInfo($">>> SetHostConfigsClients(): {data}");

            switch (key)
            {
                case "Imposter Amount":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostImposterAmt = StringAddons.ConvertToMonsterAmt(value);
                    break;
                case "Sheriff Amount":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostSheriffAmt = StringAddons.ConvertToSheriffAmt(value);
                    break;
                case "Discuss Time":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostDiscussTime = StringAddons.ConvertToFloat(value);
                    break;
                case "Vote Time":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostVoteTime = StringAddons.ConvertToFloat(value);
                    break;
                case "Meeting Cooldown":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostMeetingCooldown = StringAddons.ConvertToFloat(value);
                    break;
                case "Meeting Amount":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostMeetingNum = StringAddons.ConvertToInt(value);
                    break;
                case "Grace Period":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostGracePeriodTime = StringAddons.ConvertToFloat(value);
                    break;
                case "Kill Cooldown":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostKillCooldown = StringAddons.ConvertToFloat(value);
                    break;
                case "Scrap Spawn Timer":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostScrapTimer = StringAddons.ConvertToFloat(value);
                    break;
                case "Enable Chat":
                    Plugin.mls.LogInfo($">>> SetHostConfigsClients() SWITCH: {key}");
                    Start.hostEnableChat = StringAddons.ConvertToBool(value);
                    break;
            }

        }
        public void SetHostConfigsReceive(string data)
        {
            setHostConfigs.SendServer(data);
        }


        private static string ReConfigSettings(string key, ulong playerID)
        {
            string new_path = "";
            foreach (KeyValuePair<ConfigDefinition, ConfigEntryBase> entry in LMConfig._config)
            {
                string hostSection = entry.Key.Section;
                string hostKey = entry.Key.Key;
                string hostValue = $"{entry.Value.BoxedValue}";

                if (hostSection != "Host Settings") continue;

                Plugin.mls.LogInfo($"hostKey - {hostKey} | givenKey - {key}");

                if (hostKey == key)
                {
                    new_path += $"{hostKey}/{hostValue}/{playerID}";

                    Plugin.mls.LogInfo($"ADDED | hostKey - {hostKey} | givenKey - {key}");
                    break;
                }

            }
            return new_path;
        }

    }
}
