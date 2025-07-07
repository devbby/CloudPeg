namespace CloudPeg.Domain.Model;

public class FsResponse
{
    public string Adapter { get; set; }
    public List<string> Storages { get; set; }
    public string Dirname { get; set; }
    public List<FsResource> Files { get; set; }
}