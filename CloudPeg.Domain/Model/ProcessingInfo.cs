namespace CloudPeg.Domain.Model;

public class ProcessingInfo
{
    public ProcessingRequest ProcessRequest { get; set; }
    public DateTime Created { get; set; }
    public ProcessingStatus Status { get; set; }
    public string Info { get; set; }

    public ProcessingInfo()
    {
        
    }
    public ProcessingInfo(ProcessingRequest processRequest)
    {
        ProcessRequest = processRequest;
        Created = DateTime.Now;
        Status = ProcessingStatus.Enqueued;
    }

    public void SetError(string message)
    {
        Status = ProcessingStatus.Failed;
        Info = message;
    }
}