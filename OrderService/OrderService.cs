using NLog;
using vgt_saga_orders.Orchestrator;

namespace vgt_saga_orders.OrderService;

public class OrderService : IDisposable
{
    private readonly RabbitMq _rabbit;
    private readonly Logger _logger;

    public OrderService(IConfiguration config)
    {
        _logger = LogManager.GetCurrentClassLogger();

        // TODO: Add a new handler
        _rabbit = new RabbitMq(config, _logger);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _rabbit.Dispose();
    }
}