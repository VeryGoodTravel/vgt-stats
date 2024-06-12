using Newtonsoft.Json;

namespace vgt_stats.Models;

public class StatsRequestHttp
{
    [JsonProperty("offer_id")]
    public string OfferId { get; set; }
}