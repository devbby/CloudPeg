namespace CloudPeg.Domain.Model;

public class FsBadResponse(string message) : FsResponse
{
    public string Message { get; } = message;

    public bool Status { get; set; } = false;
}