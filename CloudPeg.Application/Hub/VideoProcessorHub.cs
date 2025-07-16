using CloudPeg.Application.Service;
using Microsoft.AspNetCore.SignalR;

namespace CloudPeg.Application.Hub;

public class VideoProcessorHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly IProcessingQueue _processingQueue;

    public VideoProcessorHub(IProcessingQueue processingQueue)
    {
        _processingQueue = processingQueue;
    }
    
    public async Task ConnectToVideoProcessor( )
    {
        var queue = _processingQueue.GetQueue();
        await Clients.Caller.SendAsync("VideoProcessorConnected", queue);
        
    }
}