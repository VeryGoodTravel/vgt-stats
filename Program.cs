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

app.MapGet("/PopularOffers", () =>
    {
        logger.Info("Received /PopularOffers request");
        
        using var scope = app.Services.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetService<StatDbContext>();

        var directions = db.PopularDirections.OrderByDescending(d => d.Count).Take(12).ToArray().Select(d => new Direction
        {
            Origin = d.From.Replace(" ", "_"),
            Destination = d.To.Replace(" ", "_")
        }).ToArray();

        var accommodations = db.PopularHotels.OrderByDescending(h => h.Count).Take(12).ToArray().Select(h => new Accommodation
        {
            Destination = h.City.Replace(" ", "_"),
            Maintenance = h.Maintenance.Replace(" ", "_"),
            Name = h.Name.Replace(" ", "_"),
            Room = h.Room.Replace(" ", "_"),
            Transportation = h.Transportation.Replace(" ", "_")
        }).ToArray();

        var response = new StatsHttp
        {
            Directions = directions,
            Accommodations = accommodations
        };

        return JsonConvert.SerializeObject(response);
    })
    .WithName("GetPopularOffers")
    .WithOpenApi();

app.MapPost("/OfferPopularity", ([FromBody]string offer_id) =>
    {
        logger.Info("Received /OfferPopularity request: {oid}", offer_id);
        
        using var scope = app.Services.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetService<StatDbContext>();
        
        var parts = offer_id.Split('$');
        var hotel = parts[1].Replace("_", " ");

        var popularity = db.PopularHotels.Where(h => h.Name.Equals(hotel)).Sum(h => h.Count);
        logger.Info("Calculated offer popularity: |{p}|", popularity);

        return popularity.ToString();
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


