namespace CloudPeg.Domain.Model;

public enum ProcessingStatus
{
    Enqueued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}