using CloudPeg.Application.Utility;

namespace CloudPeg.Application.Service;

public interface ISupportedCodecService
{
    void ScanForSupportedCodecs();

    List<FfmpegCodecUtility.Codec> GetSupportedEncoders();
    List<FfmpegCodecUtility.Codec> GetSupportedDecoders();
}