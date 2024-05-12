using Newtonsoft.Json;

namespace vgt_saga_hotel.Models;

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

/// <summary>
/// Represents data from scrapper's json concerning amounts of people required and allowed in the rooms 
/// </summary>
public record struct Range()
{
    /// <summary>
    /// Minimal amount of people required to rent the room
    /// </summary>
    [JsonProperty("min")]
    public int Min { get; set; }
    
    /// <summary>
    /// Maximal amount of people allowed in the room
    /// </summary>
    [JsonProperty("max")]
    public int Max { get; set; }
};

/// <summary>
/// Represents data from scrapper's json concerning Rooms in the hotels 
/// </summary>
public record struct Room()
{
    /// <summary>
    /// Name of the room type
    /// </summary>
    [JsonProperty("name")] 
    public string Name { get; set; }
    
    /// <summary>
    /// Ranges of people amounts
    /// </summary>
    [JsonProperty("people")] 
    public Range People { get; set; }
    
    /// <summary>
    /// Ranges of adults amounts
    /// </summary>
    [JsonProperty("adults")] 
    public Range Adults { get; set; }
    
    /// <summary>
    /// Ranges of children amounts
    /// </summary>
    [JsonProperty("children")] 
    public Range Children { get; set; }
};

/// <summary>
/// Represents data from scrapper's json concerning Hotels 
/// </summary>
public record struct Hotel()
{
    /// <summary>
    /// Name of the hotel
    /// </summary>
    [JsonProperty("name")] 
    public string Name { get; set; }
    
    /// <summary>
    /// Country the hotel is in
    /// </summary>
    [JsonProperty("country")] 
    public string Country { get; set; }
    
    /// <summary>
    /// City the hotel is assigned to
    /// </summary>
    [JsonProperty("city")] 
    public string City { get; set; }
    
    /// <summary>
    /// Airport the hotel is assigned to
    /// </summary>
    [JsonProperty("airport")] 
    public Airport Airport { get; set; }
    
    /// <summary>
    /// List of room types offered by the hotel and their amounts
    /// </summary>
    [JsonProperty("rooms")] 
    public List<Room> Rooms { get; set; }
};