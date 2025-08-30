using CloudPeg.Domain.Model;

namespace CloudPeg.Application.Service;

public interface IProcessingQueue
{
    List<ProcessingInfo> GetQueue();
    Task EnqueueForProcessing(ProcessingRequest processRequest);
    void RemoveFromQueue(Guid itemId);
    Task CancelProcessing(Guid itemId);
}