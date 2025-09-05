using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using CloudPeg.Domain.Model.CodecArguments;

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
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "QSV"
            },
            
            // h264_qsv             H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (Intel Quick Sync Video acceleration) (codec h264)
            
            new() {
                Name = "QSV H264 Original",
                EncoderVideoCodec = "h264_qsv",
                Size = "1080p",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "QSV"
            },
            // h264_qsv             H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (Intel Quick Sync Video acceleration) (codec h264)
            
            new() {
                Name = "QSV H264 1080p",
                EncoderVideoCodec = "h264_qsv",
                Size = "1080p",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "QSV",
                HwDecoderArguments = new List<ICodecArgument>()
                { 
                    new GenericCodecArgument("-hwaccel_device /dev/dri/renderD128")
                    ,
                },
                HwEncoderArguments =  new List<ICodecArgument>()
                { 
                    new ScaleQsvCodecArgument(1920, 1080), 
                    new GenericCodecArgument("-quality veryslow"),
                    new GenericCodecArgument("-q:v 19")
                }
            },
            
            // hevc_vaapi    
            new() {
                Name = "VAAPI HEVC Original",
                EncoderVideoCodec = "hevc_vaapi",
                Size = "1080p",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                    new GenericCodecArgument("-hwaccel_output_format vaapi"),
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                }
            },// hevc_vaapi    
            new() {
                Name = "VAAPI HEVC 1080p",
                EncoderVideoCodec = "hevc_vaapi", 
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                    new GenericCodecArgument("-hwaccel_output_format vaapi"),
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                },
                HwEncoderArguments =  new List<ICodecArgument>()
                { 
                    new ScaleVaapiCodecArgument(1920, 1080), 
                    new GenericCodecArgument("-preset veryslow"),
                    new GenericCodecArgument("-q:v 19"),
                }
            },
             // hevc_vaapi    
            new() {
                Name = "Soft Decode + VAAPI HEVC 1080p",
                EncoderVideoCodec = "hevc_vaapi", 
                UseHardwareDecoding = false,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                   
                },
                HwEncoderArguments =  new List<ICodecArgument>()
                { 
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                    new ScaleVaapiCodecArgument(1920, 1080), 
                    new GenericCodecArgument("-preset veryslow"),
                    new GenericCodecArgument("-q:v 18"),
                }
            },
            
            // h264_vaapi           H.264/AVC (VAAPI) (codec h264)
            new() {
                Name = "VAAPI H264 Original",
                EncoderVideoCodec = "h264_vaapi",
                Size = "1080p",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                    new GenericCodecArgument("-hwaccel_output_format vaapi"),
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                }
            },
            
            new() {
                Name = "VAAPI H264 1080p",
                EncoderVideoCodec = "h264_vaapi",
                Size = "1080p",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                    // "-hwaccel_output_format vaapi",
                    // "-hwaccel vaapi",
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                },
                HwEncoderArguments =  new List<ICodecArgument>()
                {
                    // "-vf hwmap=derive_device=vaapi,scale_vaapi=w=1920:h=1080,hwdownload,format=yuv420p",
                    // "-vf hwupload,scale_vaapi=w=1920:h=1080:format=nv12",
                    new GenericCodecArgument("-vf hwupload,scale_vaapi=w=1920:h=1080:format=nv12"), 
                    // "-vf 'hwupload'",
                    // "-vf scale=w=1920:h=1080"
                    // "-vf scale_vaapi=w=1920:h=1080",
                    // "-vf hwdownload",
                    // "-vf format=yuv420p",
                    new GenericCodecArgument("-preset slow"),
                    new GenericCodecArgument("-q:v 18"),
                }
                
            },
            new() {
                Name = "Soft Decode + VAAPI H264 1080p",
                EncoderVideoCodec = "h264_vaapi", 
                UseHardwareDecoding = false,
                UseHardwareEncoding = true,
                HwDevice = "VAAPI",
                HwDecoderArguments = new List<ICodecArgument>()
                {
                   
                },
                HwEncoderArguments =  new List<ICodecArgument>()
                { 
                    new GenericCodecArgument("-vaapi_device /dev/dri/renderD128"),
                    new ScaleVaapiCodecArgument(1920, 1080),
                    new GenericCodecArgument("-preset slow"),
                    new GenericCodecArgument("-q:v 18"),
                }
            },
            
            
            new() {
                Name = "CPU H264 Original",
                EncoderVideoCodec = "h264",
                Size = "1080p",
                UseHardwareDecoding = false,
                UseHardwareEncoding = false,
            },
            
            new() {
                Name = "CPU HEVC Original",
                EncoderVideoCodec = "hevc",
                Size = "1080p",
                UseHardwareDecoding = false,
                UseHardwareEncoding = false,
            },
            
            new() {
                Name = "NVENC HEVC Defaults",
                EncoderVideoCodec = "hevc_nvenc", 
                HwDevice = "CUDA",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
            },
            
            new() {
                Name = "NVENC HEVC 1080p HQ, software scaling",
                EncoderVideoCodec = "hevc_nvenc", 
                HwDevice = "CUDA",
                UseHardwareDecoding = true,
                UseHardwareEncoding = true,
                HwDecoderArguments = new List<ICodecArgument>()
                {
                    // "-hwaccel_output_format cuda" // for scale_npp
                },
                
                HwEncoderArguments = new List<ICodecArgument>()
                {
                    // "-vf scale_npp=width=1920:height=1080", // must be enabled in ffmpeg build
                    new GenericCodecArgument("-vf scale=1920:1080"),
                    new GenericCodecArgument("-preset p7"),
                    new GenericCodecArgument("-rc:v constqp"), // constant quality
                    new GenericCodecArgument("-qp 18"),
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