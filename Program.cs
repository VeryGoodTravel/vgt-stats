using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using RabbitMQ.Client.Exceptions;
using vgt_saga_flight;
using vgt_stats.Models;
using vgt_stats.StatsService;
using ILogger = NLog.ILogger;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
try
{
    builder.Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables().Build();
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}


try
{
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    var options = new NLogProviderOptions
    {
        AutoShutdown = true
    };
    options.Configure(builder.Configuration.GetSection("NLog"));
    builder.Logging.AddNLog(options);
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}

var logger = LogManager.GetCurrentClassLogger();

builder.Services.AddDbContext<StatDbContext>(options => options.UseNpgsql(SecretUtils.GetConnectionString(builder.Configuration, "DB_NAME_STATS", logger)));

var app = builder.Build();

var lf = app.Services.GetRequiredService<ILoggerFactory>();
logger.Info("Hello word");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

await using var scope = app.Services.CreateAsyncScope();
{
    await using var db = scope.ServiceProvider.GetService<StatDbContext>();
    {
        logger.Info("CAN CONNECT {v}" ,db.Database.CanConnect());
        logger.Info("-------------------------------------------------------------------------------- Before deletion ----------------------------------------------------------");
        //db.Database.EnsureDeleted();
        logger.Info("-------------------------------------------------------------------------------- Before migrations  ----------------------------------------------------------");
        await db.Database.MigrateAsync();
        logger.Info("-------------------------------------------------------------------------------- After migrations before disposes ----------------------------------------------------------");

    }
}

logger.Info("-------------------------------------------------------------------------------- OUTSIDE OF MIGRATIONS SUCCESFULLY DISPOSED ----------------------------------------------------------");

app.UseHttpsRedirection();

StatsService? hotelService = null;

try
{
    logger.Info("-------------------------------------------------------------------------------- Trying setup of the service ----------------------------------------------------------");

    hotelService = new StatsService(app.Configuration, lf);
}
catch (BrokerUnreachableException)
{
    logger.Info("-------------------------------------------------------------------------------- FAILED setup ----------------------------------------------------------");

    GracefulExit(app, logger, [hotelService]);
}
catch (ArgumentException)
{
    logger.Info("-------------------------------------------------------------------------------- Failed setup ----------------------------------------------------------");

    GracefulExit(app, logger, [hotelService]);
}

// app.MapPost("/flights", ([FromBody]FlightsRequestHttp request) =>
//     {
//         using var scope = app.Services.CreateAsyncScope();
//         using var db = scope.ServiceProvider.GetService<StatDbContext>();
//
//         logger.Info("fligths request date {v}, departure {d} and arrival {a}" ,
//             request.DepartureDateDt(), request.DepartureAirportCodes, request.ArrivalAirportCodes);
//         
//         var dbFlights = from flights in db.Flights
//             where request.ArrivalAirportCodes.Contains(flights.ArrivalAirport.AirportCode)
//                   && request.DepartureAirportCodes.Contains(flights.DepartureAirport.AirportCode)
//                   && flights.FlightTime.Date == request.DepartureDateDt()
//                   && (from m in db.Bookings
//                       where m.Flight == flights
//                       select m.Amount).Sum() + request.NumberOfPassengers < flights.Amount
//             select flights;
//
//         var results = dbFlights.Include(p => p.DepartureAirport).Include(p => p.ArrivalAirport).ToList();
//         logger.Info("fligths results count {c} and {v} " , results.Count,results);
//
//         return (from flight in results
//             where flight != null
//             select new FlightResponse
//             {
//                 Available = true,
//                 FlightId = flight.FlightDbId.ToString(),
//                 DepartureAirportCode = flight.DepartureAirport.AirportCode,
//                 DepartureAirportName = flight.DepartureAirport.AirportCity,
//                 ArrivalAirportCode = flight.ArrivalAirport.AirportCode,
//                 ArrivalAirportName = flight.ArrivalAirport.AirportCity,
//                 DepartureDate = flight.FlightTime.ToString(CultureInfo.InvariantCulture),
//                 Price = flight.Price
//             }).ToList();
//     })
//     .WithName("GetFlights")
//     .WithOpenApi();

app.MapGet("/PopularOffers", () =>
    {
        logger.Info("Received /PopularOffers request");
        
        using var scope = app.Services.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetService<StatDbContext>();

        var response = new StatsHttp
        {
            Directions = [Direction.GetExample()],
            Accommodations = [Accommodation.GetExample()]
        };

        return JsonConvert.SerializeObject(response);
    })
    .WithName("GetPopularOffers")
    .WithOpenApi();

app.MapGet("/OfferPopularity", ([FromBody]string offerid) =>
    {
        logger.Info("Received /OfferPopularity request: {oid}", offerid);
        
        using var scope = app.Services.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetService<StatDbContext>();

        return 1.ToString();
    })
    .WithName("IsOfferPopular")
    .WithOpenApi();

app.Run();

return;

// dispose objects and close connections
void GracefulExit(WebApplication wA, ILogger log, List<IDisposable?> toDispose)
{
    foreach (var obj in toDispose)
    {
        obj?.Dispose();
    }

    try
    {
        wA.Lifetime.StopApplication();
        AwaitAppStop(wA).Wait();
    }
    catch (ObjectDisposedException)
    {
        log.Info("App already disposed off");
    }

    throw new Exception("Kill the rest of the app");
    
    LogManager.Shutdown();
    Environment.Exit(0);
    throw new Exception("Kill the rest of the app");
}

async Task AwaitAppStop(WebApplication wA)
{
    await wA.StopAsync();
    await wA.DisposeAsync();
}


