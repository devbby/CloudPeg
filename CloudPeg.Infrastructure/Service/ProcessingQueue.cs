using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingQueue : IProcessingQueue
{
    public Queue<ProcessingInfo> Queue { get; set; }


    public ProcessingQueue()
    {
        Queue = new  Queue<ProcessingInfo>()
        {
            
        };
        
        Queue.Enqueue(new  ProcessingInfo()
        {
            Status = ProcessingStatus.Enqueued
        });
    }
    
    
    public List<ProcessingInfo> GetQueue()
    {
        
        return Queue.ToList();
    }
}