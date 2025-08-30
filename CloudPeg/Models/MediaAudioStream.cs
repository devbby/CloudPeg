using FFMpegCore;

namespace CloudPeg.Models;

public class MediaAudioStream
{
    public MediaAudioStream(AudioStream audio)
    {
        Index = audio.Index;
        Channels = audio.Channels;
        Codec = audio.CodecName;
        CodecName = audio.CodecLongName;
        ChannelLayout = audio.ChannelLayout;
        Language = audio.Language;

    }

    public string? Language { get; set; }

    public string ChannelLayout { get; set; }

    public string CodecName { get; set; }

    public string Codec { get; set; }

    public int Channels { get; set; }

    public int Index { get; set; }
}