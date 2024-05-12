using Microsoft.EntityFrameworkCore;

namespace vgt_saga_hotel.Models;

/// <inheritdoc />
public class HotelDbContext : DbContext
{
    private string _connectionString;
    
    /// <summary>
    /// Set of Database Hotel entities mapped to HotelDb objects
    /// </summary>
    public DbSet<HotelDb> Hotels { get; set; }
    /// <summary>
    /// Set of Database Room entities mapped to RoomDb objects
    /// </summary>
    public DbSet<RoomDb> Rooms { get; set; }
    /// <summary>
    /// Set of Database Booking entities mapped to Booking objects
    /// </summary>
    public DbSet<Booking> Bookings { get; set; }

    /// <inheritdoc />
    public HotelDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(_connectionString);
}

/// <summary>
/// Booking object representing an object from the database
/// </summary>
public class Booking()
{
    /// <summary>
    /// Hotel the booking concerns
    /// </summary>
    public HotelDb Hotel { get; set; } = new();

    /// <summary>
    /// Room booked
    /// </summary>
    public RoomDb Room { get; set; } = new();
    
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
    /// <summary>
    /// Time the rooms are booked from
    /// </summary>
    public DateTime BookFrom { get; set; }
    /// <summary>
    /// Time the rooms are booked to
    /// </summary>
    public DateTime BookTo { get; set; }
}

/// <summary>
/// Room type object representing an object from the database
/// </summary>
public class RoomDb()
{
    /// <summary>
    /// Amount of the rooms offered by the hotel
    /// </summary>
    public int Amount { get; set; } = -1;

    /// <summary>
    /// Name of the room
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Minimal amount of people required to rent this room
    /// </summary>
    public int MinPeople { get; set; } = -1;
    /// <summary>
    /// Maximal amount of people allowed to rent this room
    /// </summary>
    public int MaxPeople { get; set; } = -1;
    /// <summary>
    /// Minimal amount of adults required to rent this room
    /// </summary>
    public int MinAdults { get; set; } = -1;
    /// <summary>
    /// Maximal amount of adults allowed to rent this room
    /// </summary>
    public int MaxAdults { get; set; } = -1;
    
    /// <summary>
    /// 18 y.o. children min count in a room
    /// </summary>
    public int MinChildren { get; set; } = -1;
    
    /// <summary>
    /// 18 y.o. children max count in a room
    /// </summary>
    public int MaxChildren { get; set; } = -1;
    
    /// <summary>
    /// Maximal amount of 10 y.o. children allowed to rent this room
    /// </summary>
    public int Max10yo { get; set; } = -1;
    /// <summary>
    /// Maximal amount of children younger than 3 y.o. allowed to rent this room
    /// </summary>
    public int MaxLesserChildren { get; set; } = -1;
};

/// <summary>
/// Hotel object representing an object from the database
/// </summary>
public class HotelDb()
{
    /// <summary>
    /// Name of the hotel from scrapper
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Country the hotel is in
    /// </summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>
    /// City the hotel is assigned to
    /// </summary>
    public string City { get; set; } = string.Empty;
    /// <summary>
    /// Code of the airport assigned to the hotel
    /// </summary>
    public string AirportCode { get; set; } = string.Empty;
    /// <summary>
    /// Name of the airport assigned to the hotel
    /// </summary>
    public string AirportName { get; set; } = string.Empty;

    /// <summary>
    /// List of room types available in the hotel
    /// </summary>
    public List<RoomDb> Rooms { get; set; } = [];
};