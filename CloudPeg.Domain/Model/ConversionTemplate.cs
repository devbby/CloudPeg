namespace CloudPeg.Domain.Model;

public class ConversionTemplate
{
    public string Name { get; set; }
    public string Codec { get; set; }
    public string Size { get; set; }
    public bool UseHardwareAcceleration { get; set; }
}