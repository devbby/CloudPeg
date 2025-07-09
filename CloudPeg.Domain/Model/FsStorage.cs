namespace CloudPeg.Domain.Model;

public class FsStorage
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Prefix => $"{Name?? """ """ }://";

    public FsStorage(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string GetRealPath(string path)
    {
        var cleanPath = path.Replace(Prefix, "");
        
        return System.IO.Path.Join(Path, cleanPath);
    }

    
}