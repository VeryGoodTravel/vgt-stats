using System.Text;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using vgt_saga_serialization;
using vgt_saga_serialization.MessageBodies;
using vgt_stats.Models;

namespace vgt_stats.StatsService;

/// <summary>
/// Handles saga flight requests
/// Creates the appropriate saga messages
/// Handles the data in messages
/// </summary>
public class StatsHandler
{
    /// <summary>
    /// Requests from the orchestrator
    /// </summary>
    public Channel<Message> Requests { get; }
    
    /// <summary>
    /// Messages that need to be sent out to the queues
    /// </summary>
    public Channel<Message> Publish { get; }
    
    private Logger _logger;
    private Utils _jsonUtils;
    private readonly StatDbContext _writeDb;
    private readonly StatDbContext _readDb;
    
    /// <summary>
    /// Task of the requests handler
    /// </summary>
    public Task RequestsTask { get; set; }

    /// <summary>
    /// Token allowing tasks cancellation from the outside of the class
    /// </summary>
    public CancellationToken Token { get; } = new();
    
    private SemaphoreSlim _concurencySemaphore = new SemaphoreSlim(6, 6);
    
    private SemaphoreSlim _dbReadLock = new SemaphoreSlim(1, 1);
    private SemaphoreSlim _dbWriteLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Default constructor of the flight handler class
    /// handles data and prepares messages concerning saga flights availibity and booking
    /// </summary>
    /// <param name="requests"> Queue with the requests from the orchestrator </param>
    /// <param name="publish"> Queue with messages that need to be published to RabbitMQ </param>
    /// <param name="log"> logger to log to </param>
    public StatsHandler(Channel<Message> requests, Channel<Message> publish, StatDbContext writeDb, StatDbContext readDb, Logger log)
    {
        _logger = log;
        Requests = requests;
        Publish = publish;
        _writeDb = writeDb;
        _readDb = readDb;
        _jsonUtils = new Utils(_logger);

        _logger.Debug("Starting tasks handling the messages");
        RequestsTask = Task.Run(HandleStats);
        _logger.Debug("Tasks handling the messages started");
    }

    private async Task HandleStats()
    {
        while (await Requests.Reader.WaitToReadAsync(Token))
        {
            var message = await Requests.Reader.ReadAsync(Token);
            _logger.Debug("Received RMQ message: {mes}", message.ToString());
            var reply = (BackendReply)message.Body;
            _logger.Debug("Cast to BackendReply");
            var offerid = reply?.OfferId;
            _logger.Debug("Unwrap OfferId {oid}", offerid);
            
            await _concurencySemaphore.WaitAsync(Token);
            
            Task.Run(() => SaveStats(offerid), Token);
        }
    }

    private async Task SaveStats(string offerid)
    {
        _logger.Debug("SaveStats");
        
        var parts = offerid.Split('$');
        var hotel = parts[1].Replace("_", " ");
        var room = parts[3].Replace("_", " ");
        var from = parts[4].Replace("_", " ");
        var to = parts[14].Replace("_", " ");
        var maintenance = parts[15].Replace("_", " ");
        var transportation = parts[16].Replace("_", " ");
        
        _logger.Debug($"hotel = {hotel}, room = {room}, from = {from}, to = {to}, maintenance = {maintenance}, transportation = {transportation}",
            hotel, room, from, to, maintenance, transportation);
        
        await _dbReadLock.WaitAsync(Token);
        await using var transaction = await _readDb.Database.BeginTransactionAsync(Token);

        var popularDirection = _readDb.PopularDirections.Where(p => 
            p.From.Equals(from)
            && p.To.Equals(to));
        
        if (popularDirection.Any())
        {
            popularDirection.First().Count += 1;
        }
        else
        {
            _readDb.PopularDirections.Add(new PopularDirection
            {
                From = from,
                To = to,
                Count = 1
            });
        }

        var popularHotel = _readDb.PopularHotels.Where(p =>
            p.Name.Equals(hotel) 
            && p.Room.Equals(room) 
            && p.Maintenance.Equals(maintenance) 
            && p.Transportation.Equals(transportation));

        if (popularHotel.Any())
        {
            popularHotel.First().Count += 1;
        }
        else
        {
            _readDb.PopularHotels.Add(new PopularHotel
            {
                Name = hotel,
                Room = room,
                Maintenance = maintenance,
                Transportation = transportation,
                City = to,
                Count = 1
            });
        }
        
        await transaction.CommitAsync(Token);
        await _readDb.SaveChangesAsync(Token);
        
        _dbReadLock.Release();
        _concurencySemaphore.Release();
    }
}