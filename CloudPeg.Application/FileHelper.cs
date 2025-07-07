namespace CloudPeg.Application;

public class FileHelper
{
    public static long GetSize(string path)
    {

        if (Directory.Exists(path)) return 0;
        
        FileInfo fi = new FileInfo(path);
        
        
        return fi.Length;
    }
}