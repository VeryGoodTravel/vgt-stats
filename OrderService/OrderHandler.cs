using System.Threading.Channels;
using NEventStore;
using NLog;
using vgt_saga_serialization;

namespace vgt_saga_orders.OrderService;

/// <summary>
/// Handles saga orders beginning, end and failures
/// Creates the appropriate saga messages
/// Handles the data in messages
/// </summary>
public class OrderHandler
{
    /// <summary>
    /// Replies received from the orchestrator
    /// </summary>
    public Channel<Message> Replies { get; }
    /// <summary>
    /// Requests to the orchestrator
    /// </summary>
    public Channel<Message> Requests { get; }
    
    /// <summary>
    /// Messages that need to be sent out to the queues
    /// </summary>
    public Channel<Message> Publish { get; }
    
    /// <summary>
    /// current request handled
    /// </summary>
    public Message CurrentRequest { get; set; }
    
    /// <summary>
    /// current reply handled
    /// </summary>
    public Message CurrentReply { get; set; }
    private Logger _logger;
    
    private IStoreEvents EventStore { get; }
    
    /// <summary>
    /// Task of the requests handler
    /// </summary>
    public Task RequestsTask { get; set; }
    
    /// <summary>
    /// Task of the Replies handler
    /// </summary>
    public Task RepliesTask { get; set; }

    /// <summary>
    /// Token allowing tasks cancellation from the outside of the class
    /// </summary>
    public CancellationToken Token { get; } = new();

    /// <summary>
    /// Default constructor of the order handler class
    /// that handles data and prepares messages concerning saga orders beginning, end and failure
    /// </summary>
    /// <param name="replies"> Queue with the replies from the orchestrator </param>
    /// <param name="requests"> Queue with the requests to the orchestrator </param>
    /// <param name="publish"> Queue with messages that need to be published to RabbitMQ </param>
    /// <param name="eventStore"> EventStore for the event sourcing and CQRS </param>
    /// <param name="log"> logger to log to </param>
    public OrderHandler(Channel<Message> replies, Channel<Message> requests, Channel<Message> publish, IStoreEvents eventStore, Logger log)
    {
        _logger = log;
        Replies = replies;
        Requests = requests;
        Publish = publish;
        EventStore = eventStore;

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

            // TODO: do something with the request
            
            await Publish.Writer.WriteAsync(CurrentRequest, Token);
        }
    }
    
    private async Task HandleReplies()
    {
        while (await Replies.Reader.WaitToReadAsync(Token))
        {
            CurrentReply = await Replies.Reader.ReadAsync(Token);
            
            // TODO: do something with the reply
            
            await Publish.Writer.WriteAsync(CurrentReply, Token);
        }
    }
}