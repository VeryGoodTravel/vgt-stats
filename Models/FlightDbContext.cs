using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace vgt_saga_flight.Models;

/// <inheritdoc />
public class FlightDbContext : DbContext
{
    private string _connectionString;
    
    /// <summary>
    /// Set of Database Airports entities mapped to AirportDb objects
    /// </summary>
    public DbSet<AirportDb> Airports { get; set; }
    /// <summary>
    /// Set of Database Flight entities mapped to FlightDb objects
    /// </summary>
    public DbSet<FlightDb> Flights { get; set; }
    /// <summary>
    /// Set of Database Booking entities mapped to Booking objects
    /// </summary>
    public DbSet<Booking> Bookings { get; set; }

    /// <inheritdoc />
    public FlightDbContext(DbContextOptions<FlightDbContext> options)
        : base(options)
    {
    }
    // {
    //     _connectionString = connectionString;
    // }
    //
    // /// <inheritdoc />
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    //     => options.UseNpgsql(_connectionString);
}

/// <summary>
/// Booking object representing an object from the database
/// </summary>
public class Booking()
{
    
    public int BookingId { get; set; }
    
    /// <summary>
    /// Flight booked
    /// </summary>
    public FlightDb Flight { get; set; } = new();
    
    /// <summary>
    /// Guid of the transaction that requested this booking
    /// </summary>
    public Guid TransactionId { get; set; }
    
    /// <summary>
    /// If the booking is temporary
    /// </summary>
    public int Temporary { get; set; } = -1;
    /// <summary>
    /// Time of the temporary booking
    /// </summary>
    public DateTime TemporaryDt { get; set; }
    
    public int Amount { get; set; }
}

/// <summary>
/// Flight type object representing an object from the database
/// </summary>
public class FlightDb()
{
    public int FlightDbId { get; set; }
    
    
    public int Price { get; set; }
    
    /// <summary>
    /// Amount of the seats offered by the flight
    /// </summary>
    public int Amount { get; set; } = -1;

    /// <summary>
    /// Time the flight takes place
    /// </summary>
    public DateTime FlightTime { get; set; }
    
    /// <summary>
    /// Arrival Airport the flight lands on
    /// </summary>
    public AirportDb ArrivalAirport { get; set; } = new();

    /// <summary>
    /// Departure Airport the flight starts from
    /// </summary>
    public AirportDb DepartureAirport { get; set; } = new();
    
};

/// <summary>
/// Flight object representing an object from the database
/// </summary>
public class AirportDb()
{
    public int AirportDbId { get; set; }
    /// <summary>
    /// Code of the airport from scrapper
    /// </summary>
    public string AirportCode { get; set; } = string.Empty;
    /// <summary>
    /// City the airport is in
    /// </summary>
    public string AirportCity { get; set; } = string.Empty;
    /// <summary>
    /// if the airport is a departure airport definition
    /// </summary>
    public bool IsDeparture { get; set; }
    
};