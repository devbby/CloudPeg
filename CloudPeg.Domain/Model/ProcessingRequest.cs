using System.Text.Json.Serialization;
using FFMpegCore;

namespace CloudPeg.Domain.Model;

public class ProcessingRequest
{
    public Guid Id { get; set; }
    public FsResource Resource { get; }
    public ConversionTemplate Template { get; }
    public string ParentDir { get; }
    
    [JsonIgnore]
    public IMediaAnalysis MediaInfo { get; }

    public List<int> VideoStreams { get; }
    public List<int> AudioStreams { get; }
    public List<int> SubtitleStreams { get; }

    public bool IsSample { get; set; }

    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public double Progress { get; set; } = 0.0;

    public ProcessingRequest(FsResource resource, ConversionTemplate template, string parentDir,
        IMediaAnalysis mediaInfo,
        List<int> videoStreams, List<int> audioStreams, List<int> subtitleStreams, bool isSample)
    {
        Id = Guid.NewGuid();
        Resource = resource;
        Template = template;
        ParentDir = parentDir;
        MediaInfo = mediaInfo;
        VideoStreams = videoStreams;
        AudioStreams = audioStreams;
        SubtitleStreams = subtitleStreams;
        IsSample = isSample;
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

    public DateTime ProcessingStarted { get; set; }
    public DateTime ProcessingEnded { get; set; }
}