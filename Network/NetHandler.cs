using GameNetcodeStuff;
using LethalMystery.Players;
using LethalNetworkAPI;
using UnityEngine;
using static LethalMystery.Players.Roles;


namespace LethalMystery.Network
{
    public class NetHandler
    {
        private LNetworkMessage<string> customServerMessage;
        private LNetworkMessage<string> spawnWeapon;
        private LNetworkMessage<int> slots;
        private LNetworkMessage<string> tst;

        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            customServerMessage = LNetworkMessage<string>.Connect("customServ");
            spawnWeapon = LNetworkMessage<string>.Connect("SpawnWeapons");
            slots = LNetworkMessage<int>.Connect("Slots");
            tst = LNetworkMessage<string>.Connect("tst");


            slots.OnServerReceived += SlotsServer;
            slots.OnClientReceived += SlotsClients;

            spawnWeapon.OnServerReceived += SpawnWeaponServer;

            customServerMessage.OnServerReceived += GreetingsServer;
            customServerMessage.OnClientReceived += ReceiveFromClient;
            tst.OnServerReceived += tstServer;

        }





        public void SlotsServer(int data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsServer:");
            slots.SendClients(data);

        }
        public void SlotsClients(int data)
        {
            Plugin.mls.LogInfo($"<><><> I am in the slotsClients:");
            foreach (PlayerControllerB plr in StartOfRound.Instance.allPlayerScripts)
            {
                Plugin.mls.LogInfo($">>> PlayerID: {plr.playerClientId}");
                plr.ItemSlots = new GrabbableObject[5];
            }
        }
        public void SlotsReceive(int data, ulong id)
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



        public void GreetingsServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the SERVER: {data}");
            customServerMessage.SendClients(data);

        }
        public void ReceiveFromClient(string data)
        {
            Plugin.mls.LogInfo("<><><> every client has recieved this:");
        }
        public void ReceiveByServer(string data, ulong id)
        {
            Plugin.mls.LogInfo("<><><> I am in the CLIENT");
            customServerMessage.SendServer("thing");
        }


        public void tstServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the tstServer:");
            foreach (PlayerControllerB plr in StartOfRound.Instance.allPlayerScripts)
            {
                Plugin.mls.LogInfo($">>> PlayerID: {plr.playerClientId}");
            }
        }
        public void tstClients(string data)
        {

        }
        public void tstReceive(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the tstReceive:");
            tst.SendServer(data);
        }

    }
}
