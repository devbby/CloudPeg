using System.Security.AccessControl;

namespace CloudPeg.Domain.Model;

public class FileFsResponse : FsResponse
{
    public required byte[] Bytes { get; set; }
    public required string Name { get; set; }
    
    public List<FsFolder> Folders { get; set; }
    
}