
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

        public int StartTimeOut { get; set; } = 30;

        public bool ShortDrop { get; set; } = true;
        
        public int CoolDownAfterLoginError { get; set; } = 120;

        public static void Load()
        {
            var obj = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(ConfigPath));
            Config = obj;
        }
    }
}
