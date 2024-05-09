using System.Threading.Channels;
using MySqlConnector;
using NEventStore;
using NEventStore.Serialization.Json;
using NLog;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using vgt_saga_orders.Orchestrator.ServiceHandlers;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator;

/// <summary>
/// Saga Orchestrator;
/// handles all saga transactions of user orders.
/// </summary>
public class Orchestrator : IDisposable
{
    private readonly RepliesQueueHandler _queues;
    private readonly Logger _logger;
    private readonly Utils _jsonUtils;
    private readonly OrchOrderHandler _orchOrderHandler;
    private readonly IStoreEvents _eventStore;

    private readonly List<MessageType> _keys =
    [
        MessageType.OrderReply, MessageType.PaymentReply, MessageType.HotelReply, MessageType.FlightReply,
        MessageType.OrderRequest, MessageType.PaymentRequest, MessageType.HotelRequest, MessageType.FlightRequest
    ];

    private readonly Dictionary<MessageType, Channel<Message>> _repliesChannels = [];

    /// <summary>
    /// Constructor of the Orchestrator class.
    /// Initializes Orchestrator object.
    /// Creates, initializes and opens connections to the database and rabbitmq
    /// based on configuration data present and handled by specified handling objects.
    /// Throws propagated exceptions if the configuration data is nowhere to be found.
    /// </summary>
    /// <param name="config"> Configuration with the connection params </param>
    /// <param name="lf"> Logger factory to use for the Event Store </param>
    /// <exception cref="ArgumentException"> Which variable is missing in the configuration </exception>
    /// <exception cref="BrokerUnreachableException"> Couldn't establish connection with RabbitMQ </exception>
    public Orchestrator(IConfiguration config, ILoggerFactory lf)
    {
        _logger = LogManager.GetCurrentClassLogger();
        var config1 = config;

        _jsonUtils = new Utils(_logger);
        CreateChannels();

        var connStr = SecretUtils.GetConnectionString(config1, "DB_NAME_ORCH", _logger);
        
        _eventStore = Wireup.Init()
            .WithLoggerFactory(lf)
            .UsingInMemoryPersistence()
            .UsingSqlPersistence(MySqlConnectorFactory.Instance, connStr)
            .InitializeStorageEngine()
            .UsingJsonSerialization()
            .Compress()
            .Build();
        
        _orchOrderHandler = new OrchOrderHandler(_repliesChannels[MessageType.OrderRequest], _repliesChannels[MessageType.OrderReply], _eventStore, _logger);
        // TODO: Add tasks for each service

        _queues = new RepliesQueueHandler(config1, _logger);
        // TODO: Probably add consumer later after all inits
        _queues.AddRepliesConsumer(SagaRepliesEventHandler);
    }

    /// <summary>
    /// Event Handler that hooks to the event of the queue consumer.
    /// Handles incoming replies from the RabbitMQ and routes them to the appropriate tasks.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ea"></param>
    private void SagaRepliesEventHandler(object? sender, BasicDeliverEventArgs ea)
    {
        _logger.Debug("Received response | Tag: {tag}", ea.DeliveryTag);
        var body = ea.Body.ToArray();

        var reply = _jsonUtils.Deserialize(body);

        if (reply == null) return;

        var message = reply.Value;

        // send message reply to the appropriate task
        var result = _repliesChannels[message.MessageType].Writer.TryWrite(message);
        
        if (result) _logger.Debug("Replied routed successfuly to {type} handler", message.MessageType.ToString());
        else _logger.Warn("Something went wrong in routing to {type} handler", message.MessageType.ToString());

        _queues.PublishTagResponse(ea, result);
    }

    /// <summary>
    /// Creates async channels to send received messages with to the tasks handling them.
    /// Channels are stored in the dictionary MessageType - Channel
    /// </summary>
    private void CreateChannels()
    {
        _logger.Debug("Creating tasks message channels");
        foreach (var key in _keys)
        {
            _repliesChannels[key] = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions()
                { SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true });
        }

        _logger.Debug("Tasks message channels created");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logger.Debug("Disposing");
        _queues.Dispose();
    }
}