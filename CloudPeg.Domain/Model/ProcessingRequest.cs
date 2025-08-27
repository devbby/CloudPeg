using System.Text.Json.Serialization;
using FFMpegCore;

namespace CloudPeg.Domain.Model;

public class ProcessingRequest
{
    public FsResource Resource { get; }
    public ConversionTemplate Template { get; }
    public string ParentDir { get; }
    
    [JsonIgnore]
    public IMediaAnalysis MediaInfo { get; }
    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public double Progress { get; set; } = 0.0;

    public ProcessingRequest(FsResource resource, ConversionTemplate template, string parentDir, IMediaAnalysis mediaInfo)
    {
        Resource = resource;
        Template = template;
        ParentDir = parentDir;
        MediaInfo = mediaInfo;
    }
    
    [JsonIgnore]
    public CancellationToken Token
    {
        get
        {
            CancellationTokenSource ??= new CancellationTokenSource();
            return CancellationTokenSource.Token;
        }
    }
}