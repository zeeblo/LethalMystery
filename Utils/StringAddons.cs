using LethalMystery.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalMystery.Utils
{
    public class StringAddons
    {
        public static readonly Dictionary<char, string> convertableChars = new Dictionary<char, string>()
        {
            {
                ';', "&/59"
            },
            {
                '=', "&/61"
            },
            {
                '[', "&/91"
            },
            {
                ']', "&/93"
            }

        };




        /// <summary>
        /// Reset the command prefix to the default
        /// </summary>
        public static string UseDefaultPrefix()
        {
            string defaultPrefix = "/";
            if (Plugin.PrefixSetting != null)
            {
                Plugin.PrefixSetting.Value = defaultPrefix;
                Plugin.PrefixSetting.ConfigFile.Save();
            }
            
            Plugin.mls.LogInfo("setting prefix back to /");
            return defaultPrefix;
        }


        /// <summary>
        /// Converts the input (;) into a symbol (&/59)
        /// </summary>
        public static string ConvertToSymbols(string input)
        {
            foreach (char c in convertableChars.Keys)
            {
                if (c.ToString() == input)
                {
                    return convertableChars[c];
                }
            }
            return input;
        }

        /// <summary>
        /// Converts the input (&/59) into a prefix (;)
        /// </summary>
        public static string ConvertToPrefix(string input)
        {
            foreach (char c in convertableChars.Keys)
            {
                if (convertableChars[c] == input)
                {
                    return c.ToString();
                }
            }
            return input;
        }


        /// <summary>
        /// Check if the value given is convertable to a symbol or a prefix
        /// </summary>
        public static bool InConvertableChars(string input = "", string prefix = "")
        {
            Plugin.mls.LogInfo(">>>chk 3");
            if (!string.IsNullOrEmpty(input))
            {
                Plugin.mls.LogInfo(">>>chk 4");
                // checks if the "input" matches the value of the key. eg: &/59
                foreach (char c in convertableChars.Keys)
                {
                    if (convertableChars[c] == input)
                    {
                        Plugin.mls.LogInfo(">>>chk 5");
                        return true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(prefix))
            {
                Plugin.mls.LogInfo(">>>chk 6");
                // checks if the "prefix" matches the key. eg: ;
                foreach (char c in convertableChars.Keys)
                {
                    if (c.ToString() == prefix)
                    {
                        Plugin.mls.LogInfo(">>>chk 7");
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Do a number of checks to determine if the prefix is valid
        /// </summary>
        public static string CleanPrefix(string prefix)
        {
            Plugin.mls.LogInfo(">>>chk 9");
            if (string.IsNullOrEmpty(prefix))
            {
                Plugin.mls.LogInfo(">>prefix is null");
                return UseDefaultPrefix();
            }

            if (InConvertableChars(prefix: prefix.ToString()))
            {
                Plugin.mls.LogInfo(">>using symbol");
                return prefix;
            }

            if (prefix.Length > 1)
            {
                Plugin.mls.LogInfo($">>input length is to: {prefix.Length} (too big)");
                return UseDefaultPrefix();
            }
            Plugin.mls.LogInfo($">>>chk 10 | prefix: \"{prefix}\"");
            return prefix;

        }



        /// <summary>
        /// Check if the prefix being used in the chat is the same
        /// as the one saved in the .cfg
        /// </summary>
        public static bool CheckPrefix(string prefix)
        {
            Plugin.mls.LogInfo(">>>chk 1");
            if (Plugin.PrefixSetting != null && !string.IsNullOrEmpty(prefix.Trim()) )
            {
                Plugin.mls.LogInfo(">>>chk 2");
                if ( (prefix == Plugin.PrefixSetting.Value) || (StringAddons.InConvertableChars(prefix: prefix)))
                {
                    Plugin.mls.LogInfo(">>>chk 8");
                    prefix = CleanPrefix(prefix);
                    Plugin.mls.LogInfo($">>>chk done | prefix: \"{prefix}\"");
                    return ConvertToSymbols(prefix) == Plugin.PrefixSetting.Value;
                }
            }

            Plugin.mls.LogInfo(">>>chk 11");
            return false;
        }


        /// <summary>
        /// Capitalize the first letter
        /// </summary>
        public static string Title(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return char.ToUpper(input[0]) + input.Substring(1);
        }


        /// <summary>
        /// Check if the INPUT is a whitelisted item
        /// </summary>
        public static bool ContainsWhitelistedItem(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            foreach (string item in Roles.allItems)
            {
                if (item.Contains(input.ToLower()))
                    return true;
            }

            return false;
        }

        public static bool ContainsString(string input, List<string> array)
        {
            foreach (string item in array)
            {
                if (item.Contains(input.ToLower()))
                    return true;
            }

            return false;
        }
    }
}
