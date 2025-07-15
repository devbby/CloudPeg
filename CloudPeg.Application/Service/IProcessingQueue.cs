using CloudPeg.Domain.Model;

namespace CloudPeg.Application.Service;

public interface IProcessingQueue
{
    Queue<ProcessingInfo> GetQueue();
}