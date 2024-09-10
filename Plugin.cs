using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalMystery.Patches;




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
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "zeeblo.LethalMystery";
        private const string modName = "zeeblo.LethalMystery";
        private const string modVersion = "0.1.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? Instance;
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);


        private void Awake()
        {
            PatchAllStuff();

        }


        private void PatchAllStuff()
        {
            //_harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(RoundManagerPatch));
            _harmony.PatchAll(typeof(StartMatchLeverPatch));
            _harmony.PatchAll(typeof(UnlockableSuitPatch));
            _harmony.PatchAll(typeof(HUDManagerPatch));
        }




    }
}