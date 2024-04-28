using System.Threading.Channels;
using NLog;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using vgt_saga_orders.Orchestrator;
using vgt_saga_serialization;

namespace vgt_saga_orders.OrderService;

/// <summary>
/// Saga Orchestrator;
/// handles all saga transactions of user orders.
/// </summary>
public class OrderService : IDisposable
{
    private readonly OrderQueueHandler _queues;
    private readonly Logger _logger;
    private readonly IConfiguration _config;
    private readonly Utils _jsonUtils;

    private readonly List<MessageType> _keys = [MessageType.OrderReply, MessageType.OrderRequest];
    private readonly Dictionary<MessageType, Channel<Message>> _repliesChannels = [];

    /// <summary>
    /// Constructor of the OrderService class.
    /// Initializes OrderService object.
    /// Creates, initializes and opens connections to the database and rabbitmq
    /// based on configuration data present and handled by specified handling objects.
    /// Throws propagated exceptions if the configuration data is nowhere to be found.
    /// </summary>
    /// <param name="config"> Configuration with the connection params </param>
    /// <exception cref="ArgumentException"> Which variable is missing in the configuration </exception>
    /// <exception cref="BrokerUnreachableException"> Couldn't establish connection with RabbitMQ </exception>
    public OrderService(IConfiguration config)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _config = config;

        _jsonUtils = new Utils(_logger);
        CreateChannels();
        // TODO: Add tasks for each service

        _queues = new OrderQueueHandler(_config, _logger);
        // TODO: Probably add consumer later after all inits
        _queues.AddRepliesConsumer(SagaOrdersEventHandler);
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