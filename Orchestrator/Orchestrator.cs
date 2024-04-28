using NLog;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace vgt_saga_orders.Orchestrator;

/// <summary>
/// Saga Orchestrator;
/// handles all saga transactions of user orders.
/// </summary>
public class Orchestrator : IDisposable
{
    private readonly RabbitMq _rabbit;
    private readonly Logger _logger;
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor of the Orchestrator class.
    /// Initializes Orchestrator object.
    /// Creates, initializes and opens connections to the database and rabbitmq
    /// based on configuration data present and handled by specified handling objects.
    /// Throws propagated exceptions if the configuration data is nowhere to be found.
    /// </summary>
    /// <param name="config"> Configuration with the connection params </param>
    /// <exception cref="ArgumentException"> Which variable is missing in the configuration </exception>
    /// <exception cref="BrokerUnreachableException"> Couldn't establish connection with RabbitMQ </exception>
    public Orchestrator(IConfiguration config)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _config = config;
        _rabbit = new RabbitMq(_config, _logger);
    }

    public void SagaRepliesEventHandler(object? sender, BasicDeliverEventArgs ea)
    {
        _logger.Debug("Received response | Tag: {tag}", ea.DeliveryTag);
        var body = ea.Body.ToArray();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _rabbit.Dispose();
    }
}