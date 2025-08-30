using FFMpegCore;

namespace CloudPeg.Models;

public class MediaSubtitleStream
{
    public MediaSubtitleStream(SubtitleStream sub)
    {
        Index = sub.Index;
        Language = sub.Language;
        Codec = sub.CodecName;
        CodecName = sub.CodecLongName;

    }

    public string CodecName { get; set; }

    public string Codec { get; set; }

    public string? Language { get; set; }

    public int Index { get; set; }
}