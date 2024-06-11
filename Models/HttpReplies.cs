using Newtonsoft.Json;

namespace vgt_stats.Models;

public class TravelLocation
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("locations", NullValueHandling = NullValueHandling.Ignore)]
    public TravelLocation[] Locations { get; set; }
}

public class Direction
{
    [JsonProperty("origin")]
    public TravelLocation Origin { get; set; }
    
    [JsonProperty("destination")]
    public TravelLocation Destination { get; set; }

    public static Direction GetExample()
    {
        return new Direction
        {
            Origin = new TravelLocation
            {
                Id = "21",
                Label = "Warsaw",
            },
            Destination = new TravelLocation
            {
                Id = "20",
                Label = "New York",
            }
        };
    }
}

public class Accommodation
{
    [JsonProperty("destination")]
    public TravelLocation Destination { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("room")]
    public string Room { get; set; }
    
    [JsonProperty("transportation")]
    public string Transportation { get; set; }
    
    [JsonProperty("maintenance")]
    public string Maintenance { get; set; }
    
    [JsonProperty("rating")]
    public double Rating { get; set; }

    public static Accommodation GetExample()
    {
        return new Accommodation
        {
            Destination = new TravelLocation
            {
                Id = "20",
                Label = "New York",
            },
            Name = "Warwick New York",
            Room = "Double",
            Transportation = "Plane",
            Maintenance = "All inclusive",
            Rating = 4.5
        };
    }
}

public class StatsHttp
{
    [JsonProperty("directions")]
    public Direction[] Directions { get; set; }
    
    [JsonProperty("accommodations")]
    public Accommodation[] Accommodations { get; set; }
}
    