using LethalMystery.Patches;
using LethalMystery.Players;
using LethalMystery.Utils;
using LethalNetworkAPI;
using static LethalMystery.Players.Roles;


namespace LethalMystery.Network
{
    public class NetHandler
    {
        private static LNetworkVariable<Dictionary<ulong, string>> allPlayerRoles;
        private LNetworkMessage<string> spawnWeapon;
        private LNetworkMessage<Dictionary<ulong, int>> slots;
        private LNetworkMessage<string> meeting;


        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            allPlayerRoles = LNetworkVariable<Dictionary<ulong, string>>.Connect("allPlayerRoles");

            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<Dictionary<ulong, int>>.Connect("Slots");
            meeting = LNetworkMessage<string>.Connect("CallAMeeting");


            spawnWeapon.OnServerReceived += SpawnWeaponServer;
            slots.OnServerReceived += SlotsServer;
            slots.OnClientReceived += SlotsClients;
            meeting.OnServerReceived += MeetingServer;
            meeting.OnClientReceived += MeetingClients;

        }

        #region Variables

        public void AddCustomNetEvents()
        {
            Plugin.mls.LogInfo("<<< Added atInMeeting");
            Plugin.inMeeting.OnValueChanged += InMeeting_Event;
            Plugin.inGracePeriod.OnValueChanged += InGracePeriod_Event;
        }

        public void RemoveCustomNetEvents()
        {
            Plugin.mls.LogInfo("<<< Removing atInMeeting");
            Plugin.inMeeting.OnValueChanged -= InMeeting_Event;
            Plugin.inGracePeriod.OnValueChanged -= InGracePeriod_Event;
        }


        private static void InMeeting_Event(string idk, string data)
        {
            Plugin.mls.LogInfo("<><> In atInMeeting");
            Plugin.mls.LogInfo($"<><> data: {data}");

            if (StringAddons.ConvertToBool(data) == false)
            {
                Plugin.mls.LogInfo(">>> Attempting to stop MEEEEETING");
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

    }
}
