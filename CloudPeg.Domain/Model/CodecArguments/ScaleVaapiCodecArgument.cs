namespace CloudPeg.Domain.Model.CodecArguments;

public class ScaleVaapiCodecArgument : IScaleCodecArgument
{
    private string _argument;
    public int Width { get; set; }
    public int Height { get; set; }
    

    public string Type { get; set; } = "scale_vaapi";

    public string Argument
    {
        get => _argument + $",scale_vaapi=w={Width}:h={Height}";
        set => _argument = value;
    }

    public ScaleVaapiCodecArgument(int width, int height)
    {
        Width = width;
        Height = height;
        _argument = $"-vf format=nv12|yuv420p,hwupload";
    }

    public ScaleVaapiCodecArgument()
    {
        
    }
    
    public string GetScalelessArgument()
    {
        return _argument.Replace($",scale_vaapi=w={Width}:h={Height}", "").Replace($"scale_vaapi=w={Width}:h={Height},","");
    }

   
}