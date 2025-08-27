using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;

namespace CloudPeg.Infrastructure.Service;

public class ProcessingOptionsService : IProcessingOptionsService
{
    public async Task<List<ConversionTemplate>> GetConversionTemplates()
    {
        return new List<ConversionTemplate>()
        {

            new() {
                Name = "1080p H264 HW Accel",
                Codec = "h264",
                Size = "1080p",
                UseHardwareAcceleration = true
            },
            new(){
                Name = "1080p H264 CPU",
                Codec = "h264",
                Size = "1080p",
                UseHardwareAcceleration = false
            },

        };
    }
}