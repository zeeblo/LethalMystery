using BepInEx.Configuration;


namespace LethalMystery.Utils
{
    public class LMConfig
    {
        public static ConfigEntry<string>? PrefixSetting;
        public static ConfigEntry<string>? shapeshiftBind;
        public static List<ConfigEntry<string>> AllHotkeys = new List<ConfigEntry<string>>();
        public static float defaultDiscussTime = 15f; // 35
        public static float defaultVoteTime = 95f; // 85
        public static float defaultMeetingTime = defaultDiscussTime + defaultVoteTime + 15f;
        public static float defaultMeetingCooldown = 10f; // 10
        public static int defaultMeetingNum = 3; // 1
        public static float defaultGracePeriodTime = 20f; //80f;

        public static void AllConfigs(ConfigFile cfg)
        {
            PrefixSetting = cfg.Bind("Command Settings", "Command Prefix", "/", "Prefix for chat commands");
            shapeshiftBind = cfg.Bind("Gameplay Controls", "Shapeshift", "8", "Disguise yourself");
            AllHotkeys.Add(PrefixSetting);
            AllHotkeys.Add(shapeshiftBind);

        }

    }
}
