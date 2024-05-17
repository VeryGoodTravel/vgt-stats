using Newtonsoft.Json;

namespace vgt_saga_flight.Models;

public class AirportHttp
{
    [JsonProperty("airport_code")]
    public string AirportCode { get; set; }
    
    [JsonProperty("airport_name")]
    public string AirportName { get; set; }
}
    
public class DepartureAirports
{
    [JsonProperty("airports")]
    public List<AirportHttp> Airports { get; set; }
}
    
public class FlightHttp
{
    // only used when getting a single offer
    [JsonProperty("available")]
    public bool Available { get; set; }
    
    [JsonProperty("flight_id")]
    public string FlightId { get; set; }
    
    [JsonProperty("departure_airport_code")]
    public string DepartureAirportCode { get; set; }
    
    [JsonProperty("departure_airport_name")]
    public string DepartureAirportName { get; set; }
    
    [JsonProperty("arrival_airport_code")]
    public string ArrivalAirportCode { get; set; }
    
    [JsonProperty("arrival_airport_name")]
    public string ArrivalAirportName { get; set; }
    
    [JsonProperty("departure_date")]
    public string DepartureDate { get; set; }
    
    [JsonProperty("price")]
    public decimal Price { get; set; }
}
    
public class FlightResponse : FlightHttp {}
    