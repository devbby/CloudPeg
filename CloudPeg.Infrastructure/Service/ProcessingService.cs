using CloudPeg.Application.Command;
using CloudPeg.Application.Service;
using MediatR;

namespace CloudPeg.Infrastructure.Service;


public class ProcessingService : IProcessingService
{
    private readonly IMediator _mediator;
    private readonly IProcessingQueue _processingQueue;
    private Timer? _timer;

    public ProcessingService(IMediator mediator, IProcessingQueue processingQueue)
    {
        _mediator = mediator;
        _processingQueue = processingQueue;
    }

    public async Task BeginProcessing()
    {
        _timer = new Timer(async state =>
            {
                await _mediator.Send(new NotifyProcessingStatusCommand(_processingQueue.GetQueue()));
            },
        null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
}