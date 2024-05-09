using System.Threading.Channels;
using NEventStore;
using NLog;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator.ServiceHandlers;

/// <inheritdoc />
public class OrchOrderHandler : IServiceHandler
{
    
    public Channel<Message> Replies { get; }
    public Channel<Message> Requests { get; }
    
    public Channel<Message> Publish { get; }
    public Message CurrentRequest { get; set; }
    public Message CurrentReply { get; set; }
    
    public Task RequestsTask { get; set; }
    public Task RepliesTask { get; set; }
    
    private readonly Guid StreamId = Guid.NewGuid();
    private readonly Guid ReadStreamId = Guid.NewGuid();
    
    private IStoreEvents EventStore { get; }
    
    private Logger _logger;
    
    
    public CancellationToken Token { get; }

    public OrchOrderHandler(Channel<Message> replies, Channel<Message> requests, Channel<Message> publish, IStoreEvents eventStore, Logger log)
    {
        _logger = log;
        Replies = replies;
        Requests = requests;
        EventStore = eventStore;
        Publish = publish;
        
        _logger.Debug("Starting tasks handling the messages");
        RequestsTask = Task.Run(HandleRequests);
        RepliesTask = Task.Run(HandleReplies);
        _logger.Debug("Tasks handling the messages started");
    }

    private async Task HandleRequests()
    {
        while (await Requests.Reader.WaitToReadAsync(Token))
        {
            CurrentRequest = await Requests.Reader.ReadAsync(Token);
            // TODO: do something with the request and add it to the handle replies
        }
    }
    
    private async Task HandleReplies()
    {
        while (await Replies.Reader.WaitToReadAsync(Token))
        {
            CurrentReply = await Replies.Reader.ReadAsync(Token);
            
            if (CurrentReply.State == SagaState.Begin)
            {
                AppendToStream(CurrentRequest);
            }
        }
    }
    
    private void AppendToStream(Message mess)
    {
        using var stream = EventStore.OpenStream(mess.TransactionId, 0, int.MaxValue);
        
        stream.Add(new EventMessage { Body = mess });
        stream.CommitChanges(Guid.NewGuid());
    }
    
    private IEnumerable<Message> LoadFromStream(Guid transaction)
    {
        using var stream = EventStore.OpenStream(transaction);
        return stream.CommittedEvents.Select(p => (Message)p.Body);
    }
}