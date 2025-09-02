using CloudPeg.Application.Service;
using CloudPeg.Application.Utility;

namespace CloudPeg.Infrastructure.Service;

public class SupportedCodecService : ISupportedCodecService
{
    public List<FfmpegCodecUtility.Codec> Encoders { get; set; }
    public List<FfmpegCodecUtility.Codec> Decoders { get; set; }
    public void ScanForSupportedCodecs()
    {
        var codecs = new List<string>(){"h264", "h265", "hevc", "av1", "vp9", "vaapi", "qsv"};
        Decoders = FfmpegCodecUtility.GetSupportedDecoders()
            .Where(x=> x.IsAudio || x.IsSubtitle || codecs.Any(y=> x.Name.Contains(y)  )  ) 
            .Where(x=> FfmpegCodecUtility.IsCodecActuallySupported(x) ).ToList();
        Encoders = FfmpegCodecUtility.GetSupportedEncoders()
            .Where(x=> x.IsAudio || x.IsSubtitle || codecs.Any(y=> x.Name.Contains(y)  ))
            .Where(x=> FfmpegCodecUtility.IsCodecActuallySupported(x) ).ToList();
        
    }

    public List<FfmpegCodecUtility.Codec> GetSupportedEncoders()
    {
        return Encoders;
    }

    public List<FfmpegCodecUtility.Codec> GetSupportedDecoders()
    {
        return Decoders;
    }
}