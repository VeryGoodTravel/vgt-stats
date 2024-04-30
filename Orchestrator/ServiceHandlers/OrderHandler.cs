using System.Threading.Channels;
using NLog;
using vgt_saga_serialization;

namespace vgt_saga_orders.Orchestrator.ServiceHandlers;

/// <inheritdoc />
public class OrderHandler : IServiceHandler
{
    public Channel<Message> Replies { get; }
    public Channel<Message> Requests { get; }
    public Message CurrentRequest { get; set; }
    public Message CurrentReply { get; set; }
    
    public Task RequestsTask { get; set; }
    public Task RepliesTask { get; set; }
    
    private Logger _logger;
    
    public CancellationToken Token { get; }

    public OrderHandler(Channel<Message> replies, Channel<Message> requests, Logger log)
    {
        _logger = log;
        Replies = replies;
        Requests = requests;
        
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
        }
    }
}