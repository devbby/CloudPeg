using System.Text.Json.Serialization;
using CloudPeg.Domain.Model.CodecArguments;

namespace CloudPeg.Domain.Model;

public class ConversionTemplate
{
    public string Name { get; set; }
    public string EncoderVideoCodec { get; set; }
    public string Size { get; set; }
    public bool UseHardwareDecoding { get; set; }
    public bool UseHardwareEncoding { get; set; }
    public string HwDevice { get; set; }
    
    public List<ICodecArgument> HwDecoderArguments { get; set; }
    
    public List<ICodecArgument> DecoderArguments { get; set; }
    public List<ICodecArgument> HwEncoderArguments { get; set; }
}