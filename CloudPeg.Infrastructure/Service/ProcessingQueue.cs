using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using FFMpegCore;
using FFMpegCore.Enums;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingQueue : IProcessingQueue
{
    public Queue<ProcessingInfo> Queue { get; set; }


    public ProcessingQueue()
    {
        Queue = new  Queue<ProcessingInfo>()
        {
            
        };
        
        
    }
    
    
    public List<ProcessingInfo> GetQueue()
    {
        
        
        return Queue.ToList();
    }

    public async Task EnqueueForProcessing(ProcessingRequest processRequest)
    {  
        this.Queue.Enqueue(new ProcessingInfo(processRequest));
         
    }
}