
using GameNetcodeStuff;
using LethalMystery.Players;
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


        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            allPlayerRoles = LNetworkVariable<Dictionary<ulong, string>>.Connect("allPlayerRoles");

            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<Dictionary<ulong, int>>.Connect("Slots");

            slots.OnServerReceived += SlotsServer;
            slots.OnClientReceived += SlotsClients;

            spawnWeapon.OnServerReceived += SpawnWeaponServer;


        }

        #region Variables

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




        public void SlotsServer(Dictionary<ulong, int> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsServer:");
            slots.SendClients(data);
        }
        public void SlotsClients(Dictionary<ulong, int> data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsClients:");

            foreach(KeyValuePair<ulong, int> d in data)
            {
                StartOfRound.Instance.allPlayerScripts[d.Key].ItemSlots = new GrabbableObject[d.Value];
            }
            

        }
        public void SlotsReceive(Dictionary<ulong, int> data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsReceive:");
            slots.SendServer(data);
        }



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


    }
}
