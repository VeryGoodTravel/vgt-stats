using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NEventStore;
using NLog;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using vgt_saga_flight;
using vgt_saga_serialization;
using vgt_stats.Models;

namespace vgt_stats.StatsService;

/// <summary>
/// Saga Payment service;
/// handles all payments in the transaction.
/// </summary>
public class StatsService : IDisposable
{
    private readonly StatsQueueHandler _queues;
    private readonly Logger _logger;
    private readonly IConfiguration _config;
    private readonly Utils _jsonUtils;
    private readonly IStoreEvents _eventStore;
    
    private readonly Channel<Message> _payments;
    private readonly Channel<Message> _publish;
    private readonly StatsHandler _statsHandler;

    private readonly StatDbContext _writeDb;
    private readonly StatDbContext _readDb;
    
    private Task Publisher { get; set; }
    
    /// <summary>
    /// Allows tasks cancellation from the outside of the class
    /// </summary>
    public CancellationToken Token { get; } = new();

    /// <summary>
    /// Constructor of the StatsService class.
    /// Initializes StatsService object.
    /// Creates, initializes and opens connections to the database and rabbitmq
    /// based on configuration data present and handled by specified handling objects.
    /// Throws propagated exceptions if the configuration data is nowhere to be found.
    /// </summary>
    /// <param name="config"> Configuration with the connection params </param>
    /// <param name="lf"> Logger factory to use by the event store </param>
    /// <exception cref="ArgumentException"> Which variable is missing in the configuration </exception>
    /// <exception cref="BrokerUnreachableException"> Couldn't establish connection with RabbitMQ </exception>
    public StatsService(IConfiguration config, ILoggerFactory lf)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _config = config;

        _jsonUtils = new Utils(_logger);
        _payments = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions()
            { SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true });
        _logger.Info("-------------------------------------------------------------------------------- Creating service connection ----------------------------------------------------------");

        var connStr = SecretUtils.GetConnectionString(_config, "DB_NAME_STATS", _logger);
        var op = new DbContextOptions<StatDbContext>();
        //op.UseLoggerFactory(lf);
        _logger.Info("-------------------------------------------------------------------------------- creting write db context ----------------------------------------------------------");

        _writeDb = new StatDbContext(op, connStr);
        
        _logger.Info("-------------------------------------------------------------------------------- creating read db context ----------------------------------------------------------");
        _readDb = new StatDbContext(op, connStr);
        
        _logger.Info("-------------------------------------------------------------------------------- Created db connections ----------------------------------------------------------");
        
        
        _logger.Info("-------------------------------------------------------------------------------- Created db data ----------------------------------------------------------");
        _publish = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions()
            { SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true });
        
        Publisher = Task.Run(() => RabbitPublisher());
        

        
        _statsHandler = new StatsHandler(_payments, _publish, _writeDb, _readDb, _logger);

        _queues = new StatsQueueHandler(_config, _logger);
        
        _queues.AddStatsConsumer(SagaOrdersEventHandler);
    }

    /// <summary>
    /// Publishes made messages to the right queues
    /// </summary>
    private async Task RabbitPublisher()
    {
        _logger.Debug("-----------------Rabbit publisher starting");
        while (await _publish.Reader.WaitToReadAsync(Token))
        {
            _logger.Debug("-----------------Rabbit publisher message");
            var message = await _publish.Reader.ReadAsync(Token);
            _logger.Debug("Recieved message {msg} {id}", message.MessageType.ToString(), message.TransactionId);
            
            _queues.PublishToOrchestrator( _jsonUtils.Serialize(message));
        }
    }

    /// <summary>
    /// Event Handler that hooks to the event of the queue consumer.
    /// Handles incoming replies from the RabbitMQ and routes them to the appropriate tasks.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ea"></param>
    private void SagaOrdersEventHandler(object? sender, BasicDeliverEventArgs ea)
    {
        _logger.Debug("Received response | Tag: {tag}", ea.DeliveryTag);
        var body = ea.Body.ToArray();

        var reply = _jsonUtils.Deserialize(body);

        if (reply == null) return;

        var message = reply.Value;

        // send message reply to the appropriate task
        var result = _payments.Writer.TryWrite(message);
        
        if (result) _logger.Debug("Replied routed successfuly to {type} handler", message.MessageType.ToString());
        else _logger.Warn("Something went wrong in routing to {type} handler", message.MessageType.ToString());

        _queues.PublishTagResponse(ea, result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logger.Debug("Disposing");
        _queues.Dispose();
    }
}