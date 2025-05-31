using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using UnityEngine;
using LethalMystery.Players;
using static LethalMystery.Players.Roles;
using static Steamworks.InventoryItem;


namespace LethalMystery.Utils
{
    public class LMConfig
    {
        public static ConfigFile _config;
        [Header("Keybinds")]
        public static ConfigEntry<string> PrefixSetting;
        public static ConfigEntry<string> shapeshiftBind;
        public static ConfigEntry<string> selfcleanBind;
        public static ConfigEntry<string> spawnItemBind;
        public static ConfigEntry<string> showMapBind;
        public static ConfigEntry<string> showVoteBind;
        public static List<ConfigEntry<string>> AllHotkeys = new List<ConfigEntry<string>>();
        [Header("Host Settings")]
        public static ConfigEntry<MonsterAmt> defaultImposterAmt;
        public static ConfigEntry<SheriffAmt> defaultSheriffAmt;
        public static ConfigEntry<float> defaultDiscussTime; // 35
        public static ConfigEntry<float> defaultVoteTime; // 95
        public static float defaultMeetingTime;
        public static ConfigEntry<float> defaultMeetingCooldown; // 10
        public static ConfigEntry<int> defaultMeetingNum; // 1
        public static ConfigEntry<float> defaultGracePeriodTime; //20f;
        public static ConfigEntry<float> defaultKillCooldown; // 20f
        public static ConfigEntry<float> defaultScrapTimer; // 15f
        public static ConfigEntry<bool> enableChat; // false

        public enum MonsterAmt
        {
            One,
            Two
        }

        public enum SheriffAmt
        {
            Zero,
            One,
            Two
        }


        public static void AllConfigs(ConfigFile cfg)
        {
            _config = cfg;
            PrefixSetting = cfg.Bind("Command Settings", "Command Prefix", "/", "Prefix for chat commands");
            shapeshiftBind = cfg.Bind("Monster Controls", "Shapeshift", "8", "Disguise yourself");
            selfcleanBind = cfg.Bind("Monster Controls", "Self Clean", "f", "Clean the blood on you");
            spawnItemBind = cfg.Bind("Player Controls", "Spawn Item", "t", "Spawn your role specific item");
            showMapBind = cfg.Bind("Player Controls", "Show Map", "m", "A full view of the map");
            showVoteBind = cfg.Bind("Player Controls", "Show Votes", "v", "Displays everyone that can be voted");

            AllHotkeys.Add(PrefixSetting);
            AllHotkeys.Add(shapeshiftBind);
            AllHotkeys.Add(selfcleanBind);
            AllHotkeys.Add(spawnItemBind);
            AllHotkeys.Add(showMapBind);
            AllHotkeys.Add(showVoteBind);

            defaultImposterAmt = cfg.Bind("Host Settings", "Imposter Amount", MonsterAmt.One, "Number of imposters");
            defaultSheriffAmt = cfg.Bind("Host Settings", "Sheriff Amount", SheriffAmt.One, "Number of Sheriffs");
            defaultDiscussTime = cfg.Bind("Host Settings", "Discuss Time", 35f);
            defaultVoteTime = cfg.Bind("Host Settings", "Vote Time", 95f);
            defaultMeetingCooldown = cfg.Bind("Host Settings", "Meeting Cooldown", 10f, "Time until you can call a meeting again");
            defaultMeetingNum = cfg.Bind("Host Settings", "Meeting Amount", 1, "Amount of times a specific user can call a meeting");
            defaultGracePeriodTime = cfg.Bind("Host Settings", "Grace Period", 20f, "Time until people can take damage after a meeting and when a round first starts");
            defaultKillCooldown = cfg.Bind("Host Settings", "Kill Cooldown", 20f, "Time until monsters can instakill");
            defaultScrapTimer = cfg.Bind("Host Settings", "Scrap Spawn Timer", 15f, "Time until scraps will spawn on custom moons");
            enableChat = cfg.Bind("Host Settings", "Enable Chat", false, "Allow players to type in chat during a round");

            defaultImposterAmt.SettingChanged += DefaultImposterAmt_SettingChanged;
            defaultSheriffAmt.SettingChanged += DefaultSheriffAmt_SettingChanged;
            defaultDiscussTime.SettingChanged += DefaultDiscussTime_SettingChanged;
            defaultVoteTime.SettingChanged += DefaultVoteTime_SettingChanged;
            defaultMeetingCooldown.SettingChanged += DefaultMeetingCooldown_SettingChanged;
            defaultMeetingNum.SettingChanged += DefaultMeetingNum_SettingChanged;
            defaultGracePeriodTime.SettingChanged += DefaultGracePeriodTime_SettingChanged;
            defaultKillCooldown.SettingChanged += DefaultKillCooldown_SettingChanged;
            defaultScrapTimer.SettingChanged += DefaultScrapTimer_SettingChanged;
            enableChat.SettingChanged += EnableChat_SettingChanged;

            LethalConfigAddons();

        }



