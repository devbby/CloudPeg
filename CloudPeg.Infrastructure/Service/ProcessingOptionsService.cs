using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingOptionsService : IProcessingOptionsService
{
    public async Task<List<ConversionTemplate>> GetConversionTemplates()
    {
        
        // h264_amf             AMD AMF H.264 Encoder (codec h264)
        // h264_nvenc           NVIDIA NVENC H.264 encoder (codec h264)
        // h264_v4l2m2m         V4L2 mem2mem H.264 encoder wrapper (codec h264)
        // hap                  Vidvox Hap
        // hdr                  HDR (Radiance RGBE format) image
        // hevc_amf             AMD AMF HEVC encoder (codec hevc)
        // hevc_nvenc           NVIDIA NVENC hevc encoder (codec hevc)
        
        // hevc_v4l2m2m         V4L2 mem2mem HEVC encoder wrapper (codec hevc)
        // av1_nvenc            NVIDIA NVENC av1 encoder (codec av1)
        // av1_qsv              AV1 (Intel Quick Sync Video acceleration) (codec av1)
        // av1_amf              AMD AMF AV1 encoder (codec av1)
        // av1_vaapi            AV1 (VAAPI) (codec av1)
        
        
        return new List<ConversionTemplate>()
        {
            // hevc_qsv             HEVC (Intel Quick Sync Video acceleration) (codec hevc)
            new() {
                Name = "QSV HEVC Original",
                EncoderVideoCodec = "hevc_qsv",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "QSV"
            },
            
            // h264_qsv             H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (Intel Quick Sync Video acceleration) (codec h264)
            
            new() {
                Name = "QSV H264 Original",
                EncoderVideoCodec = "h264_qsv",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "QSV"
            },
            // h264_qsv             H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (Intel Quick Sync Video acceleration) (codec h264)
            
            new() {
                Name = "QSV H264 1080p",
                EncoderVideoCodec = "h264_qsv",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "QSV",
                HwDecoderArguments = new List<string>()
                { 
                    "-hwaccel_device /dev/dri/renderD128",
                },
                HwEncoderArguments =  new List<string>()
                { 
                    "-vf hwupload,scale_qsv=w=1920:h=1080:format=nv12", 
                    "-quality veryslow",
                    "-q:v 19"
                }
            },
            
            // hevc_vaapi    
            new() {
                Name = "VAAPI HEVC Original",
                EncoderVideoCodec = "hevc_vaapi",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<string>()
                {
                    "-hwaccel_output_format vaapi",
                    "-vaapi_device /dev/dri/renderD128"
                }
            },// hevc_vaapi    
            new() {
                Name = "VAAPI HEVC 1080p",
                EncoderVideoCodec = "hevc_vaapi",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<string>()
                {
                    "-hwaccel_output_format vaapi",
                    "-vaapi_device /dev/dri/renderD128"
                },
                HwEncoderArguments =  new List<string>()
                { 
                    "-vf hwupload,scale_vaapi=w=1920:h=1080:format=nv12", 
                    "-preset veryslow",
                    "-q:v 19"
                }
            },
            
            // h264_vaapi           H.264/AVC (VAAPI) (codec h264)
            new() {
                Name = "VAAPI H264 Original",
                EncoderVideoCodec = "h264_vaapi",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<string>()
                {
                    "-hwaccel_output_format vaapi",
                    "-vaapi_device /dev/dri/renderD128"
                }
            },
            
            new() {
                Name = "VAAPI H264 1080p",
                EncoderVideoCodec = "h264_vaapi",
                Size = "1080p",
                UseHardwareAcceleration = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<string>()
                {
                    // "-hwaccel_output_format vaapi",
                    // "-hwaccel vaapi",
                    "-vaapi_device /dev/dri/renderD128",
                },
                HwEncoderArguments =  new List<string>()
                {
                    // "-vf hwmap=derive_device=vaapi,scale_vaapi=w=1920:h=1080,hwdownload,format=yuv420p",
                    // "-vf hwupload,scale_vaapi=w=1920:h=1080:format=nv12",
                    "-vf hwupload,scale_vaapi=w=1920:h=1080:format=nv12",
                    // "-vf 'hwupload'",
                    // "-vf scale=w=1920:h=1080"
                    // "-vf scale_vaapi=w=1920:h=1080",
                    // "-vf hwdownload",
                    // "-vf format=yuv420p",
                    "-preset slow",
                    "-q:v 18"
                }
                
            },
            
            
            
            new() {
                Name = "CPU H264 Original",
                EncoderVideoCodec = "h264",
                Size = "1080p",
                UseHardwareAcceleration = false,
            },
            
            new() {
                Name = "CPU HEVC Original",
                EncoderVideoCodec = "hevc",
                Size = "1080p",
                UseHardwareAcceleration = false,
            },
            
            new() {
                Name = "NVENC HEVC Defaults",
                EncoderVideoCodec = "hevc_nvenc", 
                HwDevice = "CUDA",
                UseHardwareAcceleration = true,
            },
            
            new() {
                Name = "NVENC HEVC 1080p HQ, software scaling",
                EncoderVideoCodec = "hevc_nvenc", 
                HwDevice = "CUDA",
                UseHardwareAcceleration = true,
                
                HwDecoderArguments = new List<string>()
                {
                    // "-hwaccel_output_format cuda" // for scale_npp
                },
                
                HwEncoderArguments = new List<string>()
                {
                    // "-vf scale_npp=width=1920:height=1080", // must be enabled in ffmpeg build
                    "-vf scale=1920:1080",
                    "-preset p7",
                    "-rc:v constqp", // constant quality
                    "-qp 18"
                },
            },
            // new(){
            //     Name = "1080p H264 CPU",
            //     Codec = "h264",
            //     Size = "1080p",
            //     UseHardwareAcceleration = false
            // },

        };
    }
}