using CloudPeg.Application.Hub;
using CloudPeg.Domain.Model;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CloudPeg.Application.Command;

public class NotifyProcessingStatusCommand : IRequest
{
    public List<ProcessingInfo> ProcessingInfos { get; }

    public NotifyProcessingStatusCommand(List<ProcessingInfo> processingInfos)
    {
        ProcessingInfos = processingInfos;
    }
}

public class NotifyProcessingStatusCommandHandler : IRequestHandler<NotifyProcessingStatusCommand>
{
    private readonly IHubContext<VideoProcessorHub> _videoProcessorHub;

    public NotifyProcessingStatusCommandHandler(IHubContext<VideoProcessorHub> videoProcessorHub)
    {
        _videoProcessorHub = videoProcessorHub;
    }
    
    public Task Handle(NotifyProcessingStatusCommand request, CancellationToken cancellationToken)
    {
        return _videoProcessorHub.Clients.All.SendAsync(
            nameof(VideoProcessorHubEvents.VideoProcessorStatusNotified), 
            request.ProcessingInfos.OrderByDescending(x=>x.Created), 
            cancellationToken: cancellationToken);
    }
}