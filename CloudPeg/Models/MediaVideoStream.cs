using FFMpegCore;

namespace CloudPeg.Models;

public class MediaVideoStream 
{
    public int Index { get; set; }
    
    public string CodecName { get; set; }

    public string Codec { get; set; }
    
    public string? Language { get; set; }

    public double FrameRate { get; set; }

    public int Height { get; set; }

    public int Width { get; set; }
    
    
    public MediaVideoStream(VideoStream video)
    {
        Index = video.Index;
        Codec = video.CodecName;
        CodecName = video.CodecLongName;
        Width = video.Width;
        Height = video.Height;
        FrameRate = video.FrameRate;
        Language = video.Language;
    }

    
}