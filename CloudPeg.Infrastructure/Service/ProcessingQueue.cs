using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingQueue : IProcessingQueue
{
    public Queue<ProcessingInfo> Queue { get; set; }
    
    public Queue<ProcessingInfo> GetQueue()
    {
        
        return Queue;
    }
}