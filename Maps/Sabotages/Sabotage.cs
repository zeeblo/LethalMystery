

namespace LethalMystery.Maps.Sabotages
{
    internal class Sabotage
    {
        public static bool fogTimerStarted = false;
        public static bool generatorFixed = true;

        public static void ResetVars()
        {
            fogTimerStarted = false;
            generatorFixed = true;
        }
    }
}
