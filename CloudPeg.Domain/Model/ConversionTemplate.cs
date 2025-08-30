namespace CloudPeg.Domain.Model;

public class ConversionTemplate
{
    public string Name { get; set; }
    public string EncoderVideoCodec { get; set; }
    public string Size { get; set; }
    public bool UseHardwareAcceleration { get; set; }
    public string HwDevice { get; set; }
    
    public List<string> HwDecoderArguments { get; set; }
    
    public List<string> DecoderArguments { get; set; }
    public List<string> HwEncoderArguments { get; set; }
}