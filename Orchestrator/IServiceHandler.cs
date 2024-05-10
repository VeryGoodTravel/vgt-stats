using System.Threading.Channels;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator;

/// <summary>
/// Orchestrator handlers of the services in the SAGA architecture,
/// each service has its own handler.
/// </summary>
public interface IServiceHandler
{
    /// <summary>
    /// Replies received from the service
    /// </summary>
    public Channel<Message> Replies { get; }
    /// <summary>
    /// Requests to the service
    /// </summary>
    public Channel<Message> Requests { get; }
    /// <summary>
    /// Messages that need to be sent out to the queues
    /// </summary>
    public Channel<Message> Publish { get; }
    
    /// <summary>
    /// current request handled
    /// </summary>
    public Message CurrentRequest { get; }
    
    /// <summary>
    /// current reply handled
    /// </summary>
    public Message CurrentReply { get; }
    
    /// <summary>
    /// Task handling requests of the service
    /// </summary>
    public Task RequestsTask { get; set; }
    /// <summary>
    /// Task handling replies to this service
    /// </summary>
    public Task RepliesTask { get; set; }
    
    /// <summary>
    /// Cancellation token allowing a graceful exit of the class
    /// </summary>
    public CancellationToken Token { get; }
    
}