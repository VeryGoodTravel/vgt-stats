using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace vgt_stats.Models;

/// <inheritdoc />
public class StatDbContext : DbContext
{
    private string _connectionString = "";
    
    public DbSet<PopularHotel> PopularHotels { get; set; }
    
    public DbSet<PopularDirection> PopularDirections { get; set; }

    /// <inheritdoc />
    public StatDbContext(DbContextOptions<StatDbContext> options)
        : base(options)
    {
    }
    
    /// <inheritdoc />
    public StatDbContext(DbContextOptions<StatDbContext> options, string conn)
        : base(options)
    {
        _connectionString = conn;
    }
    // {
    //     _connectionString = connectionString;
    // }
    //
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!_connectionString.IsNullOrEmpty())
        {
            options.UseNpgsql(_connectionString);
        }
        
        base.OnConfiguring(options);
    }
}

public class PopularHotel()
{
    public string Name { get; set; }
    
    public string Room { get; set; }
    
    public string Maintenance { get; set; }
    
    public string Transportation { get; set; }
    
    public string City { get; set; }
    
    public int Count { get; set; }
}

public class PopularDirection()
{
    public string From { get; set; }
    
    public string To { get; set; }

    public int Count { get; set; } = 0;
}
