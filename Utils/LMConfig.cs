using BepInEx.Configuration;


namespace LethalMystery.Utils
{
    public class LMConfig
    {
        public static ConfigEntry<string>? PrefixSetting;
        public static ConfigEntry<string>? shapeshiftBind;
        public static List<ConfigEntry<string>> AllHotkeys = new List<ConfigEntry<string>>();

        public static void AllConfigs(ConfigFile cfg)
        {
            PrefixSetting = cfg.Bind("Command Settings", "Command Prefix", "/", "Prefix for chat commands");
            shapeshiftBind = cfg.Bind("Gameplay Controls", "Shapeshift", "8", "Disguise yourself");
            AllHotkeys.Add(PrefixSetting);
            AllHotkeys.Add(shapeshiftBind);

            //cfg.Save();
            //cfg.SaveOnConfigSet = true;
        }

    }
}
