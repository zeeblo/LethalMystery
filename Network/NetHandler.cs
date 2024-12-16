using LethalNetworkAPI;


namespace LethalMystery.Network
{
    public class NetHandler
    {
        public LNetworkMessage<string> customServerMessage;

        public NetHandler()
        {
            Plugin.mls.LogInfo(">>> Making network stuff");
            customServerMessage = LNetworkMessage<string>.Connect("customServ");
            customServerMessage.OnServerReceived += GreetingsServer;
            customServerMessage.OnClientReceived += ReceiveFromClient;
        }
        

        public void GreetingsServer(string data, ulong id)
        {
            Plugin.mls.LogInfo($"<><><> I am in the SERVER: {data}");
            customServerMessage.SendClients(data);

        }
        public void ReceiveFromClient(string data)
        {
            Plugin.mls.LogInfo("<><><> every client has recieves this:");
        }
        public void ReceiveByServer(string data, ulong id)
        {
            Plugin.mls.LogInfo("<><><> I am in the CLIENT");
            customServerMessage.SendServer("thing");
        }

    }
}
