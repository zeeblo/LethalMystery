using LethalMystery.GameMech;


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
            if (!string.IsNullOrEmpty(input))
            {
                // checks if the "input" matches the value of the key. eg: &/59
                foreach (char c in convertableChars.Keys)
                {
                    if (convertableChars[c] == input)
                    {
                        return true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(prefix))
            {
                // checks if the "prefix" matches the key. eg: ;
                foreach (char c in convertableChars.Keys)
                {
                    if (c.ToString() == prefix)
                    {
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
            if (string.IsNullOrEmpty(prefix))
            {
                return UseDefaultPrefix();
            }

            if (InConvertableChars(prefix: prefix.ToString()))
            {
                return prefix;
            }

            if (prefix.Length > 1)
            {
                return UseDefaultPrefix();
            }

            return prefix;

        }



        /// <summary>
        /// Check if the prefix being used in the chat is the same
        /// as the one saved in the .cfg
        /// </summary>
        public static bool CheckPrefix(string prefix)
        {
            if (Plugin.PrefixSetting != null && !string.IsNullOrEmpty(prefix.Trim()) )
            {
                if ( (prefix == Plugin.PrefixSetting.Value) || (StringAddons.InConvertableChars(prefix: prefix)))
                {
                    prefix = CleanPrefix(prefix);
                    return ConvertToSymbols(prefix) == Plugin.PrefixSetting.Value;
                }
            }

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

            foreach (Item item in Tasks.allScraps)
            {
                if (item.itemName.ToLower().Contains(input.ToLower()))
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
