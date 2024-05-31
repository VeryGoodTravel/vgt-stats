using Newtonsoft.Json;

namespace vgt_saga_flight.Models;

public class FlightsRequestHttp
{
    [JsonProperty("departure_airport_codes")]
    public List<string>? DepartureAirportCodes { get; set; }
    
    [JsonProperty("arrival_airport_codes")]
    public List<string>? ArrivalAirportCodes { get; set; }
    
    [JsonProperty("departure_date")]
    public string DepartureDate { get; set; }
    
    public  DateTime DepartureDateDt()
    {
        return DateTime.Parse(DepartureDate).ToUniversalTime().Date;
    }
    
    [JsonProperty("number_of_passengers")]
    public int NumberOfPassengers { get; set; }
}

public class FlightRequestHttp
{
    [JsonProperty("flight_id")]
    public string FlightId { get; set; }
    
    [JsonProperty("number_of_passengers")]
    public int NumberOfPassengers { get; set; }
    
    public FlightRequestHttp(string flightId, int numberOfPassengers)
    {
        FlightId = flightId;
        NumberOfPassengers = numberOfPassengers;
    }
}