        private static void LethalConfigAddons()
        {
            LethalConfigManager.SkipAutoGen();

            var localImposterAmt = new EnumDropDownConfigItem<MonsterAmt>(defaultImposterAmt, requiresRestart: false);
            var localSheriffAmt = new EnumDropDownConfigItem<SheriffAmt>(defaultSheriffAmt, requiresRestart: false);
            var localDiscussTime = new FloatSliderConfigItem(defaultDiscussTime, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 360
            });
            var localVoteTime = new FloatSliderConfigItem(defaultVoteTime, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 360
            });
            var localMeetingCooldown = new FloatSliderConfigItem(defaultMeetingCooldown, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 180
            });
            var localMeetingNum = new IntInputFieldConfigItem(defaultMeetingNum, new IntInputFieldOptions
            {
                RequiresRestart = false,
                Min = 0,
                Max = 5
            });
            var localGracePeriodTime = new FloatSliderConfigItem(defaultGracePeriodTime, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 120
            });
            var localKillCooldown = new FloatSliderConfigItem(defaultKillCooldown, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 60
            });
            var localScrapTimer = new FloatSliderConfigItem(defaultScrapTimer, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 10,
                Max = 60
            });
            var localEnableChat = new BoolCheckBoxConfigItem(enableChat, requiresRestart: false);


            LethalConfigManager.AddConfigItem(localImposterAmt);
            LethalConfigManager.AddConfigItem(localSheriffAmt);
            LethalConfigManager.AddConfigItem(localDiscussTime);
            LethalConfigManager.AddConfigItem(localVoteTime);
            LethalConfigManager.AddConfigItem(localMeetingCooldown);
            LethalConfigManager.AddConfigItem(localMeetingNum);
            LethalConfigManager.AddConfigItem(localGracePeriodTime);
            LethalConfigManager.AddConfigItem(localKillCooldown);
            LethalConfigManager.AddConfigItem(localScrapTimer);
            LethalConfigManager.AddConfigItem(localEnableChat);


        }



        public static void SetHostConfigs(int playerID = -1)
        {
            foreach (KeyValuePair<ConfigDefinition, ConfigEntryBase> entry in LMConfig._config)
            {
                string section = entry.Key.Section;
                string key = entry.Key.Key;
                string value = $"{entry.Value.BoxedValue}";

                if (section != "Host Settings") continue;
                Plugin.netHandler.SetHostConfigsReceive($"{key}/{value}/{playerID}");
            }
        }



        private static void DefaultImposterAmt_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Roles.specialRoleAmount["monster"] = (int)defaultImposterAmt.Value;
            }
        }

        private static void DefaultSheriffAmt_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Roles.specialRoleAmount["sheriff"] = (int)defaultSheriffAmt.Value;
            }
        }

        private static void DefaultDiscussTime_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Discuss Time/{defaultDiscussTime.Value}/-1");
            }
        }

        private static void DefaultVoteTime_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Vote Time/{defaultVoteTime.Value}/-1");
            }   
        }

        private static void DefaultMeetingCooldown_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Meeting Cooldown/{defaultMeetingCooldown.Value}/-1");
            }
        }

        private static void DefaultMeetingNum_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Meeting Amount/{defaultMeetingNum.Value}/-1");
            }
        }

        private static void DefaultGracePeriodTime_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Grace Period/{defaultGracePeriodTime.Value}/-1");
            }
        }


        private static void DefaultKillCooldown_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Kill Cooldown/{defaultKillCooldown.Value}/-1");
            }
        }


        private static void DefaultScrapTimer_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Scrap Spawn Timer/{defaultScrapTimer.Value}/-1");
            }
        }

        private static void EnableChat_SettingChanged(object sender, EventArgs e)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase)
            {
                Plugin.netHandler.SetHostConfigsReceive($"Enable Chat/{enableChat.Value}/-1");
            }
        }

    }
}
