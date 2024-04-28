using NLog;
using RabbitMQ.Client.Exceptions;
using vgt_saga_orders.Orchestrator;

namespace vgt_saga_orders.OrderService;

/// <summary>
/// Saga Orchestrator;
/// handles all saga transactions of user orders.
/// </summary>
public class OrderService : IDisposable
{
    private readonly OrderQueueHandler _queues;
    private readonly Logger _logger;

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

        _queues = new OrderQueueHandler(config, _logger);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _queues.Dispose();
    }
}