using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalAPI.LibTerminal;
using LethalAPI.LibTerminal.Attributes;
using LethalAPI.LibTerminal.Models;
using LethalMystery.Patches;
using LethalNetworkAPI;




/*
 * - Add Start command
 * ! Have a GUI that says if they're the monster/crew & if they're shapeshifter (Stays on whole game)
 * - Have the speaker dialouge tell players their role instead.
 * - Have a chat commands that will tell the users their role & what they do
 * - Imposters can spawn their weapon (Using config not Keyboard)
 * - 
*/




/*
 * Win Game Mechanics
 * - Top text will say (Monsters/Employees won).
 * - If monsters won the bottom text will say the names of the 1 or 2 imps
*/


namespace LethalMystery
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("LethalNetworkAPI")]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "syntax_z.LethalMystery";
        private const string modName = "syntax_z.LethalMystery";
        private const string modVersion = "0.1.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? Instance;
        internal ManualLogSource? logger;
        private TerminalModRegistry? TCommands;


        public static readonly LethalServerMessage<string> customServerMessage = new LethalServerMessage<string>(identifier: "LethalMystery", ReceiveByServer);
        public static readonly LethalClientMessage<string> customClientMessage = new LethalClientMessage<string>(identifier: "LethalMystery", ReceiveFromServer, ReceiveFromClient);


        private void Awake()
        {
            PatchAllStuff();

            TCommands = TerminalRegistry.CreateTerminalRegistry();
            TCommands.RegisterFrom(this);
        }


        private void PatchAllStuff()
        {
            //_harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(RoundManagerPatch));
            _harmony.PatchAll(typeof(StartMatchLeverPatch));
            _harmony.PatchAll(typeof(UnlockableSuitPatch));
        }



        private static void ReceiveFromServer(string data)
        {

        }
        private static void ReceiveFromClient(string data, ulong id)
        {

        }
        private static void ReceiveByServer(string data, ulong id)
        {

        }


    }
}