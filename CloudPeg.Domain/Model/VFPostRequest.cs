namespace CloudPeg.Domain.Model;

public class VFPostRequest
{
    public string Item { get; set; }

    public List<FsResource> Items { get; set; }

    public string Name { get; set; }
    
    public string Content { get; set; }
 
}