using BepInEx.Configuration;


namespace LethalMystery.Utils
{
    public class LMConfig
    {
        public static ConfigEntry<string> PrefixSetting;
        public static ConfigEntry<string> shapeshiftBind;
        public static ConfigEntry<string> selfcleanBind;
        public static ConfigEntry<string> spawnItemBind;
        public static List<ConfigEntry<string>> AllHotkeys = new List<ConfigEntry<string>>();
        public static float defaultDiscussTime = 15f; // 35
        public static float defaultVoteTime = 95f; // 85
        public static float defaultMeetingTime = defaultDiscussTime + defaultVoteTime + 15f;
        public static float defaultMeetingCooldown = 10f; // 10
        public static int defaultMeetingNum = 3; // 1
        public static float defaultGracePeriodTime = 20f; //80f;
        public static float defaultKillCooldown = 20f;
        public static float defaultScrapTimer = 15f; // 15

        public static void AllConfigs(ConfigFile cfg)
        {
            PrefixSetting = cfg.Bind("Command Settings", "Command Prefix", "/", "Prefix for chat commands");
            shapeshiftBind = cfg.Bind("Monster Controls", "Shapeshift", "8", "Disguise yourself");
            selfcleanBind = cfg.Bind("Monster Controls", "Self Clean", "f", "Clean the blood on you");
            spawnItemBind = cfg.Bind("Player Controls", "Spawn Item", "t", "Spawn your role specific item");

            AllHotkeys.Add(PrefixSetting);
            AllHotkeys.Add(shapeshiftBind);
            AllHotkeys.Add(selfcleanBind);
            AllHotkeys.Add(spawnItemBind);

        }

    }
}
