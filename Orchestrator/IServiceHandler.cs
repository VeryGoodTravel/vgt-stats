using System.Threading.Channels;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator;

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
}