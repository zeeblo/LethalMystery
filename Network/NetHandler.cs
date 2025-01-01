using LethalMystery.MainGame;
using LethalMystery.Patches;
using LethalMystery.Players;
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
        private LNetworkMessage<Dictionary<ulong, int>> slots;
        private LNetworkMessage<string> meeting;
        private LNetworkMessage<List<Item>> addScrapsToList;
        private LNetworkMessage<string> destroyScrap;


        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            allPlayerRoles = LNetworkVariable<Dictionary<ulong, string>>.Connect("allPlayerRoles");

            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<Dictionary<ulong, int>>.Connect("Slots");
            meeting = LNetworkMessage<string>.Connect("CallAMeeting");
            addScrapsToList = LNetworkMessage<List<Item>>.Connect("addScrapsToList");
            destroyScrap = LNetworkMessage<string>.Connect("destroyScrap");


            spawnWeapon.OnServerReceived += SpawnWeaponServer;
            slots.OnServerReceived += SlotsServer;
            slots.OnClientReceived += SlotsClients;
            meeting.OnServerReceived += MeetingServer;
            meeting.OnClientReceived += MeetingClients;
            addScrapsToList.OnServerReceived += addScrapsToListServer;
            addScrapsToList.OnClientReceived += addScrapsToListClients;
            destroyScrap.OnServerReceived += destroyScrapServer;
            destroyScrap.OnClientReceived += destroyScrapClients;

        }

        #region Variables

        public void AddCustomNetEvents()
        {
            Plugin.inMeeting.OnValueChanged += InMeeting_Event;
            Plugin.inGracePeriod.OnValueChanged += InGracePeriod_Event;
        }

        public void RemoveCustomNetEvents()
        {
            Plugin.inMeeting.OnValueChanged -= InMeeting_Event;
            Plugin.inGracePeriod.OnValueChanged -= InGracePeriod_Event;
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
                Plugin.MeetingDefaults();
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
            Plugin.localPlayerRoles = allPlayerRoles.Value;

            foreach (KeyValuePair<ulong, string> plrID in allPlayerRoles.Value)
            {
                if (Plugin.localPlayer.actualClientId == plrID.Key)
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
        public void SpawnWeaponReceive(Role data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the SpawnMobReceive");

            if (data.Type == RoleType.monster)
            {
                spawnWeapon.SendServer("knife");
            }
            else if (data.Name == "sheriff")
            {
                spawnWeapon.SendServer("shotgun");
            }

        }



        public void SlotsServer(Dictionary<ulong, int> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsServer:");
            slots.SendClients(data);
        }
        public void SlotsClients(Dictionary<ulong, int> data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsClients:");

            foreach (KeyValuePair<ulong, int> d in data)
            {
                StartOfRound.Instance.allPlayerScripts[d.Key].ItemSlots = new GrabbableObject[d.Value];
            }

        }
        public void SlotsReceive(Dictionary<ulong, int> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsReceive:");
            slots.SendServer(data);
        }



        public void MeetingServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingServer:");

            Plugin.inMeeting.Value = "true";
            Plugin.inGracePeriod.Value = "true";
            Plugin.currentGracePeriodCountdown.Value = $"{Plugin.defaultMeetingCountdown + 140f}";

            meeting.SendClients(data);
        }
        public void MeetingClients(string data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingClients:");

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(StartOfRound.Instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
            Plugin.RemoveEnvironment();
            HUDManagerPatch.DisplayDaysEdit("meeting");
        }
        public void MeetingReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the MeetingReceive:");
            meeting.SendServer(data);
        }



        public void addScrapsToListServer(List<Item> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListServer:");

            addScrapsToList.SendClients(data);
        }
        public void addScrapsToListClients(List<Item> data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListClients:");
            Tasks.allScraps = data;

        }
        public void addScrapsToListReceive(List<Item> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the addScrapsToListReceive:");
            addScrapsToList.SendServer(data);
        }


        public void destroyScrapServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the destroyScrapServer:");
            Plugin.mls.LogInfo($">>> data in svr: {data}");

            destroyScrap.SendClients(data);
            //StartOfRound.Instance.allPlayerScripts[1].DestroyItemInSlot(0);
            // data.NetworkObject.Despawn();


        }
        public void destroyScrapClients(string data)
        {
            Plugin.mls.LogInfo($"disabling: {data}");
            string[] splitData = data.Split('/');
            ulong.TryParse(splitData[0], out ulong playerID);
            Int32.TryParse(splitData[1], out int slot);

            if (StartOfRound.Instance.allPlayerScripts[playerID].currentlyHeldObjectServer != null)
            Plugin.Destroy(StartOfRound.Instance.allPlayerScripts[playerID].currentlyHeldObjectServer.gameObject);

        }
        public void destroyScrapReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the destroyScrapReceive:");
            Plugin.mls.LogInfo($">>> data: {data}");
            destroyScrap.SendServer(data);
        }

    }
}
