using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NLog;
using vgt_saga_flight.Models;
using vgt_saga_serialization;
using vgt_saga_serialization.MessageBodies;

namespace vgt_saga_flight.FlightService;

/// <summary>
/// Handles saga flight requests
/// Creates the appropriate saga messages
/// Handles the data in messages
/// </summary>
public class FlightHandler
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

    private readonly FlightDbContext _writeDb;
    private readonly FlightDbContext _readDb;
    
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
    public FlightHandler(Channel<Message> requests, Channel<Message> publish, FlightDbContext writeDb, FlightDbContext readDb, Logger log)
    {
        _logger = log;
        Requests = requests;
        Publish = publish;
        _writeDb = writeDb;
        _readDb = readDb;

        _logger.Debug("Starting tasks handling the messages");
        RequestsTask = Task.Run(HandleFlights);
        _logger.Debug("Tasks handling the messages started");
    }

    private async Task HandleFlights()
    {
        while (await Requests.Reader.WaitToReadAsync(Token))
        {
            var message = await Requests.Reader.ReadAsync(Token);

            await _concurencySemaphore.WaitAsync(Token);

            _ = message.State switch
            {
                SagaState.Begin => Task.Run(() => TempBookFlight(message), Token),
                SagaState.PaymentAccept => Task.Run(() => BookFlight(message), Token),
                SagaState.FlightTimedRollback => Task.Run(() => TempRollback(message), Token),
                _ => null
            };
        }
    }

    private async Task TempBookFlight(Message message)
    {
        if (message.MessageType != MessageType.FlightRequest || message.Body == null) return;
        var requestBody = (FlightRequest)message.Body;
        
        await _dbWriteLock.WaitAsync(Token);
        await using var transaction = await _writeDb.Database.BeginTransactionAsync(Token);

        var flights = await _writeDb.Flights
            .Include(p => p.ArrivalAirport)
            .Include(p => p.DepartureAirport)
            .FirstOrDefaultAsync(p => p.DepartureAirport.AirportCity.Equals(requestBody.CityFrom)
                                      && p.ArrivalAirport.AirportCity.Equals(requestBody.CityTo)
                                      && p.Amount - requestBody.PassangerCount >= 0
                                      && p.FlightTime.Equals(requestBody.BookFrom));
        
        if (flights == null)
        {
            await transaction.RollbackAsync(Token);

            message.MessageId += 1;
            message.MessageType = MessageType.PaymentRequest;
            message.State = SagaState.FlightTimedFail;
            message.Body = new PaymentRequest();
            
            await Publish.Writer.WriteAsync(message, Token);
            _dbWriteLock.Release();
            _concurencySemaphore.Release();
            return;
        }
        

        _writeDb.Bookings.Add(new Booking
        {
            Flight = flights,
            TransactionId = message.TransactionId,
            Temporary = 1,
            TemporaryDt = DateTime.Now,
            Amount = flights.Amount
        });
        flights.Amount-=requestBody.PassangerCount;
        await _readDb.SaveChangesAsync(Token);
        await transaction.CommitAsync(Token);

        message.MessageId += 1;
        message.MessageType = MessageType.PaymentRequest;
        message.State = SagaState.HotelTimedAccept;
        message.Body = new PaymentRequest();
        message.CreationDate = DateTime.Now;
        
        await Publish.Writer.WriteAsync(message, Token);
        _dbWriteLock.Release();
        _concurencySemaphore.Release();

    }
    
    private async Task TempRollback(Message message)
    {
        if (message.MessageType != MessageType.FlightRequest || message.Body == null) return;
        var requestBody = (FlightRequest)message.Body;
        
        await _dbReadLock.WaitAsync(Token);
        await using var transaction = await _readDb.Database.BeginTransactionAsync(Token);

        var booked = _readDb.Bookings
            .Where(p => p.TransactionId == message.TransactionId);

        if (booked.Any())
        {
            booked.First().Flight.Amount+=booked.First().Amount;
            await booked.ExecuteDeleteAsync(Token);
            
        }
        await _readDb.SaveChangesAsync(Token);
        await transaction.CommitAsync(Token);
        
        message.MessageType = MessageType.OrderReply;
        message.MessageId += 1;
        message.State = SagaState.FlightTimedRollback;
        message.Body = new FlightReply();
        message.CreationDate = DateTime.Now;
        
        await Publish.Writer.WriteAsync(message, Token);
        _dbReadLock.Release();
        _concurencySemaphore.Release();
    }
    
    private async Task BookFlight(Message message)
    {
        if (message.MessageType != MessageType.FlightRequest || message.Body == null) return;
        var requestBody = (FlightRequest)message.Body;
        
        await _dbReadLock.WaitAsync(Token);
        await using var transaction = await _readDb.Database.BeginTransactionAsync(Token);

        var booked = _readDb.Bookings
            .Where(p => p.TransactionId == message.TransactionId);

        if (booked.Any())
        {
            var booking = booked.FirstOrDefault();
            if (booking != null)
            {
                booking.Temporary = 0;
                await _readDb.SaveChangesAsync(Token);
                await transaction.CommitAsync(Token);
                
                message.MessageType = MessageType.OrderReply;
                message.MessageId += 1;
                message.State = SagaState.FlightFullAccept;
                message.Body = new FlightReply();
                message.CreationDate = DateTime.Now;
        
                await Publish.Writer.WriteAsync(message, Token);
                _dbReadLock.Release();
                _concurencySemaphore.Release();
            }
        }
        await transaction.RollbackAsync(Token);
        
        message.MessageType = MessageType.OrderReply;
        message.MessageId += 1;
        message.State = SagaState.FlightFullFail;
        message.Body = new FlightReply();
        message.CreationDate = DateTime.Now;
        
        await Publish.Writer.WriteAsync(message, Token);
        _dbReadLock.Release();
        _concurencySemaphore.Release();
    }
}