namespace CloudPeg.Domain.Model;

public class FsSubfoldersResponse : FsResponse
{
    public List<FsResource> Folders { get; set; }
}