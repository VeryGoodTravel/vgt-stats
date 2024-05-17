using Newtonsoft.Json;

namespace vgt_saga_flight.Models;

/// <summary>
/// Represents data from scrapper's json concerning airports 
/// </summary>
public record struct Airport()
{
    /// <summary>
    /// Code of the airport
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }
    
    /// <summary>
    /// Name of the airport
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
};