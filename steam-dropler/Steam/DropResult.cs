using Newtonsoft.Json;

namespace steam_dropler.Steam
{
    public class DropResult
    {
        public string AccountId { get; set; }

        public string ItemId { get; set; }

        public int Quantity { get; set; }

        public string OriginalItemId { get; set; }

        public string ItemDefId { get; set; }

        public int AppId { get; set; }

        public string Acquired { get; set; }

        public string State { get; set; }

        public string Origin { get; set; }

        [JsonProperty("state_changed_timestamp")]
        public string StateChangedTimestamp { get; set; }
    }
}
