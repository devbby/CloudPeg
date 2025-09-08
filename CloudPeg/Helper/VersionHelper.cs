namespace CloudPeg.Helper;

public class VersionHelper
{
    
    public static string GetVersion()
    {
        return "CloudPeg " +
               ThisAssembly.Git.SemVer.Major + "." +
               ThisAssembly.Git.SemVer.Minor + "." +
               ThisAssembly.Git.Commits + "-" +
               ThisAssembly.Git.Branch + "+" +
               ThisAssembly.Git.Sha + "(" + (ThisAssembly.Git.IsDirty  ? "dirty" : "clean") + ")";

    }
    
}