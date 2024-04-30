using System.Threading.Channels;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator;

public interface IServiceHandler
{
    public Channel<Message> Replies { get; }
    public Channel<Message> Requests { get; }
    public Message CurrentRequest { get; }
    public Message CurrentReply { get; }
}