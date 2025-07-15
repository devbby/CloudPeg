using CloudPeg.Application.Service;
using Microsoft.AspNetCore.SignalR;

namespace CloudPeg.Hubs;

public class VideoProcessorHub : Hub
{
    private readonly IProcessingQueue _processingQueue;

    public VideoProcessorHub(IProcessingQueue processingQueue)
    {
        _processingQueue = processingQueue;
    }
    
    public async Task ConnectToVideoProcessor( )
    {
        var queue = _processingQueue.GetQueue();
        await Clients.Caller.SendAsync("VideoProcessorConnected", queue, default);
        
    }
}