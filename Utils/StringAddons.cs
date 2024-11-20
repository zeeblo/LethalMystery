using LethalMystery.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalMystery.Utils
{
    public class StringAddons
    {

        public static bool CheckPrefix(string text)
        {
            string prefix = "/";

            if (Plugin.PrefixSetting?.Value != "" && Plugin.PrefixSetting != null)
            {
                prefix = Plugin.PrefixSetting.Value;
            }
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text[0] == prefix[0];
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
    }
}
