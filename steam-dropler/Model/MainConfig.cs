
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace steam_dropler.Model
{
    public class MainConfig
    {
        private const string ConfigPath = "Configs\\MainConfig.json";

        [JsonIgnore]
        public static MainConfig Config { get; set; }

        public string MaFileFolder { get; set; }

        public string DropHistoryFolder { get; set; }

        public int ParallelCount { get; set; }

        public TimeConfig TimeConfig { get; set; }

        public static void Load()
        {
            var obj = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(ConfigPath));
            Config = obj;
        }
    }
}
