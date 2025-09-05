using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using FFMpegCore;
using FFMpegCore.Enums;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingQueue : IProcessingQueue
{
    public List<ProcessingInfo> Queue { get; set; }


    public ProcessingQueue()
    {
        Queue = new  List<ProcessingInfo>() {};
    }
    
    public List<ProcessingInfo> GetQueue()
    {
        
        
        return Queue.ToList();
    }

    public async Task EnqueueForProcessing(ProcessingRequest processRequest)
    {  
        this.Queue.Add(new ProcessingInfo(processRequest));
    }

    public void RemoveFromQueue(Guid itemId)
    {
        var item = Queue.FirstOrDefault(x => x.ProcessRequest.Id == itemId);
        if (item != null)
        {
            Queue.Remove(item);
        }
    }

    public async Task CancelProcessing(Guid itemId)
    {
        var item = Queue.FirstOrDefault(x => x.ProcessRequest.Id == itemId);
        if (item is { ProcessRequest.CancellationTokenSource: not null })
        {
            await item.ProcessRequest.CancellationTokenSource.CancelAsync();
            item.Status = ProcessingStatus.Failed; 
            item.ProcessRequest.ProcessingEnded = DateTime.Now;
        }
    }
}