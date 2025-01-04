using Newtonsoft.Json;

namespace LethalMystery.Utils
{
    public class Data
    {

        private class CMDInfo()
        {
            public List<string>? help { get; set; }
            public List<string>? hosthelp { get; set; }
        }


        public static string GetInfo(string key)
        {
            string filepath = Plugin.MainDir + "\\Assets\\data\\info.json";
            string info = File.ReadAllText(filepath);

            var data = JsonConvert.DeserializeObject<CMDInfo>(info);

            string value = "";
            string prefix = LMConfig.PrefixSetting != null ? LMConfig.PrefixSetting.Value : "/";

            if (data == null)
            {
                return value;
            }

            if (key == "help" && data.help != null)
            {
                foreach (string cmd in data.help)
                {
                    value += cmd.Replace("[PX]", prefix) + "\n";
                }
            }
            else if (key == "hosthelp" && data.hosthelp != null) 
            {
                foreach (string cmd in data.hosthelp)
                {
                    value += cmd.Replace("[PX]", prefix) + "\n";
                }
            }

            return value;

        }

    }
}
