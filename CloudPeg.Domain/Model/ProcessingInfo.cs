namespace CloudPeg.Domain.Model;

public class ProcessingInfo
{
    public DateTime Created { get; set; }
    public ProcessingStatus Status { get; set; }

    public ProcessingInfo()
    {
        Created = DateTime.Now;
    }
}